using System;
using System.Collections.Generic;
using System.Text;

namespace appFBLA2019
{
    public class SearchInfo
    {
        public string Title { get; set; }
        public string DBId { get; set; }
        public string Author { get; set; }
        public string QuizName { get; set; }
        public string Category { get; set; }
        public int SubCount { get; set; }

        public SearchInfo(params string[] data)
        {
            this.DBId = data[0];
            this.Author = data[1];
            this.QuizName = data[2];
            this.Category = data[3];
            this.SubCount = int.Parse(data[4]);
        }

        public SearchInfo()
        { }
    }
}
