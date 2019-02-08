﻿using System;
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
        private const string realmExtension = ".realm";

        public Realm realmDB;
        public readonly string fileName;
        private string dbPath;
        private string dbFolderPath;
        
        public GameDatabase(string dbFolderPath, string levelTitle)
        {
            try
            {
                this.dbPath = dbFolderPath + $"/{levelTitle}{realmExtension}";
                this.dbFolderPath = dbFolderPath;
                RealmConfiguration rC = new RealmConfiguration(this.dbPath);
                this.realmDB = Realm.GetInstance(rC);
                this.fileName = $"/{levelTitle}{realmExtension}";
            }
            catch (Exception ex)
            {
                string test = ex.Message.ToString();
            }
        }

        public List<Question> GetQuestions()
        {
            IQueryable<Question> queryable = this.realmDB.All<Question>();
            List<Question> questions = new List<Question>(queryable);
            for (int i = 0; i < queryable.Count(); i++)
            {
                if (questions[i].NeedsPicture)
                    questions[i].ImagePath = this.dbFolderPath + "/" + questions[i].DBId + ".jpg";
            }
            return questions;
        }

        public void AddQuestions(List<Question> questions)
        {
            foreach (Question question in questions)
            {
                string dbPrimaryKey = Guid.NewGuid().ToString(); // Once created, it will be PERMANENT AND IMMUTABLE
                question.DBId = dbPrimaryKey;

                if (question.NeedsPicture)
                {
                    byte[] imageByteArray = File.ReadAllBytes(question.ImagePath);

                    if (!question.ImagePath.Contains(".jpg") 
                        || !question.ImagePath.Contains(".jpeg") 
                        || !question.ImagePath.Contains(".jpe") 
                        || !question.ImagePath.Contains(".jif")
                        || !question.ImagePath.Contains(".jfif")
                        || !question.ImagePath.Contains(".jfi"))
                    {
                        Stream imageStream = DependencyService.Get<IGetImage>().GetJPGStreamFromByteArray(imageByteArray);
                        MemoryStream imageMemoryStream = new MemoryStream();

                        imageStream.Position = 0;
                        imageStream.CopyTo(imageMemoryStream);

                        imageMemoryStream.Position = 0;
                        imageByteArray = new byte[imageMemoryStream.Length];
                        imageMemoryStream.ToArray().CopyTo(imageByteArray, 0);
                    }
                    File.WriteAllBytes(this.dbFolderPath + "/" + dbPrimaryKey + ".jpg", imageByteArray);
                }

                this.realmDB.Write(() =>
                {
                    this.realmDB.Add(question);
                });
            }
        }

        // To update an existing question in the database:
        //
        // DBHandler.Database.realmDB.Write(() =>
        //         yourQuestion.Property = newValue
        //     );
        //

        public void DeleteQuestions(params Question[] questions)
        {
            foreach (Question question in questions)
            {
                if (question.NeedsPicture)
                    File.Delete(this.dbFolderPath + "/" + question.DBId + ".jpg");
                this.realmDB.Write(() =>
                {
                    this.realmDB.Remove(question);
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
