//BizQuiz App 2019

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("appFBLA2019_Tests")]

namespace appFBLA2019
{
    public class Level
    {
        //this one will be used in the app for database stuff
        /// <summary>
        /// Select/create database file for the current game/topic, Load the questions from file
        /// </summary>
        /// <param name="levelTitle"> The name of the database file - if one does not yet exist, it will create one based on the name you pass it. DO NOT INCLUDE FILE EXTENSION IN FILENAME. </param>
        /// <param name="author">     The username of the author of the level </param>
        public Level(string levelTitle, string author)
        {
            DBHandler.SelectDatabase(levelTitle, author);
        }

        public List<Question> Questions { get; set; }

        //if the first question has already been answered correctly, then we have 100% completion
        public bool QuestionsAvailable
        {
            get
            {
                this.Questions.Sort();
                if (this.Questions != null)
                {
                    return this.Questions[0].Status != 2;
                }
                return false;
            }
        }

        public int QuestionsRemaining
        {
            get
            {
                this.Questions.Sort();
                if (this.Questions != null)
                {
                    //uses a predicate to find the # of questions with a status lower than 2 (failed or unattempted)
                    return this.Questions.FindAll(x => x.Status < 2).Count;
                }
                return 0;
            }
        }

        //returns the avg score that the player gets on this level
        public static double GetLevelAvgScore(string level, string author)
        {
            DBHandler.SelectDatabase(level, author);
            return 0;
        }

        public Question GetQuestion()
        {
            if (this.Questions != null && this.Questions.Count > 0)
            {
                this.Questions.Sort();

                //randomly selects from questions that haven't been correct yet (includes unanswered)
                //to make sure you don't get the same question every time
                int availableQuestions = 0;
                for (int i = 0; i < this.Questions.Count; i++)
                {
                    if (this.Questions[i].Status != 2)
                    {
                        availableQuestions++;
                    }
                }
                // More elegant but doesn't work - out of range exception if no elements have a status of 2
                //while (!(this.Questions[availableQuestions].Status == 2))
                //{
                //    availableQuestions++;
                //}

                return this.Questions[new Random().Next(0, availableQuestions)];
            }
            return null;
        }

        public void LoadQuestions()
        {
            this.Questions = DBHandler.Database.GetQuestions();
            this.ResetLevel();
        }

        public void ResetLevel()
        {
            DBHandler.Database.realmDB.Write(() =>
            {
                foreach (Question x in this.Questions)
                {
                    x.Status = 0;
                }
            }
               );
        }

        internal string Title { get; private set; }

        //leave this here, it can be the filebased constructor

        //public Topic(string path)
        //{
        //    List<Question> freshQuestions = new List<Question>();
        //    List<Question> correctQuestions = new List<Question>();
        //    List<Question> failedQuestions = new List<Question>();
        //    try
        //    {
        //        using (StreamReader reader = new StreamReader(Path.GetFullPath(path)))
        //        {
        //            title = reader.ReadLine();
        //            List<Question> currentSet = new List<Question>();
        //            currentSet = freshQuestions;
        //            while (!reader.EndOfStream)
        //            {
        //                switch (reader.Peek())
        //                {
        //                    case '#':
        //                        currentSet = freshQuestions;
        //                        reader.ReadLine();
        //                        break;
        //                    case '!':
        //                        currentSet = failedQuestions;
        //                        reader.ReadLine();
        //                        break;
        //                    case '$':
        //                        currentSet = correctQuestions;
        //                        reader.ReadLine();
        //                        break;
        //                    default:
        //                        break;
        //                }
        //                List<string> answers = new List<string>();
        //                string question = "Something went wrong!";
        //                try
        //                {
        //                    question = reader.ReadLine();
        //                    while (reader.Peek() == '*')
        //                    {
        //                        answers.Add(reader.ReadLine().Substring(1));
        //                    }
        //                }
        //                catch (NullReferenceException)
        //                { //do nothing, just finish the question with what we have here}
        //                }
        //                currentSet.Insert(new Random().Next(currentSet.Count), new Question(question, answers.ToArray()));
        //            }
        //        }
        //    }
        //    catch (FileNotFoundException)
        //    {
        //        //fix it
        //    }

        //    this.questions.AddRange(freshQuestions);
        //    this.questions.AddRange(failedQuestions);
        //    this.questions.AddRange(correctQuestions);
        //    this.questions.Sort();
        //}

        private void GetImageNames()
        {
            // Replace "DependencyService... .GetStorage()" with the location where the databases are being stored when the app is is released (See DBHandler)
            DirectoryInfo dInfo = new DirectoryInfo(App.Path);

            List<FileInfo> files = dInfo.GetFiles("*.jpg").ToList();
            files.AddRange(dInfo.GetFiles("*.jpeg").ToList());
            List<string> imageNames = new List<string>();
            foreach (FileInfo file in files)
            {
                imageNames.Add(file.Name);
            }

            // TO DO: Generate list of file locations and store as property so TextGame can access
        }
    }
}