//BizQuiz App 2019

using Realms;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace appFBLA2019
{
    public class BugReport : RealmObject
    {
        public BugReport(string title, string category, string body)
        {
            this.title = title;
            this.category = category;
            this.body = body;
        }

        public BugReport()
        {
        }

        private readonly string title;
        private readonly string category;
        private readonly string body;
        public Image image;

        [PrimaryKey]
        private int ReportID { get; set; } = Guid.NewGuid().GetHashCode();

        public override string ToString()
        {
            return $"{this.title}`{this.category}`{this.body}`{this.ReportID}";
        }

        public int Length
        {
            get { return this.ToString().Length; }
        }
    }
}