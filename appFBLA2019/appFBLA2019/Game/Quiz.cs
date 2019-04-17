//BizQuiz App 2019

using System;
using System.Collections.Generic;
using Realms;

namespace appFBLA2019
{
    /// <summary>
    /// This class handles questions and the quiz
    /// </summary>
    public class Quiz
    {
        public GameDatabase Database { get; set; }
        //this one will be used in the app for database stuff
        /// <summary>
        /// Select/create database file for the current game/topic, Load the questions from file
        /// </summary>
        public Quiz(string DBId)
        {
            this.Database = new GameDatabase(DBId);
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
                    //uses a predicate to find the # of questions with a status lower than 2 (failed once or unattempted)
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
            this.Questions = this.Database.GetQuestions();
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
                this.Database.EditQuestion(copyQuestion);
            }
        }
    }
}