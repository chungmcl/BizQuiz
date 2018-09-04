using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("appFBLA2019_Tests")]
namespace appFBLA2019
{
    public class Topic
    {
        internal List<Question> freshQuestions = new List<Question>();
        internal List<Question> correctQuestions = new List<Question>();
        internal List<Question> failedQuestions = new List<Question>();

        internal string title { get; private set; }
        public Topic(string path)
        {
            try
            {   
                using (StreamReader reader = new StreamReader(path))
                {
                    title = reader.ReadLine();
                    List<Question> currentSet = new List<Question>();
                    currentSet = freshQuestions;
                    while (!reader.EndOfStream)
                    {
                        switch (reader.Peek())
                        {
                            case '#':
                                currentSet = freshQuestions;
                                reader.ReadLine();
                                break;
                            case '!':
                                currentSet = failedQuestions;
                                reader.ReadLine();
                                break;
                            case '$':
                                currentSet = correctQuestions;
                                reader.ReadLine();
                                break;
                            default:
                                break;
                        }
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
                        currentSet.Add(new Question(question, answers.ToArray()));
                    }
                }
            }
            catch (FileNotFoundException)
            {
                //fix it
            }
        }


        public void SaveState()
        {
            //write 
        }
        public Question GetQuestion()
        {
            System.Random rand = new Random();
            int questionIndex = rand.Next(freshQuestions.Count);
            Question returnQuestion = freshQuestions[questionIndex];
            freshQuestions.RemoveAt(questionIndex);
            return returnQuestion;
        }
    }
}
