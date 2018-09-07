using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace appFBLA2019.LocalDatabase
{
    class AppDatabase
    {
        readonly SQLiteAsyncConnection database;

        public AppDatabase(string dbPath)
        {
            database = new SQLiteAsyncConnection(dbPath);
            database.CreateTableAsync<Question>().Wait();
        }
    }
}
