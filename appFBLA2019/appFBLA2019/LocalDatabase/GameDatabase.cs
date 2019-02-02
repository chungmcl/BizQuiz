using System;
using System.Collections.Generic;
using Realms;
using System.Linq;
using System.IO;
using Xamarin.Forms;

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
        private string dbPath;
        private string dbFolderPath;
        
        public GameDatabase(string dbFolderPath, string fileName)
        {
            try
            {
                this.dbPath = dbFolderPath + fileName;
                this.dbFolderPath = dbFolderPath;
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
            foreach (Question question in queryable)
            {
                question.ImagePath = dbFolderPath + question.DBId + ".jpg";
            }
            return new List<Question>(queryable);
        }

        public void AddQuestions(List<Question> questions)
        {
            foreach (Question question in questions)
            {
                string dbPrimaryKey = Guid.NewGuid().ToString(); // Once created, it will be PERMANENT AND IMMUTABLE
                question.DBId = dbPrimaryKey;

                byte[] imageByteArray = File.ReadAllBytes(question.ImagePath);

                if (question.NeedsPicture)
                    File.WriteAllBytes("dbPrimaryKey.jpg", imageByteArray);

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

        public double GetAvgScore()
        {
            if (this.realmDB != null)
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
            else
            {
                return 0.0;
            }
        }
    }
}
