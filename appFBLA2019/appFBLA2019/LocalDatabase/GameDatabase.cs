using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Realms;
using System.Linq;

namespace appFBLA2019
{
    /// <summary>
    /// Object representing the database file selected through DBHandler.
    /// Contains methods to modify the database file.
    /// </summary>
    public class GameDatabase
    {
        public Realm realmDB;
        public readonly string fileName;
        
        public GameDatabase(string dbPath, string fileName)
        {
            try
            {
                RealmConfiguration rC = new RealmConfiguration(dbPath);
                this.realmDB = Realm.GetInstance(rC);
                this.fileName = fileName;
            }
            catch (Exception ex)
            {
                string test = ex.Message.ToString();
            }
        }

        public List<Question> GetQuestions()
        {
            IQueryable<Question> queryable = this.realmDB.All<Question>();
            return new List<Question>(queryable);
        }

        // Make changes to existing questions in the database and add new questions
        public void UpdateQuestions(List<Question> questions)
        {
            int highestDBId = Increment() + 1;
            foreach (Question question in questions)
            {
                question.DBId = highestDBId;
                this.realmDB.Write(() =>
                {
                    this.realmDB.Add(question);
                });
                highestDBId++;
            }
        }

        public void AddQuestions(List<Question> questions)
        {
            foreach (Question question in questions)
            {
                this.realmDB.Write(() =>
                {
                    this.realmDB.Add(question);
                });
            }
        }

        public void AddScore(ScoreRecord score)
        {
            this.realmDB.Write(() =>
            {
                this.realmDB.Add(score);
            });
        }

        // the increment of the next primary key
        private int Increment()
        {
            IQueryable<Question> queryable = this.realmDB.All<Question>();
            if (queryable.Count() == 0)
                return 0;
            return (queryable.OrderByDescending(question => question.DBId).First()).DBId;
        }

        public double GetAvgScore()
        {
            IQueryable<ScoreRecord> queryable = this.realmDB.All<ScoreRecord>();
            List<ScoreRecord> scores = new List<ScoreRecord>(queryable);
            if (scores.Count <= 0)
            {
                return 0;
            }
            else
            {
                double runningTotal = 0;
                foreach (ScoreRecord score in scores)
                {
                    runningTotal += score.Score;
                }
                return runningTotal / scores.Count;
            }
        }
    }
}
