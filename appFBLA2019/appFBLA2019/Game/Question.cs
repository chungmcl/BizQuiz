//BizQuiz App 2019

using Realms;
using System;
using System.Collections.Generic;
using System.Text;

namespace appFBLA2019
{
    /// <summary>
    /// The question object holds both the question text and answers for a particular question to ask
    /// the user.
    /// </summary>
    public class Question : RealmObject, IComparable
    {
        /// <summary>
        /// Creates a question object given the question and answers
        /// </summary>
        /// <param name="question">  What to ask the user </param>
        /// <param name="answersIn"> 
        /// The potential answers to the question. The correct answer will be prefixed with "c/". The
        /// rest will be prefixed with "x/".
        /// </param>
        public Question(string question, string imagePath, params string[] answers)
        {
            this.QuestionText = question;
            //tries to assign the params to the local Answers property which in turn assigns the fields
            //if answers is null, throws an exception
            this.Status = 0;
            this.Answers = new List<string>(answers) ?? throw new ArgumentException("Must have at least one answer!");
            this.ImagePath = imagePath;
        }

        public Question(string question, params string[] answers) : this(question, null, answers)
        {
            this.NeedsPicture = false;
        }

        public Question() : this("This question is empty!", null, new string[0])
        {
        }

        public string AnswerOne { get; set; }

        [Ignored]
        public List<string> Answers
        {
            //turns the 4 answers into an easy to use array
            get
            { return new List<string> { this.CorrectAnswer, this.AnswerOne, this.AnswerTwo, this.AnswerThree }; }
            //takes an array, makes sure it can be used in our answers, and assigns it to the answers
            private set
            {
                //temp has empty spaces so we dont assign from a null spot in value
                string[] temp = new string[4];
                if (value.Count > 4)
                {
                    throw new ArgumentException("You can only have 4 answers!");
                }

                value.CopyTo(temp, 0);

                //makes sure there's actually a value to assign, otherwise makes it empty

                this.CorrectAnswer = temp[0] ?? "";
                this.AnswerOne = temp[1] ?? "";
                this.AnswerTwo = temp[2] ?? "";
                this.AnswerThree = temp[3] ?? "";
            }
        }

        public string AnswerThree { get; set; }

        public string AnswerTwo { get; set; }

        public string CorrectAnswer { get; set; }

        // Primary key ID for database Once set, IT CAN NEVER BE CHANGED.
        [PrimaryKey]
        public string DBId { get; set; }

        // SQLITE WILL IGNORE ALL PROPERTIES THAT ARE NOT DEFINED BY public { get; set; }

        [Ignored]
        public string ImagePath { get; set; }

        public bool NeedsPicture { get; set; }

        public string QuestionText { get; set; }

        // 0 = Multiple choice, 1 = Text answer w/o upper/lower case, 2 = Text answer with
        // upper/lower case
        public int QuestionType { get; set; }

        //declare enum (to use this type other places, say Question.QuestionStatus)
        //public enum QuestionStatus { Unanswered, Failed, Correct }
        public int Status { get; set; }

        //used to sort questions by status
        public int CompareTo(object obj)
        {
            return this.Status.CompareTo(((Question)obj).Status);
        }
    }
}