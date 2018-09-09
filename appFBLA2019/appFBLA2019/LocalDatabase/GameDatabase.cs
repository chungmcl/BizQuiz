using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace appFBLA2019
{
    public class GameDatabase
    {
        public readonly SQLiteAsyncConnection database;
        public readonly string fileName;

        public GameDatabase(string dbPath, string fileName)
        {
            this.database = new SQLiteAsyncConnection(dbPath);
            this.fileName = fileName;

            this.database.CreateTableAsync<Question>().Wait();
        }
    }
}
