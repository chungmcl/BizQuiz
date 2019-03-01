//BizQuiz App 2019

using Plugin.Connectivity;
using Realms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace appFBLA2019
{
    public static class QuizRosterDatabase
    {
        //needs to be a property so that it changes when UserPath does
        private static string RosterPath { get { return App.UserPath + "roster.realm"; } }

        public static void NewQuizInfo(string authorName, string quizName, string category)
        {
            RealmConfiguration threadConfig = new RealmConfiguration(RosterPath);
            Realm realmDB = Realm.GetInstance(threadConfig);
            realmDB.Write(() =>
            {
                realmDB.Add(new QuizInfo(authorName, quizName, category));
            });
        }

        public static void NewQuizInfo(QuizInfo quizInfo)
        {
            RealmConfiguration threadConfig = new RealmConfiguration(RosterPath);
            Realm realmDB = Realm.GetInstance(threadConfig);
            realmDB.Write(() =>
            {
                realmDB.Add(quizInfo);
            });
        }

        public static void EditQuizInfo(QuizInfo editedQuizInfo)
        {
            RealmConfiguration threadConfig = new RealmConfiguration(RosterPath);
            Realm realmDB = Realm.GetInstance(threadConfig);
            realmDB.Write(() =>
            {
                realmDB.Add(editedQuizInfo, update: true);
            });
        }

        private static void EditQuizInfo(Realm threadedRealm, QuizInfo editedQuizInfo)
        {
            threadedRealm.Write(() =>
            {
                threadedRealm.Add(editedQuizInfo, update: true);
            });
        }

        /// <summary>
        /// Load QuizInfos property before entering task.
        /// </summary>
        public static QuizInfo GetQuizInfo(string quizName, string authorName)
        {
            try
            {
                RealmConfiguration threadConfig = new RealmConfiguration(RosterPath);
                Realm realmDB = Realm.GetInstance(threadConfig);
                return realmDB.All<QuizInfo>().Where
                                 (quizInfo => quizInfo.AuthorName == authorName && quizInfo.QuizName == quizName).First();
            }
            catch
            {
                return null;
            }
        }

        public static QuizInfo GetQuizInfo(string dbId)
        {
            try
            {
                RealmConfiguration threadConfig = new RealmConfiguration(RosterPath);
                Realm realmDB = Realm.GetInstance(threadConfig);
                return realmDB.All<QuizInfo>().Where
                                 (quizInfo => quizInfo.DBId == dbId).First();
            }
            catch (Exception ex)
            {
                BugReportHandler.SaveReport(ex, "QuizRosterDatabase.GetQuizInfo()");
                return null;
            }
        }

        public static List<QuizInfo> GetRoster()
        {
            RealmConfiguration threadConfig = new RealmConfiguration(RosterPath);
            Realm realmDB = Realm.GetInstance(threadConfig);
            return new List<QuizInfo>(realmDB.All<QuizInfo>());
        }

        public static List<QuizInfo> GetRoster(string category)
        {
            RealmConfiguration threadConfig = new RealmConfiguration(RosterPath);
            Realm realmDB = Realm.GetInstance(threadConfig);
            return new List<QuizInfo>(realmDB.All<QuizInfo>().Where(quizInfo => quizInfo.Category == category && !quizInfo.IsDeletedLocally));
        }

        public static bool DeleteQuizInfo(string dbId)
        {
            try
            {
                RealmConfiguration threadConfig = new RealmConfiguration(RosterPath);
                Realm realmDB = Realm.GetInstance(threadConfig);
                realmDB.Remove(realmDB.All<QuizInfo>().Where(quizInfo => quizInfo.DBId == dbId).First());
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task UpdateLocalDatabase()
        {
            RealmConfiguration threadConfig = new RealmConfiguration(RosterPath);
            Realm threadInstance = Realm.GetInstance(threadConfig);

            List<QuizInfo> QuizInfos = new List<QuizInfo>(threadInstance.All<QuizInfo>());
            if (CrossConnectivity.Current.IsConnected)
            {
                for (int i = 0; i < QuizInfos.Count(); i++)
                {
                    if (CredentialManager.IsLoggedIn)
                    {
                        if (QuizInfos[i].IsDeletedLocally)
                        {
                            await ServerOperations.DeleteQuiz(QuizInfos[i].DBId);
                        }
                        else if (QuizInfos[i].SyncStatus != 4)
                        {
                            string lastModifiedDate = ServerOperations.GetLastModifiedDate(QuizInfos[i].DBId);
                            if (lastModifiedDate == "") // returns empty string could not reach server
                            {
                                QuizInfo copy = new QuizInfo(QuizInfos[i])
                                {
                                    SyncStatus = 3 // 3 represents offline
                                };
                                EditQuizInfo(threadInstance, copy);
                            }
                            else
                            {
                                // Server returns "false" if level is not already on the server
                                if (lastModifiedDate == "false" || lastModifiedDate == null)
                                {
                                    QuizInfo copy = new QuizInfo(QuizInfos[i])
                                    {
                                        SyncStatus = 1 // 1 represents need upload
                                    };
                                    EditQuizInfo(threadInstance, copy);
                                }
                                else
                                {
                                    DateTime localModifiedDateTime = Convert.ToDateTime(QuizInfos[i].LastModifiedDate);
                                    DateTime serverModifiedDateTime = Convert.ToDateTime(lastModifiedDate);
                                    if (localModifiedDateTime > serverModifiedDateTime)
                                    {
                                        QuizInfo copy = new QuizInfo(QuizInfos[i])
                                        {
                                            SyncStatus = 1 // 1 represents need upload
                                        };
                                        EditQuizInfo(threadInstance, copy);
                                    }
                                    else if (localModifiedDateTime < serverModifiedDateTime)
                                    {
                                        QuizInfo copy = new QuizInfo(QuizInfos[i])
                                        {
                                            SyncStatus = 0 // 0 represents needs download
                                        };
                                        EditQuizInfo(threadInstance, copy);
                                    }
                                    else if (localModifiedDateTime == serverModifiedDateTime)
                                    {
                                        QuizInfo copy = new QuizInfo(QuizInfos[i])
                                        {
                                            SyncStatus = 2 // 2 represents in sync
                                        };
                                        EditQuizInfo(threadInstance, copy);
                                    }
                                }
                            }
                        }
                    }
                }

                int numberOfQuizsOnServer = ServerOperations.GetNumberOfQuizzesByAuthorName(CredentialManager.Username);
                if (numberOfQuizsOnServer > QuizInfos.Count)
                {
                    string[] dbIds = new string[QuizInfos.Count];
                    for (int i = 0; i < dbIds.Length; i++)
                        dbIds[i] = QuizInfos[i].DBId;

                    List<string[]> missingQuizs = ServerOperations.GetMissingQuizzesByAuthorName(CredentialManager.Username, dbIds);
                    foreach (string[] missingQuiz in missingQuizs)
                    {
                        QuizInfo info = new QuizInfo
                        {
                            DBId = missingQuiz[0],
                            AuthorName = missingQuiz[1],
                            QuizName = missingQuiz[2],
                            Category = missingQuiz[3],
                            LastModifiedDate = missingQuiz[4],
                            SyncStatus = 4
                        };
                        threadInstance.Write(() =>
                        {
                            threadInstance.Add(info);
                        });
                    }
                }
            }
            else
            {
                for (int i = 0; i < QuizInfos.Count; i++)
                {
                    QuizInfo copy = new QuizInfo(QuizInfos[i])
                    {
                        SyncStatus = 3 // 3 represents offline
                    };
                    EditQuizInfo(threadInstance, copy);
                }
            }
        }
    }
}