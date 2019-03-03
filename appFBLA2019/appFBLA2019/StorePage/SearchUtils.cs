using System;
using System.Collections.Generic;
using System.Text;

namespace appFBLA2019
{
    static class SearchUtils
    {

        public static List<SearchInfo> GetQuizzesByAuthorChunked(string username = "BizQuiz", int chunk = 1)
        {
            List<SearchInfo> toReturn = new List<SearchInfo>();
            List<string[]> quizzes = ServerOperations.GetQuizzesByAuthorName(username, chunk);

            foreach (string[] quizData in quizzes)
            {
                toReturn.Add(new SearchInfo(quizData[0], quizData[1], quizData[2], quizData[3], quizData[4]));
            }
            return toReturn;
            //testInfo.Add(new QuizInfo { DBId = "TestDBID", AuthorName = "TestAuthor", QuizName = "TestQuiz", Category = "FBLA General", Subscribers = 12 });
            //testInfo.Add(new QuizInfo { DBId = "TestDBID2", AuthorName = "TestAuthor2", QuizName = "TestQuiz2", Category = "FBLA General", Subscribers = 3 });
        }

        public static List<SearchInfo> GetQuizzesByQuizNameChunked(string quizName, int chunk = 1)
        {
            List<SearchInfo> toReturn = new List<SearchInfo>();
            List<string[]> quizzes = ServerOperations.GetQuizzesByQuizName(quizName, chunk);

            foreach (string[] quizData in quizzes)
            {
                toReturn.Add(new SearchInfo(quizData[0], quizData[1], quizData[2], quizData[3], quizData[4]));
            }
            return toReturn;
            //testInfo.Add(new QuizInfo { DBId = "TestDBID", AuthorName = "TestAuthor", QuizName = "TestQuiz", Category = "FBLA General", Subscribers = 12 });
            //testInfo.Add(new QuizInfo { DBId = "TestDBID2", AuthorName = "TestAuthor2", QuizName = "TestQuiz2", Category = "FBLA General", Subscribers = 3 });
        }
    }
}
