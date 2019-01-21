using System;
using System.Collections.Generic;
using System.Text;
using Realms;

namespace appFBLA2019
{
    /// <summary>
    /// The question object holds both the question text and answers for a particular question to ask the user.
    /// </summary>
    public class Question : RealmObject, IComparable
    {
        // Primary key ID for database
        // Once set, IT CAN NEVER BE CHANGED.
        [PrimaryKey]
        public string DBId { get; set; }
        // SQLITE WILL IGNORE ALL PROPERTIES THAT ARE NOT DEFINED BY public { get; set; }

        //declare enum (to use this type other places, say Question.QuestionStatus)
        //public enum QuestionStatus { Unanswered, Failed, Correct }
        public int Status { get; set; }

        // 0 = Multiple choice, 1 = Text answer w/o upper/lower case, 2 = Text answer with upper/lower case
        public int QuestionType { get; set; }
        public string QuestionText { get; set; }
        public string CorrectAnswer { get; set; }
        public bool NeedsPicture { get; set; }

        [Ignored]
        public byte[] ImageByteArray { get; set; }

        public string[] Answers
        {
            //turns the 4 answers into an easy to use array
            get
            { return new string[] { this.AnswerOne, this.AnswerTwo, this.AnswerThree, this.AnswerFour }; }
            //takes an array, makes sure it can be used in our answers, and assigns it to the answers
            private set
            {
                //temp has empty spaces so we dont assign from a null spot in value
                string[] temp = new string[4];
                if (value.Length > 4)
                    throw new ArgumentException("You can only have 4 answers!");
                value.CopyTo(temp, 0);

                //makes sure there's actually a value to assign, otherwise makes it empty
                this.AnswerOne = temp[0]?.Split('/')[1] ?? "";
                this.AnswerTwo = temp[1]?.Split('/')[1] ?? "";
                this.AnswerThree = temp[2]?.Split('/')[1] ?? "";
                this.AnswerFour = temp[3]?.Split('/')[1] ?? "";

                //once the answers are assigned, find the correct one and assign it to CorrectAnswer
                foreach (string answer in temp)
                {
                    if (answer != null && answer[0] == 'c')
                        this.CorrectAnswer = answer.Split('/')[1];
                }
            }
        }

        public string AnswerOne { get; set; }
        public string AnswerTwo { get; set; }
        public string AnswerThree { get; set; }
        public string AnswerFour { get; set; }


        /// <summary>
        /// Creates a question object given the question and answers
        /// </summary>
        /// <param name="question">What to ask the user</param>
        /// <param name="answersIn">
        /// The potential answers to the question. The correct answer will be prefixed with "c/".
        /// The rest will be prefixed with "x/".
        /// </param>
        public Question(string question, byte[] image, params string[] answers)
        {
            this.QuestionText = question;
            //tries to assign the params to the local Answers property which in turn assigns the fields
            //if answers is null, throws an exception
            this.Status = 0;
            this.Answers = answers ?? throw new ArgumentException("Must have at least one answer!");
            image.CopyTo(this.ImageByteArray, 0);
        }

        public Question() : this ("This question is empty!", new byte[0], new string[0])
        {

        }

        //used to sort questions by status
        public int CompareTo(object obj)
        {
            return this.Status.CompareTo(((Question)obj).Status);
        }
    }
}
