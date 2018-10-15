using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace appFBLA2019
{
    /// <summary>
    /// Object representing the database file selected through DBHandler.
    /// Contains methods to modify the database file.
    /// </summary>
    public class GameDatabase
    {
        private readonly SQLiteAsyncConnection database;
        public readonly string fileName;
        
        public GameDatabase(string dbPath, string fileName)
        {
            this.database = new SQLiteAsyncConnection(dbPath);
            this.fileName = fileName;

            this.database.CreateTableAsync<Question>().Wait();
        }

        public async Task<List<Question>> GetQuestions()
        {
            return await this.database.QueryAsync<Question>("SELECT * FROM Question");
        }

        public async void UpdateQuestions(List<Question> questions)
        {
            await this.database.InsertAllAsync(questions);
        }

        public async void ClearDatabase()
        {
            await this.database.DeleteAllAsync<Question>();
        }
    }
}
