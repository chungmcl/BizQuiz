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
        public readonly string[] answers;
        public readonly string correctAnswer;

        /// <summary>
        /// Creates a question object given the question and answers
        /// </summary>
        /// <param name="text">What to ask the user</param>
        /// <param name="answersIn">
        /// The potential answers to the question. The correct answer will be prefixed with "correct/".
        /// The rest will be prefixed with "wrong/".
        /// </param>
        public Question(string text, params string[] answersIn)
        {
            this.QuestionText = text;
            this.answers = new string[answersIn.Length];

            for (int answersProcessed = 0; answersProcessed < answersIn.Length; answersProcessed++)
            {
                string answer = answersIn[answersProcessed];
                if (answer.Split('/')[0] == "c")
                {
                    this.correctAnswer = answer.Split('/')[1];
                }
                this.answers[answersProcessed] = answer.Split('/')[1];
            }
        }

        // parameterless constructor for SQLite compatibility
    }
}
