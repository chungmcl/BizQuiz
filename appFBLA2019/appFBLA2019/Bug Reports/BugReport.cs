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
            this.ReportID = this.ToString().GetHashCode();
        }

        public BugReport(string title, string category, string body)
        {
            this.Title = title;
            this.Category = category;
            this.Body = body;
            this.DateAndTime = $"{DateTime.Now.ToShortDateString()} at {DateTime.Now.ToLongTimeString()}";
            this.ReportID = this.ToString().GetHashCode();
        }

        public BugReport()
        {
            this.ReportID = this.ToString().GetHashCode();
        }

        public string Title { get; set; }
        public string Category { get; set; }
        public string Body { get; set; }
        public string ImagePath { get; set; }
        public string DateAndTime { get; set; }

        [PrimaryKey]
        public int ReportID { get; private set; }

        public override string ToString()
        {
            return $"{this.DateAndTime}\n{this.Title}\n{this.Category}\n{this.ReportID}\n{this.Body}";
        }

        public int Length
        {
            get { return this.ToString().Length; }
        }
    }
}