using System;
using System.Collections.Generic;
using System.Text;

namespace appFBLA2019
{
    public struct ScoreRecord : IComparable
    {
        public string DateTime { get; set; }
        public double Score { get; set; }

        public ScoreRecord(double score)
        {
            this.DateTime = System.DateTime.Now.ToString();
            this.Score = score;
        }

        public int CompareTo(object obj)
        {
            return this.Score.CompareTo(((ScoreRecord)obj).Score);
        }
    }
}
