﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("appFBLA2019_Tests")]
namespace appFBLA2019
{
    public class Topic
    {
        List<Question> questions;
        //if the first question has already been got, then we have 100% completion
        public bool QuestionsAvailable { get { this.questions.Sort(); return this.questions[0].Status != 2; } }
        internal string title { get; private set; }
    //leave this here, it can be the filebased constructor
        public Topic(string path)
        {
            List<Question> freshQuestions = new List<Question>();
            List<Question> correctQuestions = new List<Question>();
            List<Question> failedQuestions = new List<Question>();
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

            this.questions.AddRange(freshQuestions);
            this.questions.AddRange(failedQuestions);
            this.questions.AddRange(correctQuestions);
            this.questions.Sort();
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
            return this.questions[0];
        }
    }
}
