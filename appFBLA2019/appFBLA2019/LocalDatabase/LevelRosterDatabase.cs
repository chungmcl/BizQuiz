using Realms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public static void NewLevelInfo(LevelInfo levelInfo)
        {
            realmDB.Write(() =>
            {
                realmDB.Add(levelInfo);
            });
        }

        public static void EditLevelInfo(LevelInfo levelInfo)
        {
            realmDB.Write(() =>
            {
                realmDB.Add(levelInfo, update: true);
            });
        }

        public static LevelInfo GetLevelInfo(string DBId)
        {
            IQueryable<LevelInfo> levelInfos = realmDB.All<LevelInfo>();
            return levelInfos.Where(levelInfo => levelInfo.DBId == DBId).First();
        }
    }
}
