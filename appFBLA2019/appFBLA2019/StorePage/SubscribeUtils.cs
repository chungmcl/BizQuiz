﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace appFBLA2019
{
    public static class SubscribeUtils
    {
        /// <summary>
        /// different actions that the sub/download button takes
        /// </summary>
        public enum SubscribeType { Subscribe = 1, Unsubscribe, Syncing };


        /// <summary>
        /// Subscribes to a quiz given the ID
        /// </summary>
        /// <param name="dbId">the ID of the quiz to sub to</param>
        /// <param name="quizzesSearched">the quizzes currently displayed (used to get info about the quiz from the dbid)</param>
        /// <returns>If the subscription was successful</returns>
        public static async Task<OperationReturnMessage> SubscribeToQuizAsync(string dbId, List<QuizInfo> quizzesSearched)
        {
            if (QuizRosterDatabase.GetQuizInfo(dbId) == null) // make sure it isn't in yet
            {
                OperationReturnMessage returnMessage = await Task.Run(async () => await ServerOperations.SubscribeToQuiz(dbId));
                if (returnMessage == OperationReturnMessage.True)
                {
                    QuizInfo quiz = quizzesSearched.Where(quizInfo => quizInfo.DBId == dbId).First();
                    string lastModifiedDate = await Task.Run(() => ServerOperations.GetLastModifiedDate(dbId));
                    QuizInfo newInfo = new QuizInfo
                    {
                        DBId = quiz.DBId,
                        AuthorName = quiz.AuthorName,
                        QuizName = quiz.QuizName,
                        Category = quiz.Category,
                        LastModifiedDate = lastModifiedDate,
                        SyncStatus = (int)SyncStatusEnum.NotDownloadedAndNeedDownload // 4 to represent not present in local directory and need download
                    };
                    QuizRosterDatabase.SaveQuizInfo(newInfo);
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

        /// <summary>
        /// Unsubscribe from a quiz
        /// </summary>
        /// <param name="dbId">ID to unsub from</param>
        /// <returns>If the unsub was successful</returns>
        public static async Task<OperationReturnMessage> UnsubscribeFromQuizAsync(string dbId)
        {
            QuizInfo info = QuizRosterDatabase.GetQuizInfo(dbId);
            if (info.SyncStatus != (int)SyncStatusEnum.NotDownloadedAndNeedDownload)
            {
                string location = App.UserPath + "/" + info.Category + "/" + info.QuizName + "`" + info.AuthorName;
                if (Directory.Exists(location))
                    Directory.Delete(location, true);
            }

            OperationReturnMessage returnMessage = await Task.Run(async () => await ServerOperations.UnsubscribeToQuizAsync(dbId));
            if (returnMessage == OperationReturnMessage.True)
            {
                QuizRosterDatabase.DeleteQuizInfo(dbId);
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
