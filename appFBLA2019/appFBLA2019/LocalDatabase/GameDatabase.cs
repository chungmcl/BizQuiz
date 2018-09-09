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
        private readonly string fileName;

        public GameDatabase(string dbPath, string fileName)
        {
            this.database = new SQLiteAsyncConnection(dbPath);
            this.fileName = fileName;

            this.database.CreateTableAsync<Question>().Wait();
        }

        //this whole setup may be unecessary now that each question knows if its been answered or not
        public Task<List<Question>> GetUnansweredQuestions()
        {
            
        }

        public Task<List<Question>> GetAnsweredQuestions()
        {

        }

        public void SetUnansweredQuestions(List<Question> unansweredQuestions)
        {

        }

        public void SetAnsweredQuestions(List<Question> answeredQuestions)
        {

        }
    }
}
