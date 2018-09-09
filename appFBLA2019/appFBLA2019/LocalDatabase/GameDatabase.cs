using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace appFBLA2019
{
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

        public Task<List<Question>> GetQuestions()
        {
            return this.database.QueryAsync<Question>("SELECT * FROM Question");
        }

        public void UpdateQuestions(List<Question> questions)
        {
            this.database.ExecuteAsync("DELETE * FROM Question");
            this.database.InsertAllAsync(questions);
        }
    }
}
