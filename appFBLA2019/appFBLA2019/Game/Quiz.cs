//BizQuiz App 2019

using System;
using System.Collections.Generic;
namespace appFBLA2019
{
    /// <summary>
    /// This class handles questions and the quiz
    /// </summary>
    public class Quiz
    {
        //this one will be used in the app for database stuff
        /// <summary>
        /// Select/create database file for the current game/topic, Load the questions from file
        /// </summary>
        /// <param name="quizTitle">
        /// The name of the database file - if one does not yet exist, it will create one based on the name you pass it. DO NOT INCLUDE FILE EXTENSION IN FILENAME.
        /// </param>
        /// <param name="author">
        /// The username of the author of the quiz
        /// </param>
        public Quiz(string category, string quizTitle, string author)
        {
            this.Title = quizTitle;
            DBHandler.SelectDatabase(category, quizTitle, author);
        }

        /// <summary>
        /// The current list of questions (includes answered)
        /// </summary>
        public List<Question> Questions { get; set; }

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
                    //uses a predicate to find the # of questions with a status lower than 2 (failed or unattempted)
                    return this.Questions.FindAll(x => x.Status < 2).Count;
                }
                return 0;
            }
        }

        /// <summary>
        /// the title of the quiz
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Gets a question from the pool of available questions
        /// </summary>
        /// <returns>
        /// a question that the user hasn't got right yet
        /// </returns>
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
            this.Questions = DBHandler.Database.GetQuestions();
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
                DBHandler.Database.EditQuestion(copyQuestion);
            }
        }
    }
}