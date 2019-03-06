//BizQuiz App 2019

using Realms;
using System;
using System.Collections.Generic;

namespace appFBLA2019
{
    /// <summary>
    /// The question object holds both the question text and answers for a particular question to ask the user.
    /// </summary>
    public class Question : RealmObject, IComparable
    {
        /// <summary>
        /// Creates a question object given the question and answers
        /// </summary>
        /// <param name="question">  What to ask the user </param>
        /// <param name="answersIn"> The potential answers to the question. The correct answer is in the first slot. </param>
        public Question(string question, string imagePath, params string[] answers)
        {
            this.QuestionText = question.Trim();
            //tries to assign the params to the local Answers property which in turn assigns the fields
            //if answers is null, throws an exception
            this.Status = 0;
            this.Answers = new List<string>(answers) ?? throw new ArgumentException("Must have at least one answer!");
            this.ImagePath = imagePath;
        }

        /// <summary>
        /// Creates a question based on another question - copy constructor
        /// </summary>
        /// <param name="question">  </param>
        public Question(Question question)
        {
            this.AnswerOne = question.AnswerOne.Trim();
            this.AnswerTwo = question.AnswerTwo.Trim();
            this.AnswerThree = question.AnswerThree.Trim();
            this.CorrectAnswer = question.CorrectAnswer.Trim();
            this.ImagePath = question.ImagePath;
            this.NeedsPicture = question.NeedsPicture;
            this.QuestionId = question.QuestionId;
            this.QuestionType = question.QuestionType;
            this.Status = question.Status;
            this.QuestionText = question.QuestionText.Trim();
        }

        /// <summary>
        /// Creates a question that doesn't need an image
        /// </summary>
        /// <param name="question">  </param>
        /// <param name="answers">   </param>
        public Question(string question, params string[] answers) : this(question, null, answers)
        {
            this.NeedsPicture = false;
        }

        /// <summary>
        /// Default constructor, creates an empty question
        /// </summary>
        public Question() : this("This question is empty!", "", new string[0])
        {
        }

        /// <summary>
        /// Stores and returns the answers with some logic to keep them stable
        /// </summary>
        [Ignored]
        public List<string> Answers
        {
            //turns the 4 answers into an easy to use array
            get
            { return new List<string> { this.CorrectAnswer.Trim(), this.AnswerOne.Trim(), this.AnswerTwo.Trim(), this.AnswerThree.Trim() }; }
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

        /// <summary>
        /// the right answer
        /// </summary>
        public string CorrectAnswer { get; set; }

        /// <summary>
        /// The first of the wrong answers
        /// </summary>
        public string AnswerOne { get; set; }

        /// <summary>
        /// The second of the wrong answers
        /// </summary>
        public string AnswerTwo { get; set; }

        /// <summary>
        /// The 3rd of the wrong answers
        /// </summary>
        public string AnswerThree { get; set; }

        /// <summary>
        /// Path to the image for this question
        /// </summary>
        [Ignored]
        public string ImagePath { get; set; }

        /// <summary>
        /// if the question needs a picture
        /// </summary>
        public bool NeedsPicture { get; set; }

        /// <summary>
        /// Primary key ID for database Once set, IT CAN NEVER BE CHANGED.
        /// </summary>
        [PrimaryKey]
        public string QuestionId { get; set; }

        /// <summary>
        /// the question itself
        /// </summary>
        public string QuestionText { get; set; }

        /// <summary>
        /// The type of the question
        /// </summary>
        // 0 = Multiple choice, 1 = Text answer w/o upper/lower case, 2 = Text answer with upper/lower case
        public int QuestionType { get; set; }

        /// <summary>
        /// if the question has been attempted, answered, or neither 0 = unattemped, 1 = answered wrong, 2 = answered correctly
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// For sorting questions by status
        /// </summary>
        /// <param name="obj">  </param>
        /// <returns>  </returns>
        public int CompareTo(object obj)
        {
            return this.Status.CompareTo(((Question)obj).Status);
        }
    }
}