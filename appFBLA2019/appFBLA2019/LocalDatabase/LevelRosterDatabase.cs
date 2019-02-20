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
        private static string rosterPath = App.Path + "/" + "roster.realm";
        public static void NewLevelInfo(string authorName, string levelName, string category)
        {
            RealmConfiguration threadConfig = new RealmConfiguration(rosterPath);
            Realm realmDB = Realm.GetInstance(threadConfig);
            realmDB.Write(() =>
            {
                realmDB.Add(new LevelInfo(authorName, levelName, category));
            });
        }

        public static void NewLevelInfo(LevelInfo levelInfo)
        {
            RealmConfiguration threadConfig = new RealmConfiguration(rosterPath);
            Realm realmDB = Realm.GetInstance(threadConfig);
            realmDB.Write(() =>
            {
                realmDB.Add(levelInfo);
            });
        }

        public static void EditLevelInfo(LevelInfo editedLevelInfo)
        {
            RealmConfiguration threadConfig = new RealmConfiguration(rosterPath);
            Realm realmDB = Realm.GetInstance(threadConfig);
            realmDB.Write(() =>
            {
                realmDB.Add(editedLevelInfo, update : true);
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
                RealmConfiguration threadConfig = new RealmConfiguration(rosterPath);
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
                RealmConfiguration threadConfig = new RealmConfiguration(rosterPath);
                Realm realmDB = Realm.GetInstance(threadConfig);
                return realmDB.All<LevelInfo>().Where
                                 (levelInfo => levelInfo.DBId == dbId).First();
            }
            catch
            {
                return null;
            }
        }

        public static List<LevelInfo> GetRoster()
        {
            RealmConfiguration threadConfig = new RealmConfiguration(rosterPath);
            Realm realmDB = Realm.GetInstance(threadConfig);
            return new List<LevelInfo>(realmDB.All<LevelInfo>());
        }

        public static List<LevelInfo> GetRoster(string category)
        {
            RealmConfiguration threadConfig = new RealmConfiguration(rosterPath);
            Realm realmDB = Realm.GetInstance(threadConfig);
            return new List<LevelInfo>(realmDB.All<LevelInfo>().Where(levelInfo => levelInfo.Category == category && !levelInfo.IsDeletedLocally));
        }

        public static void UpdateLocalDatabase()
        {
            RealmConfiguration threadConfig = new RealmConfiguration(App.Path + "/" + "roster.realm");
            Realm threadInstance = Realm.GetInstance(threadConfig);

            List <LevelInfo> LevelInfos = new List<LevelInfo>(threadInstance.All<LevelInfo>());
            for (int i = 0; i < LevelInfos.Count(); i++)
            {
                if (CrossConnectivity.Current.IsConnected)
                {
                    if (CredentialManager.IsLoggedIn)
                    {
                        if (LevelInfos[i].IsDeletedLocally)
                        {
                            ServerOperations.DeleteLevel(LevelInfos[i].DBId);
                        }
                        else
                        {
                            string lastModifiedDate = ServerOperations.GetLastModifiedDate(LevelInfos[i].DBId);
                            if (lastModifiedDate != null) // returns null if could not reach server
                            {
                                if (lastModifiedDate == "" || lastModifiedDate == null)
                                {
                                    LevelInfo copy = new LevelInfo(LevelInfos[i]);
                                    copy.SyncStatus = 1; // 1 represents need upload
                                    EditLevelInfo(threadInstance, copy);
                                }
                                else
                                {
                                    DateTime localModifiedDateTime = Convert.ToDateTime(LevelInfos[i].LastModifiedDate);
                                    DateTime serverModifiedDateTime = Convert.ToDateTime(lastModifiedDate);
                                    if (localModifiedDateTime > serverModifiedDateTime)
                                    {
                                        LevelInfo copy = new LevelInfo(LevelInfos[i]);
                                        copy.SyncStatus = 1; // 1 represents need upload
                                        EditLevelInfo(threadInstance, copy);
                                    }
                                    else if (localModifiedDateTime < serverModifiedDateTime)
                                    {
                                        LevelInfo copy = new LevelInfo(LevelInfos[i]);
                                        copy.SyncStatus = 0; // 0 represents needs download
                                        EditLevelInfo(threadInstance, copy);
                                    }
                                    else if (localModifiedDateTime == serverModifiedDateTime)
                                    {
                                        LevelInfo copy = new LevelInfo(LevelInfos[i]);
                                        copy.SyncStatus = 2; // 2 represents in sync
                                        EditLevelInfo(threadInstance, copy);
                                    }
                                }
                            }
                            else
                            {
                                LevelInfo copy = new LevelInfo(LevelInfos[i]);
                                copy.SyncStatus = 3; // 3 represents offline
                                EditLevelInfo(threadInstance, copy);
                            }
                        }
                    }
                }
                else
                {
                    LevelInfo copy = new LevelInfo(LevelInfos[i]);
                    copy.SyncStatus = 3; // 3 represents offline
                    EditLevelInfo(threadInstance, copy);
                }
            }
        }
    }
}
