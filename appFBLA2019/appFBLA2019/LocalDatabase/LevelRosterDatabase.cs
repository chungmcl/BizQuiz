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
    public static class LevelRosterDatabase
    {
        //needs to be a property so that it changes when UserPath does
        private static string RosterPath { get { return App.UserPath + "roster.realm"; } }

        public static void NewLevelInfo(string authorName, string levelName, string category)
        {
            RealmConfiguration threadConfig = new RealmConfiguration(RosterPath);
            Realm realmDB = Realm.GetInstance(threadConfig);
            realmDB.Write(() =>
            {
                realmDB.Add(new LevelInfo(authorName, levelName, category));
            });
        }

        public static void NewLevelInfo(LevelInfo levelInfo)
        {
            RealmConfiguration threadConfig = new RealmConfiguration(RosterPath);
            Realm realmDB = Realm.GetInstance(threadConfig);
            realmDB.Write(() =>
            {
                realmDB.Add(levelInfo);
            });
        }

        public static void EditLevelInfo(LevelInfo editedLevelInfo)
        {
            RealmConfiguration threadConfig = new RealmConfiguration(RosterPath);
            Realm realmDB = Realm.GetInstance(threadConfig);
            realmDB.Write(() =>
            {
                realmDB.Add(editedLevelInfo, update: true);
            });
        }

        private static void EditLevelInfo(Realm threadedRealm, LevelInfo editedLevelInfo)
        {
            threadedRealm.Write(() =>
            {
                threadedRealm.Add(editedLevelInfo, update: true);
            });
        }

        /// <summary>
        /// Load LevelInfos property before entering task.
        /// </summary>
        public static LevelInfo GetLevelInfo(string levelName, string authorName)
        {
            try
            {
                RealmConfiguration threadConfig = new RealmConfiguration(RosterPath);
                Realm realmDB = Realm.GetInstance(threadConfig);
                return realmDB.All<LevelInfo>().Where
                                 (levelInfo => levelInfo.AuthorName == authorName && levelInfo.LevelName == levelName).First();
            }
            catch
            {
                return null;
            }
        }

        public static LevelInfo GetLevelInfo(string dbId)
        {
            try
            {
                RealmConfiguration threadConfig = new RealmConfiguration(RosterPath);
                Realm realmDB = Realm.GetInstance(threadConfig);
                return realmDB.All<LevelInfo>().Where
                                 (levelInfo => levelInfo.DBId == dbId).First();
            }
            catch (Exception ex)
            {
                BugReportHandler.SubmitReport(ex, "LevelRosterDatabase.GetLevelInfo()");
                return null;
            }
        }

        /// <summary>
        /// Gets all subscribed levels of a user.
        /// </summary>
        /// <returns></returns>
        public static List<LevelInfo> GetRoster()
        {
            RealmConfiguration threadConfig = new RealmConfiguration(RosterPath);
            Realm realmDB = Realm.GetInstance(threadConfig);
            return new List<LevelInfo>(realmDB.All<LevelInfo>());
        }

        /// <summary>
        /// Gets all subscribed levels of the user of a specified category.
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public static List<LevelInfo> GetRoster(string category)
        {
            RealmConfiguration threadConfig = new RealmConfiguration(RosterPath);
            Realm realmDB = Realm.GetInstance(threadConfig);
            return new List<LevelInfo>(realmDB.All<LevelInfo>().Where(levelInfo => levelInfo.Category == category && !levelInfo.IsDeletedLocally));
        }

        public static bool DeleteLevelInfo(string dbId)
        {
            try
            {
                RealmConfiguration threadConfig = new RealmConfiguration(RosterPath);
                Realm realmDB = Realm.GetInstance(threadConfig);
                realmDB.Remove(realmDB.All<LevelInfo>().Where(levelInfo => levelInfo.DBId == dbId).First());
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

            List<LevelInfo> LevelInfos = new List<LevelInfo>(threadInstance.All<LevelInfo>());
            if (CrossConnectivity.Current.IsConnected)
            {
                for (int i = 0; i < LevelInfos.Count(); i++)
                {
                    if (CredentialManager.IsLoggedIn)
                    {
                        if (LevelInfos[i].IsDeletedLocally)
                        {
                            await ServerOperations.DeleteLevel(LevelInfos[i].DBId);
                        }
                        else if (LevelInfos[i].SyncStatus != 4)
                        {
                            string lastModifiedDate = ServerOperations.GetLastModifiedDate(LevelInfos[i].DBId);
                            if (lastModifiedDate != null) // returns null if could not reach server
                            {
                                if (lastModifiedDate == "" || lastModifiedDate == null)
                                {
                                    LevelInfo copy = new LevelInfo(LevelInfos[i])
                                    {
                                        SyncStatus = 1 // 1 represents need upload
                                    };
                                    EditLevelInfo(threadInstance, copy);
                                }
                                else
                                {
                                    DateTime localModifiedDateTime = Convert.ToDateTime(LevelInfos[i].LastModifiedDate);
                                    DateTime serverModifiedDateTime = Convert.ToDateTime(lastModifiedDate);
                                    if (localModifiedDateTime > serverModifiedDateTime)
                                    {
                                        LevelInfo copy = new LevelInfo(LevelInfos[i])
                                        {
                                            SyncStatus = 1 // 1 represents need upload
                                        };
                                        EditLevelInfo(threadInstance, copy);
                                    }
                                    else if (localModifiedDateTime < serverModifiedDateTime)
                                    {
                                        LevelInfo copy = new LevelInfo(LevelInfos[i])
                                        {
                                            SyncStatus = 0 // 0 represents needs download
                                        };
                                        EditLevelInfo(threadInstance, copy);
                                    }
                                    else if (localModifiedDateTime == serverModifiedDateTime)
                                    {
                                        LevelInfo copy = new LevelInfo(LevelInfos[i])
                                        {
                                            SyncStatus = 2 // 2 represents in sync
                                        };
                                        EditLevelInfo(threadInstance, copy);
                                    }
                                }
                            }
                            else
                            {
                                LevelInfo copy = new LevelInfo(LevelInfos[i])
                                {
                                    SyncStatus = 3 // 3 represents offline
                                };
                                EditLevelInfo(threadInstance, copy);
                            }
                        }
                    }
                }

                int numberOfLevelsOnServer = ServerOperations.GetNumberOfLevelsByAuthorName(CredentialManager.Username);
                if (numberOfLevelsOnServer > LevelInfos.Count)
                {
                    string[] dbIds = new string[LevelInfos.Count];
                    for (int i = 0; i < dbIds.Length; i++)
                        dbIds[i] = LevelInfos[i].DBId;

                    List<string[]> missingLevels = ServerOperations.GetMissingLevelsByAuthorName(CredentialManager.Username, dbIds);
                    foreach (string[] missingLevel in missingLevels)
                    {
                        LevelInfo info = new LevelInfo
                        {
                            DBId = missingLevel[0],
                            AuthorName = missingLevel[1],
                            LevelName = missingLevel[2],
                            Category = missingLevel[3],
                            LastModifiedDate = missingLevel[4],
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
                for (int i = 0; i < LevelInfos.Count; i++)
                {
                    LevelInfo copy = new LevelInfo(LevelInfos[i])
                    {
                        SyncStatus = 3 // 3 represents offline
                    };
                    EditLevelInfo(threadInstance, copy);
                }
            }
        }
    }
}