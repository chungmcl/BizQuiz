//BizQuiz App 2019

using Plugin.Connectivity;
using Realms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace appFBLA2019
{
    /// <summary>
    /// The quiz roster keeps track of the quizzes the user has downloaded and enables syncing to server
    /// </summary>
    public static class QuizRosterDatabase
    {
        /// <summary>
        /// The path of the current user's roster of quizzes
        /// </summary>
        private static string RosterPath { get { return App.UserPath + "roster.realm"; } }

        /// <summary>
        /// Creates and saves a new quizinfo to the roster
        /// </summary>
        /// <param name="authorName">Author of the quiz</param>
        /// <param name="quizName">name of the quiz</param>
        /// <param name="category">category of the quiz</param>
        public static void SaveQuizInfo(string authorName, string quizName, string category)
        {
            SaveQuizInfo(new QuizInfo(authorName, quizName, category));
        }

        /// <summary>
        /// Saves a quizinfo to the roster
        /// </summary>
        /// <param name="quizInfo">Quizinfo to save</param>
        public static void SaveQuizInfo(QuizInfo quizInfo)
        {
            RealmConfiguration threadConfig = App.realmConfiguration(RosterPath);
            Realm realmDB = Realm.GetInstance(threadConfig);
            realmDB.Write(() =>
            {
                realmDB.Add(quizInfo);
            });
        }

        /// <summary>
        /// updates a quiz in the roster
        /// </summary>
        /// <param name="editedQuizInfo">quiz to update</param>
        public static void EditQuizInfo(QuizInfo editedQuizInfo)
        {
            RealmConfiguration threadConfig = App.realmConfiguration(RosterPath);
            Realm realmDB = Realm.GetInstance(threadConfig);
            realmDB.Write(() =>
            {
                realmDB.Add(editedQuizInfo, update: true);
            });
            realmDB.Dispose();
        }

        /// <summary>
        /// updates a quizinfo in a threaded realm
        /// </summary>
        /// <param name="editedQuizInfo">quiz to update</param>
        /// <param name="threadedRealm">threaded realm to use</param>
        private static void EditQuizInfo(QuizInfo editedQuizInfo, Realm threadedRealm)
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
                RealmConfiguration threadConfig = App.realmConfiguration(RosterPath);
                Realm realmDB = Realm.GetInstance(threadConfig);
                return realmDB.All<QuizInfo>().Where
                                 (quizInfo => quizInfo.AuthorName == authorName && quizInfo.QuizName == quizName).First();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a quizinfo based on a dbid
        /// </summary>
        /// <param name="dbId">id of the quiz to fetch</param>
        /// <returns>the quiz matching the DBID (null if there isn't one)</returns>
        public static QuizInfo GetQuizInfo(string dbId)
        {
            try
            {
                RealmConfiguration threadConfig = App.realmConfiguration(RosterPath);
                Realm realmDB = Realm.GetInstance(threadConfig);
                return realmDB.All<QuizInfo>().Where
                                 (quizInfo => quizInfo.DBId == dbId).First();
            }
            catch (Exception ex)
            {
                BugReportHandler.SaveReport(ex);
                return null;
            }
        }

        /// <summary>
        /// Open the roster as a list of QuizInfos
        /// </summary>
        /// <returns>The roster</returns>
        public static List<QuizInfo> GetRoster()
        {
            RealmConfiguration threadConfig = App.realmConfiguration(RosterPath);
            Realm realmDB = Realm.GetInstance(threadConfig);
            return new List<QuizInfo>(realmDB.All<QuizInfo>());
        }

        /// <summary>
        /// Gets the roster of the category specified
        /// </summary>
        /// <param name="category">Category of quizzes to fetch</param>
        /// <returns>a filtered roster</returns>
        public static List<QuizInfo> GetRoster(string category)
        {
            RealmConfiguration threadConfig = App.realmConfiguration(RosterPath);
            Realm realmDB = Realm.GetInstance(threadConfig);
            return new List<QuizInfo>(realmDB.All<QuizInfo>().Where(
                quizInfo => quizInfo.Category == category && !quizInfo.IsDeletedLocally));
        }

        /// <summary>
        /// Given a dbid, deletes it from the roster
        /// </summary>
        /// <param name="dbId">ID of quiz to delete</param>
        /// <returns>indicated whether or not the quizz was successfully deleted</returns>
        public static bool DeleteQuizInfo(string dbId)
        {
            try
            {
                RealmConfiguration threadConfig = App.realmConfiguration(RosterPath);
                Realm realmDB = Realm.GetInstance(threadConfig);
                realmDB.Write(() =>
                {
                    realmDB.Remove(realmDB.All<QuizInfo>().Where(quizInfo => quizInfo.DBId == dbId).First());
                });
                return true;
            }
            catch (Exception ex)
            {
                BugReportHandler.SaveReport(ex);
                return false;
            }
        }

        /// <summary>
        /// Updates the local database by syncing with the server
        /// </summary>
        /// <returns>an awaitable task for the completion of syncing</returns>
        public static async Task UpdateLocalDatabaseAsync()
        {
            if (App.Path != null && App.UserPath.Length > 2)
            {
                RealmConfiguration threadConfig = App.realmConfiguration(RosterPath);
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
                                if (await ServerOperations.DeleteQuiz(QuizInfos[i].DBId) == OperationReturnMessage.True)
                                {
                                    string toDeleteDBId = QuizInfos[i].DBId;
                                    QuizInfos.Remove(QuizInfos[i]);
                                    DeleteQuizInfo(toDeleteDBId);
                                }
                            }
                            else if (QuizInfos[i].SyncStatus != (int)SyncStatusEnum.NotDownloadedAndNeedDownload)
                            {
                                string lastModifiedDate = ServerOperations.GetLastModifiedDate(QuizInfos[i].DBId);
                                if (lastModifiedDate == "") // returns empty string could not reach server
                                {
                                    QuizInfo copy = new QuizInfo(QuizInfos[i])
                                    {
                                        SyncStatus = (int)SyncStatusEnum.Offline // 3 represents offline
                                    };
                                    EditQuizInfo(copy, threadInstance);
                                }
                                else
                                {
                                    // Server returns "false" if quiz is not already on the server
                                    if (lastModifiedDate == "false" || lastModifiedDate == null)
                                    {
                                        QuizInfo copy = new QuizInfo(QuizInfos[i])
                                        {
                                            SyncStatus = (int)SyncStatusEnum.NeedUpload // 1 represents need upload
                                        };
                                        EditQuizInfo(copy, threadInstance);
                                    }
                                    else
                                    {
                                        DateTime localModifiedDateTime = Convert.ToDateTime(QuizInfos[i].LastModifiedDate);
                                        DateTime serverModifiedDateTime = Convert.ToDateTime(lastModifiedDate);
                                        if (localModifiedDateTime > serverModifiedDateTime)
                                        {
                                            QuizInfo copy = new QuizInfo(QuizInfos[i])
                                            {
                                                SyncStatus = (int)SyncStatusEnum.NeedUpload // 1 represents need upload
                                            };
                                            EditQuizInfo(copy, threadInstance);
                                        }
                                        else if (localModifiedDateTime < serverModifiedDateTime)
                                        {
                                            QuizInfo copy = new QuizInfo(QuizInfos[i])
                                            {
                                                SyncStatus = (int)SyncStatusEnum.NeedDownload // 0 represents needs download
                                            };
                                            EditQuizInfo(copy, threadInstance);
                                        }
                                        else if (localModifiedDateTime == serverModifiedDateTime)
                                        {
                                            QuizInfo copy = new QuizInfo(QuizInfos[i])
                                            {
                                                SyncStatus = (int)SyncStatusEnum.Synced // 2 represents in sync
                                            };
                                            EditQuizInfo(copy, threadInstance);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            QuizInfo copy = new QuizInfo(QuizInfos[i])
                            {
                                SyncStatus = (int)SyncStatusEnum.Synced // 2 represents in sync
                            };
                            EditQuizInfo(copy, threadInstance);
                        }
                    }

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
                            SubscriberCount = int.Parse(missingQuiz[5]),
                            SyncStatus = (int)SyncStatusEnum.NotDownloadedAndNeedDownload
                        };
                        threadInstance.Write(() =>
                        {
                            threadInstance.Add(info);
                        });
                    }
                }
                else
                {
                    for (int i = 0; i < QuizInfos.Count; i++)
                    {
                        QuizInfo copy = new QuizInfo(QuizInfos[i])
                        {
                            SyncStatus = (int)SyncStatusEnum.Offline // 3 represents offline
                        };
                        EditQuizInfo(copy, threadInstance);
                    }
                }
            }
        }
    }
}