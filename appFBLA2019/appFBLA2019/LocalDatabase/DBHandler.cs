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
        private const string sqLiteExtension = ".db3";
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName">Name of the database file to be selected without extension</param>
        /// <returns>Bool representing successful database connection or not.</returns>
        public static bool SelectDatabase(string fileName)
        {
            try
            {
                // If the current database is null, or the name of the current database
                // does not match the name of the database being requested to be selected,
                // connect to the database specified in the parameter fileName
                if (Database == null || Database.fileName != fileName)
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
                    //string publicPath = $"/storage/emulated/0/{fileName}.db3";
                    string publicPath = DependencyService.Get<IGetStorage>().GetStorage() + $"/{fileName}{sqLiteExtension}";
                    Database = new GameDatabase(publicPath, fileName);

                    return true;
                }
                else // Otherwise, the database being requested is already open
                {
                    return true;
                }
            }
            catch // If the database failed to connect
            {
                return false;
            }
        }
    }
}
