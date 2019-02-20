//BizQuiz App 2019

using System;
using System.Collections.Generic;
using System.IO;
using Xamarin.Forms;

namespace appFBLA2019
{
    /// <summary>
    /// Manages connection between the rest of the app and the selected database file.
    /// </summary>
    internal static class DBHandler
    {
        public static GameDatabase Database { get; private set; }

        /// <summary>
        /// </summary>
        /// <param name="fileName"> Name of the database file to be selected without extension </param>
        /// <param name="author">   The username of the author of this level, which is unique (Two levels with the same name must have two unique authors) </param>
        /// <returns> Bool representing successful database connection or not. </returns>
        public static bool SelectDatabase(string category, string levelTitle, string author)
        {
            try
            {
                // If the current database is null, or the name of the current database does not match the name of the database being requested to be selected, connect to the database specified in the parameter fileName

                // Backtick ( ` ) character used to seperate level name from author name
                if (Database == null || Database.fileName != $"/{category}/{levelTitle}`{author}")
                {
                    // This path should be used when app is finished This will hide the application database and prevent it from unwanted user manipulation

                    //Database = new GameDatabase(
                    //  Path.Combine(
                    //      Environment.GetFolderPath(
                    //          Environment.SpecialFolder.LocalApplicationData), fileName + sqLiteExtension)
                    //          , fileName);

                    // On Android: Set appFBLA2019.Android's storage permissions to "on"

                    string folderPath = App.UserPath + $"{category}/{levelTitle}`{author}";
                    Directory.CreateDirectory(folderPath);
                    Database = new GameDatabase(folderPath, levelTitle);

                    return true;
                }
                else // Otherwise, the database being requested is already open
                {
                    return true;
                }
            }
            catch (Exception ex)// If the database failed to connect
            {
                BugReportHandler.SubmitReport(ex, nameof(DBHandler));
                return false;
            }
        }

        public static void DisposeDatabase()
        {
            if (Database != null)
                if (Database.realmDB != null)
                    if (!Database.realmDB.IsClosed)
                        Database.realmDB.Dispose();
        }
    }
}