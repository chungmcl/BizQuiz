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
            
            // Create new table for Questions and ScoreRecords if they do not already exist
            this.database.CreateTableAsync<Question>().Wait();
            this.database.CreateTableAsync<ScoreRecord>().Wait();
        }

        public async Task<List<Question>> GetQuestions()
        {
            return await this.database.QueryAsync<Question>("SELECT * FROM Question");
        }

        public async void UpdateQuestions(List<Question> questions)
        {
            await this.database.UpdateAllAsync(questions);
        }

        public async void AddScore(ScoreRecord score)
        {
            await this.database.InsertAsync(score);
        }

        public async Task<double> GetAvgScore()
        {
            List<double> scores = await this.database.QueryAsync<double>("SELECT Score FROM ScoreRecord");
            double runningTotal = 0;
            foreach(double score in scores)
            {
                runningTotal += score;
            }
            return runningTotal / scores.Count;
        }

        public async void ClearDatabase()
        {
            await this.database.DeleteAllAsync<Question>();
        }
    }
}
