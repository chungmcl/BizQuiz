﻿//BizQuiz App 2019

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
        /// <param name="fileName"> Name of the database file to be selected without extension </param>
        /// <param name="author">   The username of the author of this quiz, which is unique (Two quizzes with the same name must have two unique authors) </param>
        /// <returns> Bool representing successful database connection or not. </returns>
        public static bool SelectDatabase(string category, string quizTitle, string author)
        {
            try
            {
                // If the current database is null, or the name of the current database does not match the name of the database being requested to be selected, connect to the database specified in the parameter fileName

                // Backtick ( ` ) character used to seperate quiz name from author name
                if (Database == null || Database.fileName != $"/{category}/{quizTitle}`{author}")
                {
                    string folderPath = App.UserPath + $"{category}/{quizTitle}`{author}";
                    Directory.CreateDirectory(folderPath);
                    Database = new GameDatabase(folderPath, quizTitle);

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