//BizQuiz App 2019

using Realms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xamarin.Forms;

namespace appFBLA2019
{
    /// <summary>
    /// Object representing the database file selected through DBHandler. Contains methods to modify the database file.
    /// </summary>
    public class GameDatabase
    {
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

        public readonly string fileName;
        public Realm realmDB;

        public void AddQuestions(List<Question> questions)
        {
            foreach (Question question in questions)
            {
                this.SaveQuestion(question);
            }
        }

        public void AddQuestions(Question question)
        {
            this.SaveQuestion(question);
        }

        public void DeleteQuestions(params Question[] questions)
        {
            foreach (Question question in questions)
            {
                if (question.NeedsPicture)
                {
                    File.Delete(this.dbFolderPath + "/" + question.DBId + ".jpg");
                }

                this.realmDB.Write(() =>
                {
                    this.realmDB.Remove(question);
                });
            }
        }

        public void SetCategory(LevelInfo levelInfo)
        {
            this.realmDB.Write(() =>
            {
                this.realmDB.Add(levelInfo, update: true);
            });
        }

        public string GetCategory()
        {
            List<LevelInfo> category = new List<LevelInfo>(this.realmDB.All<LevelInfo>());
            if (category.Count == 1)
                return category[0].Category;
            else
                return null;
        }

        public string GetDateEdited()
        {
            List<LevelInfo> dateEdited = new List<LevelInfo>(this.realmDB.All<LevelInfo>());
            if (dateEdited.Count == 1)
                return dateEdited[0].DateEdited;
            else
                return null;
        }

        public void EditQuestion(Question updatedQuestion)
        {
            this.realmDB.Write(() =>
            {
                this.realmDB.Add(updatedQuestion, update: true);
            });

            if (updatedQuestion.NeedsPicture)
            {
                byte[] imageByteArray = File.ReadAllBytes(updatedQuestion.ImagePath);
                File.WriteAllBytes(this.dbFolderPath + "/" + updatedQuestion.DBId + ".jpg", imageByteArray);
            }
        }

        public List<Question> GetQuestions()
        {
            IQueryable<Question> queryable = this.realmDB.All<Question>();
            List<Question> questions = new List<Question>(queryable);
            for (int i = 0; i < queryable.Count(); i++)
            {
                if (questions[i].NeedsPicture)
                {
                    questions[i].ImagePath = this.dbFolderPath + "/" + questions[i].DBId + ".jpg";
                }
            }
            return questions;
        }

        private const string realmExtension = ".realm";
        private readonly string dbFolderPath;
        private readonly string dbPath;

        private void SaveQuestion(Question question)
        {
            string dbPrimaryKey = Guid.NewGuid().ToString(); // Once created, it will be PERMANENT AND IMMUTABLE
            question.DBId = dbPrimaryKey;

            if (question.NeedsPicture)
            {
                byte[] imageByteArray = File.ReadAllBytes(question.ImagePath);
                File.WriteAllBytes(this.dbFolderPath + "/" + dbPrimaryKey + ".jpg", imageByteArray);
            }

            this.realmDB.Write(() =>
            {
                this.realmDB.Add(question);
            });
        }
    }
}