using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace appFBLA2019
{
    static class DBHandler
    {
        private static GameDatabase database;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName">Name of the database file to be selected</param>
        /// <returns>Bool representing successful database connection or not.</returns>
        public static bool SelectDatabase(string fileName)
        {
            try
            {
                if (database == null || database.fileName != fileName)
                {
                    database = new GameDatabase(
                      Path.Combine(
                          Environment.GetFolderPath(
                              Environment.SpecialFolder.LocalApplicationData), fileName)
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
