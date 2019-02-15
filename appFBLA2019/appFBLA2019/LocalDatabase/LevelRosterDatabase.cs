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
            RealmConfiguration rC = new RealmConfiguration(App.Path + "/" + "roster.realm");
            realmDB = Realm.GetInstance(rC);
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

        public static void UpdateLocalDatabase()
        {
            if (LevelInfos != null)
            {
                lock (LevelInfos)
                {
                    for (int i = 0; i < LevelInfos.Count(); i++)
                    {
                        if (CrossConnectivity.Current.IsConnected)
                        {
                            ServerConnector.SendData(ServerRequestTypes.GetLastModifiedDate, LevelInfos[i].DBId);
                            string serverData = ServerConnector.ReceiveFromServerStringData();
                            if (serverData == "" || serverData == null)
                            {
                                LevelInfo copy = new LevelInfo(LevelInfos[i]);
                                copy.SyncStatus = 1; // 1 represents need upload
                                EditLevelInfo(copy);
                            }
                            else
                            {
                                DateTime localModifiedDateTime = Convert.ToDateTime(LevelInfos[i].LastModifiedDate);
                                DateTime serverModifiedDateTime = Convert.ToDateTime(serverData);
                                if (localModifiedDateTime > serverModifiedDateTime)
                                {
                                    LevelInfo copy = new LevelInfo(LevelInfos[i]);
                                    copy.SyncStatus = 1; // 1 represents need upload
                                    EditLevelInfo(copy);
                                }
                                else if (localModifiedDateTime < serverModifiedDateTime)
                                {
                                    LevelInfo copy = new LevelInfo(LevelInfos[i]);
                                    copy.SyncStatus = 0; // 0 represents needs download
                                    EditLevelInfo(copy);
                                }
                                else if (localModifiedDateTime == serverModifiedDateTime)
                                {
                                    LevelInfo copy = new LevelInfo(LevelInfos[i]);
                                    copy.SyncStatus = 2; // 2 represents in sync
                                    EditLevelInfo(copy);
                                }
                            }
                        }
                        else
                        {
                            LevelInfo copy = new LevelInfo(LevelInfos[i]);
                            copy.SyncStatus = 3; // 3 represents offline
                            EditLevelInfo(copy);
                        }
                    }
                }
            }
        }
    }
}
