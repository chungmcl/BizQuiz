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
        /// <summary>
        /// the extension for realm files
        /// </summary>
        private const string realmExtension = ".realm";
        /// <summary>
        /// the path to the database folder
        /// </summary>
        public string DBFolderPath { get { return App.UserPath + relativePath + "/"; } set { } }
        /// <summary>
        /// the path to the quiz database
        /// </summary>
        private readonly string dbPath;
        /// <summary>
        /// the relative path from the user folder to the quiz folder
        /// </summary>
        public readonly string relativePath;

        public GameDatabase(string DBId)
        {
            QuizInfo info = QuizRosterDatabase.GetQuizInfo(DBId);
            this.relativePath = info.RelativePath;
            this.dbPath = DBFolderPath + $"/{DBId}{realmExtension}";
        }

        /// <summary>
        /// adds questions to the database
        /// </summary>
        /// <param name="questions">one or more questions to add to the database</param>
        public void AddQuestions(params Question[] questions)
        {
            foreach (Question question in questions)
            {
                this.SaveQuestion(question);
            }
        }

        /// <summary>
        /// deletes questions from the database if they exist
        /// </summary>
        /// <param name="questions">one or more questions to delete</param>
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

        /// <summary>
        /// updates a question that already exists
        /// </summary>
        /// <param name="updatedQuestion">the question to save</param>
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

        /// <summary>
        /// Gets all the questions in the database
        /// </summary>
        /// <returns>Every question in this database</returns>
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

        /// <summary>
        /// Saves a question to the database
        /// </summary>
        /// <param name="question">the question to save</param>
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

        /// <summary>
        /// Get the quizinfo tied to this database
        /// </summary>
        /// <returns>A quiz info for the same quiz as this page</returns>
        public QuizInfo GetQuizInfo()
        {
            Realm realmDB = Realm.GetInstance(new RealmConfiguration(this.dbPath));
            return realmDB.All<QuizInfo>().First();
        }

        /// <summary>
        /// Create a new quizinfo and adds it to the 
        /// </summary>
        /// <param name="authorName">Author</param>
        /// <param name="quizName">Quiz name</param>
        /// <param name="category">Category</param>
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
            QuizRosterDatabase.SaveQuizInfo(rosterCopy);
        }

        /// <summary>
        /// Saves updates to a quizInfo
        /// </summary>
        /// <param name="editedQuizInfo">the new version of the quizinfo to save</param>
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