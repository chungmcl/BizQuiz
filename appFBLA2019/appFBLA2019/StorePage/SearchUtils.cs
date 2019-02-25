using System;
using System.Collections.Generic;
using System.Text;

namespace appFBLA2019
{
    static class SearchUtils
    {

        public static List<SearchInfo> GetQuizsByAuthorChunked(string username = "BizQuiz", int chunk = 1)
        {
            List<SearchInfo> toReturn = new List<SearchInfo>();
            List<string[]> quizs = ServerOperations.GetQuizsByAuthorName(username, chunk);

            foreach (string[] quizData in quizs)
            {
                toReturn.Add(new SearchInfo(quizData));
            }
            return toReturn;
            //testInfo.Add(new QuizInfo { DBId = "TestDBID", AuthorName = "TestAuthor", QuizName = "TestQuiz", Category = "FBLA General", Subscribers = 12 });
            //testInfo.Add(new QuizInfo { DBId = "TestDBID2", AuthorName = "TestAuthor2", QuizName = "TestQuiz2", Category = "FBLA General", Subscribers = 3 });
        }

        public static List<SearchInfo> GetQuizsByQuizNameChunked(string quizName, int chunk = 1)
        {
            List<SearchInfo> toReturn = new List<SearchInfo>();
            List<string[]> quizs = ServerOperations.GetQuizsByQuizName(quizName, chunk);

            foreach (string[] quizData in quizs)
            {
                toReturn.Add(new SearchInfo(quizData));
            }
            return toReturn;
            //testInfo.Add(new QuizInfo { DBId = "TestDBID", AuthorName = "TestAuthor", QuizName = "TestQuiz", Category = "FBLA General", Subscribers = 12 });
            //testInfo.Add(new QuizInfo { DBId = "TestDBID2", AuthorName = "TestAuthor2", QuizName = "TestQuiz2", Category = "FBLA General", Subscribers = 3 });
        }
    }
}
