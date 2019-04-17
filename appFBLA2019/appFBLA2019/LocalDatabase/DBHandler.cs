//BizQuiz App 2019

using System;
using System.IO;

namespace appFBLA2019
{
    /// <summary>
    /// Manages connection between the rest of the app and the selected database file.
    /// </summary>
    internal static class DBHandler
    {
        /// <summary>
        /// The gamedatabase currently selected
        /// </summary>
        public static GameDatabase Database { get; private set; }

        /// <summary>
        /// </summary>
        /// <returns> Bool representing successful database connection or not. </returns>
        public static bool SelectDatabase(string DBId)
        {
            try
            {
                QuizInfo info = QuizRosterDatabase.GetQuizInfo(DBId);
                // If the current database is null, or the name of the current database does not match the name of the database being requested to be selected, 
                // connect to the database specified in the parameter fileName
                if (Database == null || Database.relativePath != info.RelativePath)
                {
                    Database = new GameDatabase(info.RelativePath);

                    return true;
                }
                else // Otherwise, the database being requested is already open
                {
                    return true;
                }
            }
            catch (Exception ex)// If the database failed to connect
            {
                BugReportHandler.SaveReport(ex);
                return false;
            }
        }
    }
}