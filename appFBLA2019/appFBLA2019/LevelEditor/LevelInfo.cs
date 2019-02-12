//BizQuiz App 2019

using System;
using System.Collections.Generic;
using System.Threading;
using System.Globalization;

namespace appFBLA2019
{
    /// <summary>
    /// Holds onto information needed for a level, like Catagory
    /// </summary>
    public class LevelInfo : Realms.RealmObject
    {
        public string Category { get; set; }
        public string DateEdited { get; set; }

        public LevelInfo()
        {
            this.Category = "No catagory";
            this.DateEdited = DateTime.Now.ToShortDateString();
        }

        public LevelInfo(string category)
        {
            this.Category = category;
            this.DateEdited = DateTime.Now.ToShortDateString();
        }
    }
}