//BizQuiz App 2019

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Realms;
using Xamarin.Forms;

namespace appFBLA2019
{
    /// <summary>
    /// This class handles questions and the quiz
    /// </summary>
    public class Quiz
    {
        /// <summary>
        /// the extension for realm files
        /// </summary>
        private const string realmExtension = ".realm";
        /// <summary>
        /// the path to the database folder
        /// </summary>
        public string DBFolderPath { get { return this.QuizInfo.RelativePath; } }

        /// <summary>
        /// the path to the quiz database
        /// </summary>
        private string dbPath
        {
            get
            {
                Directory.CreateDirectory(this.QuizInfo.RelativePath);
                return this.DBFolderPath + $"/{this.QuizInfo.DBId}{realmExtension}";
            }
        }

        /// <summary>
        /// Get the QuizInfo tied to this database
        /// </summary>
        /// <returns>A quiz info for the same quiz as this page</returns>
        public QuizInfo QuizInfo { get; private set; }

        /// <summary>
        /// the title of the quiz
        /// </summary>
        public string Title { get { return this.QuizInfo.QuizName; } }
        
        /// <summary>
        /// The current list of questions (includes answered)
        /// </summary>
        [Ignored]
        public List<Question> Questions { get; set; }

        
        /// <summary>
        /// Select database file for the current game/topic, Load the questions from file
        /// </summary>
        public Quiz(string DBId)
        {
            this.QuizInfo = QuizRosterDatabase.GetQuizInfo(DBId);
            this.LoadQuestions();
        }

        /// <summary>
        /// Create a new Quiz
        /// </summary>
        public Quiz(string authorName, string quizName, string category)
        {
            NewQuizInfo(authorName, quizName, category);
        }

        /// <summary>
        /// The amount of questions left that the user hasn't answered
        /// </summary>
        public int QuestionsRemaining
        {
            get
            {
                this.Questions.Sort();
                if (this.Questions != null)
                {
                    //uses a predicate to find the # of questions with a status lower than 2 (failed once or unattempted)
                    return this.Questions.FindAll(x => x.Status < 2).Count;
                }
                return 0;
            }
        }

        /// <summary>
        /// Gets a question from the pool of available questions
        /// </summary>
        /// <returns> a question that the user hasn't got right yet </returns>
        public Question GetQuestion()
        {
            if (this.Questions != null && this.Questions.Count > 0)
            {
                this.Questions.Sort();

                //randomly selects from questions that haven't been correct yet (includes unanswered)

                return this.Questions[App.random.Next(0, this.QuestionsRemaining)];
            }
            return null;
        }

        /// <summary>
        /// Loads the questions from the DB into the quiz and sets them all unanswered
        /// </summary>
        public void LoadQuestions()
        {
            this.Questions = this.GetQuestions();
            this.ResetQuiz();
        }

        /// <summary>
        /// Sets all the questions to be unanswered
        /// </summary>
        public void ResetQuiz()
        {
            foreach (Question x in this.Questions)
            {
                Question copyQuestion = new Question(x)
                {
                    Status = 0
                };
                this.EditQuestion(copyQuestion);
            }
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
            Realm realmDB = Realm.GetInstance(App.realmConfiguration(this.dbPath));
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
            Realm realmDB = Realm.GetInstance(App.realmConfiguration(this.dbPath));
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
            Realm realmDB = Realm.GetInstance(App.realmConfiguration(this.dbPath));
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
            Realm realmDB = Realm.GetInstance(App.realmConfiguration(this.dbPath));
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
        /// Create a new quizinfo and adds it to the Quiz DB.
        /// A copy is stored in the device quiz roster.
        /// </summary>
        /// <param name="authorName">Author</param>
        /// <param name="quizName">Quiz name</param>
        /// <param name="category">Category</param>
        public void NewQuizInfo(string authorName, string quizName, string category)
        {
            QuizInfo newQuizInfo = new QuizInfo(authorName, quizName, category)
            {
                // Sync status is irrelevant in a Quiz Database's copy of the QuizInfo
                SyncStatus = -1
            };
            this.QuizInfo = newQuizInfo;

            Realm realmDB = Realm.GetInstance(App.realmConfiguration(this.dbPath));

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
            Realm realmDB = Realm.GetInstance(App.realmConfiguration(this.dbPath));
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