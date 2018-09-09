using System;
using System.Collections.Generic;
using System.Text;

namespace appFBLA2019
{
    /// <summary>
    /// The question object holds both the question text and answers for a particular question to ask the user.
    /// </summary>
    public class Question
    {
        public readonly string QuestionText;
        public readonly string answerOne;
        public readonly string answerTwo;
        public readonly string answerThree;
        public readonly string answerFour;
        public readonly string correctAnswer;

        /// <summary>
        /// Creates a question object given the question and answers
        /// </summary>
        /// <param name="text">What to ask the user</param>
        /// <param name="answersIn">
        /// The potential answers to the question. The correct answer will be prefixed with "correct/".
        /// The rest will be prefixed with "wrong/".
        /// </param>
        public Question(string text, string[] answers, int correctAnswer)
        {
            this.answerOne = answers[0];
            this.answerTwo = answers[1];
            this.answerThree = answers[2];
            this.answerFour = answers[3];
            this.correctAnswer = answers[correctAnswer];
        }

        public Question() /*: this ("This question is empty!")*/
        {

        }
    }
}
