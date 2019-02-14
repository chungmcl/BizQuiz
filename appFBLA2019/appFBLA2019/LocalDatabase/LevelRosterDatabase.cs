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
        public static List<LevelInfo> LevelInfos { get; set; }
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

        /// <summary>
        /// Load LevelInfos property before entering task.
        /// </summary>
        public static void LoadLevelInfos()
        {
            IQueryable<LevelInfo> levelInfosQueryable = realmDB.All<LevelInfo>();
            LevelInfos = new List<LevelInfo>(levelInfosQueryable);
        }

        private static async Task UpdateLocalDatabase()
        {
            for (int i = 0; i < LevelInfos.Count(); i++)
            {
                if (CrossConnectivity.Current.IsConnected)
                {
                    await Task.Run(() => ServerConnector.SendData(ServerRequestTypes.GetLastModifiedDate, LevelInfos[i].DBId));
                    string serverData = await Task.Run(() => ServerConnector.ReceiveFromServerStringData());
                    if (serverData == "" || serverData == null)
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            using (var trans = realmDB.BeginWrite())
                            {
                                LevelInfos[i].SyncStatus = 1; // 1 represents need upload
                                trans.Commit();
                            }
                        });
                    }
                    else
                    {
                        DateTime localModifiedDateTime = Convert.ToDateTime(LevelInfos[i].LastModifiedDate);
                        DateTime serverModifiedDateTime = Convert.ToDateTime(serverData);
                        if (localModifiedDateTime > serverModifiedDateTime)
                        {
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                using (var trans = realmDB.BeginWrite())
                                {
                                    LevelInfos[i].SyncStatus = 1; // 1 represents need upload
                                    trans.Commit();
                                }
                            });
                        }
                        else if (localModifiedDateTime < serverModifiedDateTime)
                        {
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                using (var trans = realmDB.BeginWrite())
                                {
                                    LevelInfos[i].SyncStatus = 0; // 0 represents needs download
                                    trans.Commit();
                                }
                            });
                        }
                        else if (localModifiedDateTime == serverModifiedDateTime)
                        {
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                using (var trans = realmDB.BeginWrite())
                                {
                                    LevelInfos[i].SyncStatus = 2; // 2 represents in sync
                                    trans.Commit();
                                }
                            });
                        }
                    }
                }
                else
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        using (var trans = realmDB.BeginWrite())
                        {
                            LevelInfos[i].SyncStatus = 3; // 3 represents offline
                            trans.Commit();
                        }
                    });
                }
            }
        }

        public static void StartTimedUpdate()
        {
            var minutes = TimeSpan.FromMinutes(2.0);
            Device.StartTimer(minutes, () =>
            {
                LoadLevelInfos();
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
