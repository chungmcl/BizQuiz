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
        public BugReport(string title, string category, string body, string imagePath) : this(title, category, body)
        {
            this.ImagePath = imagePath;
        }

        public BugReport(string title, string category, string body) : this()
        {
            this.Title = $"Bug on {DateTime.Now.ToShortDateString()} at {DateTime.Now.ToLongTimeString()}: {title}";
            this.Body = body;
        }

        public BugReport()
        {
            this.ReportID = this.ToString().GetHashCode();
        }

        private string Title { get; set; }
        private string Category { get; set; }
        private string Body { get; set; }
        public string ImagePath { get; set; }

        [PrimaryKey]
        public int ReportID { get; private set; }

        public override string ToString()
        {
            return $"{this.Title}`{this.Category}`{this.Body}`{this.ReportID}";
        }

        public int Length
        {
            get { return this.ToString().Length; }
        }
    }
}