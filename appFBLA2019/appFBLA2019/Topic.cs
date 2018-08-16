using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("appFBLA2019_Tests")]
namespace appFBLA2019
{
    internal class Topic
    {
        internal List<Question> availableQuestions = new List<Question>();
        internal List<Question> usedQuestions = new List<Question>();

        internal string title { get; private set; }
        internal Topic(string path)
        {
            try
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    title = reader.ReadLine();
                    while (!reader.EndOfStream)
                    {
                        List<string> answers = new List<string>();
                        string question = "Something went wrong!";
                        try
                        {
                            question = reader.ReadLine();
                            while (reader.Peek() == '*')
                            {
                                answers.Add(reader.ReadLine().Substring(1));

                            }
                        }
                        catch (NullReferenceException)
                        { //do nothing, just finish the question with what we have here}

                        }
                        this.availableQuestions.Add(new Question(question, answers.ToArray()));
                    }
                }
            }
            catch (FileNotFoundException)
            {
                //fix it
            }
        }


        public void Set()
        {
            while (usedQuestions.Count > 0)
            {
                availableQuestions.Add(usedQuestions[0]);
                usedQuestions.RemoveAt(0);
            }
        }
        public Question GetQuestion()
        {
            System.Random rand = new Random();
            int questionIndex = rand.Next(availableQuestions.Count);
            Question returnQuestion = availableQuestions[questionIndex];
            availableQuestions.RemoveAt(questionIndex);
            usedQuestions.Add(returnQuestion);
            return returnQuestion;
        }
    }
}
