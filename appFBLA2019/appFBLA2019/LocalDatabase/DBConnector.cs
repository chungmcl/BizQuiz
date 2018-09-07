using appFBLA2019.LocalDatabase;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace appFBLA2019
{
    static class DBConnector
    {
        static AppDatabase database;

        public static AppDatabase Database
        {
            get
            {
                if (database == null)
                {
                    database = new AppDatabase(
                      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "appFBlA2019DB.db3"));
                }
                return database;
            }
        }
    }
}
