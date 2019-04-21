using System.Collections.Generic;

namespace appFBLA2019
{
    static class SearchUtils
    {
        /// <summary>
        /// Gets a specified chunk of the levels on the server by a given author
        /// </summary>
        /// <param name="username">User to get levels from</param>
        /// <param name="chunk">chunk of levels to get</param>
        /// <returns>The chunk requested</returns>
        public static List<QuizInfo> GetQuizzesByAuthorChunked(string username = "BizQuiz", int chunk = 1)
        {
            List<string[]> quizDatas = ServerOperations.GetQuizzesByAuthorName(username, chunk);
            return ListOfDataToQuizInfo(quizDatas);
        }

        /// <summary>
        /// Gets a specified chunk of the levels on the server with a given string in their name
        /// </summary>
        /// <param name="quizName">search string for level names</param>
        /// <param name="chunk">chunk of levels to get</param>
        /// <returns>The chunk requested</returns>
        public static List<QuizInfo> GetQuizzesByQuizNameChunked(string quizName, int chunk = 1)
        {
            List<string[]> quizData = ServerOperations.GetQuizzesByQuizName(quizName, chunk);
            return ListOfDataToQuizInfo(quizData);
        }

        /// <summary>
        /// Convert List of String arrays containing quiz data (from server) to QuizInfo objects
        /// </summary>
        /// <param name="quizData"></param>
        private static List<QuizInfo> ListOfDataToQuizInfo(List<string[]> quizDatas)
        {
            List<QuizInfo> toReturn = new List<QuizInfo>();
            foreach (string[] quizData in quizDatas)
            {
                QuizInfo newInfo = new QuizInfo();
                newInfo.DBId = quizData[0];
                newInfo.AuthorName = quizData[1];
                newInfo.QuizName = quizData[2];
                newInfo.Category = quizData[3];
                newInfo.SubscriberCount = int.Parse(quizData[4]);
                toReturn.Add(newInfo);
            }
            return toReturn;
        }
    }
}
