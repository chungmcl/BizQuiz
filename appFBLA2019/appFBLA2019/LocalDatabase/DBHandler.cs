using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace appFBLA2019
{
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
                if (Database == null || Database.fileName != fileName)
                {
                    Database = new GameDatabase(
                      Path.Combine(
                          Environment.GetFolderPath(
                              Environment.SpecialFolder.LocalApplicationData), fileName + sqLiteExtension)
                              , fileName);
                    return true;
                }
                else
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
