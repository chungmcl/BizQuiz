using System;
using System.Collections.Generic;
using System.Text;
using Realms;

namespace appFBLA2019
{
    public class ScoreRecord : RealmObject, IComparable
    {
        public string DateTime { get; set; }
        public double Score { get; set; }

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
