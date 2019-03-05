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
        public static List<SearchInfo> GetQuizzesByAuthorChunked(string username = "BizQuiz", int chunk = 1)
        {
            List<SearchInfo> toReturn = new List<SearchInfo>();
            List<string[]> quizzes = ServerOperations.GetQuizzesByAuthorName(username, chunk);

            foreach (string[] quizData in quizzes)
            {
                toReturn.Add(new SearchInfo(quizData[0], quizData[1], quizData[2], quizData[3], quizData[4]));
            }
            return toReturn;
        }

        /// <summary>
        /// Gets a specified chunk of the levels on the server with a given string in their name
        /// </summary>
        /// <param name="quizName">search string for level names</param>
        /// <param name="chunk">chunk of levels to get</param>
        /// <returns>The chunk requested</returns>
        public static List<SearchInfo> GetQuizzesByQuizNameChunked(string quizName, int chunk = 1)
        {
            List<SearchInfo> toReturn = new List<SearchInfo>();
            List<string[]> quizzes = ServerOperations.GetQuizzesByQuizName(quizName, chunk);

            foreach (string[] quizData in quizzes)
            {
                toReturn.Add(new SearchInfo(quizData[0], quizData[1], quizData[2], quizData[3], quizData[4]));
            }
            return toReturn;
        }
    }
}
