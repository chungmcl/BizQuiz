using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace appFBLA2019
{
    public static class SubscribeUtils
    {
        public static async Task<OperationReturnMessage> SubscribeToLevel(string dbId, List<SearchInfo> quizzesSearched)
        {
            if (QuizRosterDatabase.GetQuizInfo(dbId) == null) // make sure it isn't in yet
            {
                OperationReturnMessage returnMessage = await Task.Run(async () => await ServerOperations.SubscribeToQuiz(dbId));
                if (returnMessage == OperationReturnMessage.True)
                {
                    SearchInfo quiz = quizzesSearched.Where(searchInfo => searchInfo.DBId == dbId).First();
                    string lastModifiedDate = await Task.Run(() => ServerOperations.GetLastModifiedDate(dbId));
                    QuizInfo newInfo = new QuizInfo
                    {
                        DBId = quiz.DBId,
                        AuthorName = quiz.Author,
                        QuizName = quiz.QuizName,
                        Category = quiz.Category,
                        LastModifiedDate = lastModifiedDate,
                        SyncStatus = 4 // 4 to represent not present in local directory and need download
                    };
                    QuizRosterDatabase.NewQuizInfo(newInfo);
                    return returnMessage;
                }
                else if (returnMessage == OperationReturnMessage.FalseInvalidCredentials)
                {
                    CredentialManager.IsLoggedIn = false;
                    return returnMessage;
                }
                else
                {
                    return returnMessage;
                }
            }
            else
            {
                return OperationReturnMessage.False;
            }
            
        }

        public static async Task<OperationReturnMessage> UnsubscribeToLevel(string dbId)
        {
            QuizInfo info = QuizRosterDatabase.GetQuizInfo(dbId);
            string location = App.UserPath + "/" + info.Category + "/" + info.QuizName + "`" + info.AuthorName;
            if (Directory.Exists(location))
                Directory.Delete(location, true);

            QuizRosterDatabase.DeleteQuizInfo(dbId);
            OperationReturnMessage returnMessage = await Task.Run(async () => await ServerOperations.UnsubscribeToQuiz(dbId));
            if (returnMessage == OperationReturnMessage.True)
            {
                return returnMessage;
            }
            else if (returnMessage == OperationReturnMessage.FalseInvalidCredentials)
            {
                CredentialManager.IsLoggedIn = false;
                return returnMessage;
            }
            else
            {
                return returnMessage;
            }
        }
    }
}
