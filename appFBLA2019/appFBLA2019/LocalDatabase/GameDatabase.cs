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
        private const string realmExtension = ".realm";
        public readonly string DBFolderPath;
        private readonly string dbPath;
        public readonly string fileName;

        public GameDatabase(string dbFolderPath, string quizTitle)
        {
            this.dbPath = dbFolderPath + $"/{quizTitle}{realmExtension}";
            this.DBFolderPath = dbFolderPath + "/";
            this.fileName = $"/{quizTitle}{realmExtension}";
        }

        /// <summary>
        /// Access the full database with the full path to the file.
        /// </summary>
        /// <param name="fullPath"></param>
        public GameDatabase(string fullPath)
        {
            this.dbPath = fullPath;
        }

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
            Realm realmDB = Realm.GetInstance(new RealmConfiguration(this.dbPath));
            foreach (Question question in questions)
            {
                if (question.NeedsPicture)
                {
                    File.Delete(this.DBFolderPath + "/" + question.QuestionId + ".jpg");
                }

                realmDB.Write(() =>
                {
                    realmDB.Remove(question);
                });
            }
        }

        public void EditQuestion(Question updatedQuestion)
        {
            Realm realmDB = Realm.GetInstance(new RealmConfiguration(this.dbPath));
            realmDB.Write(() =>
            {
                realmDB.Add(updatedQuestion, update: true);
            });

            if (updatedQuestion.NeedsPicture)
            {
                byte[] imageByteArray = File.ReadAllBytes(updatedQuestion.ImagePath);
                File.WriteAllBytes(this.DBFolderPath + "/" + updatedQuestion.QuestionId + ".jpg", imageByteArray);
            }
        }

        public List<Question> GetQuestions()
        {
            Realm realmDB = Realm.GetInstance(new RealmConfiguration(this.dbPath));
            IQueryable<Question> queryable = realmDB.All<Question>();
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

        private void SaveQuestion(Question question)
        {
            Realm realmDB = Realm.GetInstance(new RealmConfiguration(this.dbPath));
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

            realmDB.Write(() =>
            {
                realmDB.Add(question);
            });
        }

        public QuizInfo GetQuizInfo()
        {
            Realm realmDB = Realm.GetInstance(new RealmConfiguration(this.dbPath));
            return realmDB.All<QuizInfo>().First();
        }

        public void NewQuizInfo(string authorName, string quizName, string category)
        {
            Realm realmDB = Realm.GetInstance(new RealmConfiguration(this.dbPath));
            QuizInfo newQuizInfo = new QuizInfo(authorName, quizName, category)
            {
                // Sync status is irrelevant in a Quiz Database's copy of the QuizInfo
                SyncStatus = -1
            };

            realmDB.Write(() =>
            {
                realmDB.Add(newQuizInfo);
            });

            QuizInfo rosterCopy = new QuizInfo(newQuizInfo)
            {
                SyncStatus = 1 // Default to 1, meaning "needs upload" in roster
            };
            QuizRosterDatabase.NewQuizInfo(rosterCopy);
        }

        public void EditQuizInfo(QuizInfo editedQuizInfo)
        {
            Realm realmDB = Realm.GetInstance(new RealmConfiguration(this.dbPath));
            realmDB.Write(() =>
            {
                realmDB.Add(editedQuizInfo, update: true);
            });

            QuizInfo rosterCopy = new QuizInfo(editedQuizInfo)
            {
                SyncStatus = 1 // Default to 1, meaning "needs upload" in roster
            };
            QuizRosterDatabase.EditQuizInfo(rosterCopy);
        }
    }
}