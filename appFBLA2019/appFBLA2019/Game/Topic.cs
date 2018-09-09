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
    //leave this here, it can be the filebased constructor
        public Topic(string path)
        {
            try
            {
                using (StreamReader reader = new StreamReader(Path.GetFullPath(path)))
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
                        currentSet.Insert(new Random().Next(currentSet.Count), new Question(question, answers.ToArray()));
                    }
                }
            }
            catch (FileNotFoundException)
            {
                //fix it
            }
        }

        //this one will be used in the app for database stuff
        public Topic(/* idk whatever SQL data we need*/)
        {

        }

        public void SaveState()
        {
            //write 
        }
        public Question GetQuestion()
        {
            System.Random rand = new Random();

            int questionIndex;
            Question returnQuestion = new Question();
            switch (rand.Next(1))
            {
                case 0:
                    questionIndex = rand.Next(this.freshQuestions.Count);
                    returnQuestion = this.freshQuestions[questionIndex];
                    this.freshQuestions.RemoveAt(questionIndex);
                    return returnQuestion;
                case 1:
                    questionIndex = rand.Next(this.failedQuestions.Count);
                    returnQuestion = this.failedQuestions[questionIndex];
                    this.failedQuestions.RemoveAt(questionIndex);
                    return returnQuestion;
                default:
                    return new Question();
            }

            
        }

        public void UpdateQuestionInfo(Question question, bool answeredCorrectly)
        {
            if (answeredCorrectly)
                correctQuestions.Add(question);
            else
                failedQuestions.Add(question);
        }
    }
}
