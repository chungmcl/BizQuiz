using System;
using System.Collections.Generic;
using System.Text;

namespace appFBLA2019
{
    /// <summary>
    /// Data usefull to the Search and Featured Pages
    /// </summary>
    public class SearchInfo
    {
        public string Title { get; set; }
        public string DBId { get; set; }
        public string Author { get; set; }
        public string QuizName { get; set; }
        public string Category { get; set; }
        public int SubCount { get; set; }

        public SearchInfo(string dbId, string author, string quizName, string category, string subCount)
        {
            this.DBId = dbId;
            this.Author = author;
            this.QuizName = quizName;
            this.Category = category;
            this.SubCount = int.Parse(subCount);
        }

        public SearchInfo()
        { }
    }
}
