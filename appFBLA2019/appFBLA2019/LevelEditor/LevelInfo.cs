//BizQuiz App 2019

using System;
using System.Collections.Generic;

namespace appFBLA2019.LevelEditor
{
    internal class LevelInfo : Realms.RealmObject
    {
        public string Author { get; set; }
        public string Category { get; set; }
        public string DateEdited { get; set; }
        public string LevelID { get; set; }
    }
}