using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xamarin.Forms;

namespace appFBLA2019
{
    /// <summary>
    /// Manages connection between the rest of the app and the
    /// selected database file.
    /// </summary>
    static class DBHandler
    {
        public static GameDatabase Database { get; private set; }
        private const string realmExtension = ".realm";
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName">Name of the database file to be selected without extension</param>
        /// <param name="author">The username of the author of this level, which is unique (Two levels with the same
        /// name must have two unique authors)</param>
        /// <returns>Bool representing successful database connection or not.</returns>
        public static bool SelectDatabase(string levelTitle, string author)
        {
            try
            {
                // If the current database is null, or the name of the current database
                // does not match the name of the database being requested to be selected,
                // connect to the database specified in the parameter fileName
                
                // Backtick ( ` ) character used to seperate level name from author name
                if (Database == null || Database.fileName != $"{levelTitle}`{author}")
                {
                    // This path should be used when app is finished
                    // This will hide the application database and prevent it from
                    // unwanted user manipulation

                    //Database = new GameDatabase(
                    //  Path.Combine(
                    //      Environment.GetFolderPath(
                    //          Environment.SpecialFolder.LocalApplicationData), fileName + sqLiteExtension)
                    //          , fileName);

                    // On Android: Set appFBLA2019.Android's storage permissions to "on"

                    /*REMOVE DURING RELEASE*/ Directory.CreateDirectory(DependencyService.Get<IGetStorage>().GetStorage() + "/FBLADebug");
                    string publicPath = DependencyService.Get<IGetStorage>().GetStorage() + "/FBLADebug";
                    string folderPath = publicPath + $"/{levelTitle}`{author}";
                    Directory.CreateDirectory(folderPath);
                    string inFolderFileName = $"/{levelTitle}{realmExtension}";
                    Database = new GameDatabase(folderPath + inFolderFileName, levelTitle);

                    return true;
                }
                else // Otherwise, the database being requested is already open
                {
                    return true;
                }
            }
            catch (Exception ex)// If the database failed to connect
            {
                string test = ex.Message.ToString();
                return false;
            }
        }
    }
}
