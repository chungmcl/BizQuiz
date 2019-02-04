using System;
using System.Collections.Generic;
using System.Text;

namespace appFBLA2019.LevelEditor
{
    class LevelInfo : Realms.RealmObject
    {
        public string Author { get; set; }
        public string DateEdited { get; set; }
        public string Category { get; set; }
        public string LevelID { get; set; }
    }
}
