using System;
using System.Collections.Generic;
using System.Text;

namespace appFBLA2019
{
    static class SearchUtils
    {

        public static List<SearchInfo> GetLevelsByAuthorChunked(string username = "BizQuiz", int chunk = 1)
        {
            List<SearchInfo> toReturn = new List<SearchInfo>();
            List<string[]> levels = ServerOperations.GetLevelsByAuthorName(username, chunk);

            foreach (string[] levelData in levels)
            {
                toReturn.Add(new SearchInfo(levelData));
            }
            return toReturn;
            //testInfo.Add(new LevelInfo { DBId = "TestDBID", AuthorName = "TestAuthor", LevelName = "TestLevel", Category = "FBLA General", Subscribers = 12 });
            //testInfo.Add(new LevelInfo { DBId = "TestDBID2", AuthorName = "TestAuthor2", LevelName = "TestLevel2", Category = "FBLA General", Subscribers = 3 });
        }

        public static List<SearchInfo> GetLevelsByLevelNameChunked(string levelName, int chunk = 1)
        {
            List<SearchInfo> toReturn = new List<SearchInfo>();
            List<string[]> levels = ServerOperations.GetLevelsByLevelName(levelName, chunk);

            foreach (string[] levelData in levels)
            {
                toReturn.Add(new SearchInfo(levelData));
            }
            return toReturn;
            //testInfo.Add(new LevelInfo { DBId = "TestDBID", AuthorName = "TestAuthor", LevelName = "TestLevel", Category = "FBLA General", Subscribers = 12 });
            //testInfo.Add(new LevelInfo { DBId = "TestDBID2", AuthorName = "TestAuthor2", LevelName = "TestLevel2", Category = "FBLA General", Subscribers = 3 });
        }
    }
}
