using Plugin.Connectivity;
using Realms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace appFBLA2019
{
    public static class LevelRosterDatabase
    {
        private static Realm realmDB;
        public static void Initialize()
        {
            try
            {
                RealmConfiguration rC = new RealmConfiguration(App.Path + "/" + "roster.realm");
                realmDB = Realm.GetInstance(rC);
            }
            catch
            {
            }
        }

        public static void NewLevelInfo(string authorName, string levelName, string category)
        {
            realmDB.Write(() =>
            {
                realmDB.Add(new LevelInfo(authorName, levelName, category));
            });
        }

        public static void EditLevelInfo(LevelInfo editedLevelInfo)
        {
            realmDB.Write(() =>
            {
                realmDB.Add(editedLevelInfo, update : true);
            });
        }

        public static LevelInfo GetLevelInfo(string DBId)
        {
            IQueryable<LevelInfo> levelInfos = realmDB.All<LevelInfo>();
            return levelInfos.Where(levelInfo => levelInfo.DBId == DBId).First();
        }

        public static LevelInfo GetLevelInfo(string author, string levelName)
        {
            IQueryable<LevelInfo> levelInfos = realmDB.All<LevelInfo>();
            return levelInfos.Where(levelInfo => levelInfo.AuthorName == author && levelInfo.LevelName == levelName).First();
        }

        private static async Task UpdateLocalDatabase()
        {
            IQueryable<LevelInfo> levelInfosQueryable = realmDB.All<LevelInfo>();
            List<LevelInfo> levelInfos = new List<LevelInfo>(levelInfosQueryable);
            levelInfosQueryable = null;
            for (int i = 0; i < levelInfos.Count(); i++)
            {
                if (CrossConnectivity.Current.IsConnected)
                {
                    await Task.Run(() => ServerConnector.SendData(ServerRequestTypes.GetLastModifiedDate, levelInfos[i].DBId));
                    string serverData = await Task.Run(() => ServerConnector.ReceiveFromServerStringData());
                    if (serverData == "" || serverData == null)
                    {
                        using (var trans = realmDB.BeginWrite())
                        {
                            levelInfos[i].SyncStatus = 1; // 1 represents need upload
                            trans.Commit();
                        }
                    }
                    else
                    {
                        DateTime localModifiedDateTime = Convert.ToDateTime(levelInfos[i].LastModifiedDate);
                        DateTime serverModifiedDateTime = Convert.ToDateTime(serverData);
                        if (localModifiedDateTime > serverModifiedDateTime)
                        {
                            using (var trans = realmDB.BeginWrite())
                            {
                                levelInfos[i].SyncStatus = 1; // 1 represents need upload
                                trans.Commit();
                            }
                        }
                        else if (localModifiedDateTime < serverModifiedDateTime)
                        {
                            using (var trans = realmDB.BeginWrite())
                            {
                                levelInfos[i].SyncStatus = 0; // 0 represents needs download
                                trans.Commit();
                            }
                        }
                        else if (localModifiedDateTime == serverModifiedDateTime)
                        {
                            using (var trans = realmDB.BeginWrite())
                            {
                                levelInfos[i].SyncStatus = 2; // 2 represents in sync
                                trans.Commit();
                            }
                        }
                    }
                }
                else
                    levelInfos[i].SyncStatus = 3; // 3 represents offline
            }
        }

        public static void StartTimedUpdate()
        {
            var minutes = TimeSpan.FromMinutes(2.0);
            Device.StartTimer(minutes, () =>
            {
                Task.Run(async () =>
                {
                    await UpdateLocalDatabase();
                });

                // Return true to continue the timer
                return true;
            });
        }
    }
}
