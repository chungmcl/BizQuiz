//BizQuiz App 2019



using Realms;
using System;
using System.Collections.Generic;

namespace appFBLA2019
{
    public class ScoreRecord : RealmObject, IComparable
    {
        #region Public Constructors

        public ScoreRecord(double score)
        {
            this.DateTime = System.DateTime.Now.ToShortDateString() + System.DateTime.Now.ToShortTimeString();
            this.Score = score;
        }

        public ScoreRecord()
        { }

        #endregion Public Constructors

        #region Public Properties + Fields

        public string DateTime { get; set; }
        public double Score { get; set; }

        #endregion Public Properties + Fields

        #region Public Methods

        public int CompareTo(object obj)
        {
            return this.Score.CompareTo(((ScoreRecord)obj).Score);
        }

        #endregion Public Methods
    }
}