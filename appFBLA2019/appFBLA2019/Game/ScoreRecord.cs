using System;
using System.Collections.Generic;
using System.Text;
using Realms;

namespace appFBLA2019
{
    public class ScoreRecord : RealmObject, IComparable
    {
        public IList<double> Scores { get; set; }
        public string LevelName

        public ScoreRecord(double score)
        {
            this.DateTime = System.DateTime.Now.ToString();
            this.Score = score;
        }

        public ScoreRecord()
        { }

        public int CompareTo(object obj)
        {
            return this.Score.CompareTo(((ScoreRecord)obj).Score);
        }
    }
}
