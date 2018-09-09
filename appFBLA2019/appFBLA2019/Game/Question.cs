using System;
using System.Collections.Generic;
using System.Text;

namespace appFBLA2019
{
    /// <summary>
    /// The question object holds both the question text and answers for a particular question to ask the user.
    /// </summary>
    public class Question : IComparable
    {
        //declare enum (to use this type other places, say Question.QuestionStatus
        public enum QuestionStatus { Unanswered, Failed, Correct}

        public QuestionStatus Status { get { return (QuestionStatus)this.status; } set { this.status = (int)value; } }
        private int status;
        public readonly string QuestionText;
        public string CorrectAnswer { get { return correctAnswer; } private set { this.correctAnswer = value; } }
        private string correctAnswer;

        public string[] Answers
        {
            //turns the 4 answers into an easy to use array
            get
            { return new string[] { answerOne, answerTwo, answerThree, answerFour }; }
            //takes an array, makes sure it can be used in our answers, and assigns it to the answers
            private set
            {
                //temp has empty spaces so we dont assign from a null spot in value
                string[] temp = new string[4];
                if (value.Length > 4)
                    throw new ArgumentException("You can only have 4 answers!");
                value.CopyTo(temp, 0);

                //makes sure there's actually a value to assign, otherwise makes it empty
                this.answerOne = temp[0]?.Split('/')[1] ?? "";
                this.answerTwo = temp[1]?.Split('/')[1] ?? "";
                this.answerThree = temp[2]?.Split('/')[1] ?? "";
                this.answerFour = temp[3]?.Split('/')[1] ?? "";

                //once the answers are assigned, find the correct one and assign it to CorrectAnswer
                foreach (string answer in temp)
                {
                    if (answer != null && answer[0] == 'c')
                        this.correctAnswer = answer.Split('/')[1];
                }
            }
        }
        private string answerOne;
        private string answerTwo;
        private string answerThree;
        private string answerFour;


        /// <summary>
        /// Creates a question object given the question and answers
        /// </summary>
        /// <param name="text">What to ask the user</param>
        /// <param name="answersIn">
        /// The potential answers to the question. The correct answer will be prefixed with "c/".
        /// The rest will be prefixed with "x/".
        /// </param>
        public Question(string text, params string[] answers)
        {
            this.QuestionText = text;
            //tries to assign the params to the local Answers property which in turn assigns the fields
            //if answers is null, throws an exception
            this.status = 0;
            this.Answers = answers ?? throw new ArgumentException("Must have at least one answer!");
        }

        public Question() : this ("This question is empty!", new string[0])
        {

        }

        //used to sort questions by status
        public int CompareTo(object obj)
        {
            return Math.Sign(((int)obj).CompareTo(this.status));
        }
    }
}
