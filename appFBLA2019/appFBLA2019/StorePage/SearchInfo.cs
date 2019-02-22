using System;
using System.Collections.Generic;
using System.Text;

namespace appFBLA2019
{
    internal class SearchInfo
    {
        public string DBId { get; set; }
        public string Author { get; set; }
        public string LevelName { get; set; }
        public string Category { get; set; }
        public int SubCount { get; set; }

        internal SearchInfo(params string[] data)
        {
            this.DBId = data[0];
            this.Author = data[1];
            this.LevelName = data[2];
            this.Category = data[3];
            this.SubCount = int.Parse(data[4]);
        }
    }
}
