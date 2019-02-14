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
    }
}
