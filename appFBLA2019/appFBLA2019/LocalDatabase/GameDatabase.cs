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
                this.DBFolderPath = dbFolderPath + "/";
                RealmConfiguration rC = new RealmConfiguration(this.dbPath);
                this.realmDB = Realm.GetInstance(rC);
                this.fileName = $"/{levelTitle}{realmExtension}";
            }
            catch (Exception ex)
            {
                BugReportHandler.SubmitReport(ex, nameof(GameDatabase));
            }
        }

        /// <summary>
        /// Access the full database with the full path to the file.
        /// </summary>
        /// <param name="fullPath"></param>
        public GameDatabase(string fullPath)
        {
            try
            {
                RealmConfiguration rC = new RealmConfiguration(fullPath);
                this.realmDB = Realm.GetInstance(rC);
            }
            catch
            {

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
                    File.Delete(this.DBFolderPath + "/" + question.QuestionId + ".jpg");
                }

                this.realmDB.Write(() =>
                {
                    this.realmDB.Remove(question);
                });
            }
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
                File.WriteAllBytes(this.DBFolderPath + "/" + updatedQuestion.QuestionId + ".jpg", imageByteArray);
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
                    questions[i].ImagePath = this.DBFolderPath + "/" + questions[i].QuestionId + ".jpg";
                }
            }
            return questions;
        }

        private const string realmExtension = ".realm";
        public readonly string DBFolderPath;
        private readonly string dbPath;

        private void SaveQuestion(Question question)
        {
            string dbPrimaryKey = Guid.NewGuid().ToString(); // Once created, it will be PERMANENT AND IMMUTABLE
            question.QuestionId = dbPrimaryKey;

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
                File.WriteAllBytes(this.DBFolderPath + "/" + dbPrimaryKey + ".jpg", imageByteArray);
            }

            this.realmDB.Write(() =>
            {
                this.realmDB.Add(question);
            });
        }

        public LevelInfo GetLevelInfo()
        {
            return this.realmDB.All<LevelInfo>().First();
        }

        public void NewLevelInfo(string authorName, string levelName, string category)
        {
            LevelInfo newLevelInfo = new LevelInfo(authorName, levelName, category)
            {
                // Sync status is irrelevant in a Level Database's copy of the LevelInfo
                SyncStatus = -1
            };

            this.realmDB.Write(() =>
            {
                this.realmDB.Add(newLevelInfo);
            });

            LevelInfo rosterCopy = new LevelInfo(newLevelInfo)
            {
                SyncStatus = 1 // Default to 1, meaning "needs upload" in roster
            };
            LevelRosterDatabase.NewLevelInfo(rosterCopy);
        }

        public void EditLevelInfo(LevelInfo editedLevelInfo)
        {
            this.realmDB.Write(() =>
            {
                this.realmDB.Add(editedLevelInfo, update: true);
            });

            LevelInfo rosterCopy = new LevelInfo(editedLevelInfo)
            {
                SyncStatus = 1 // Default to 1, meaning "needs upload" in roster
            };
            LevelRosterDatabase.EditLevelInfo(rosterCopy);
        }
    }
}