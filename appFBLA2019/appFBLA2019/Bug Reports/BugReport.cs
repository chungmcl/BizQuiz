//BizQuiz App 2019

using Realms;
using System;

namespace appFBLA2019
{
    public class BugReport : RealmObject
    {
        /// <summary>
        /// Constructor for a bug report with an image
        /// </summary>
        /// <param name="title">title of the bug report</param>
        /// <param name="category">what part of the app the report is related to</param>
        /// <param name="body">the text of the bug report</param>
        /// <param name="imagePath">where the relevant image is stored</param>
        public BugReport(string title, string category, string body, string imagePath) : this(title, category, body)
        {
            this.ImagePath = imagePath;
            this.ReportID = this.ToString().GetHashCode();
        }

        /// <summary>
        /// A constructor for bug reports without an image
        /// </summary>
        /// <param name="title">title of the bug report</param>
        /// <param name="category">what part of the app the report is related to</param>
        /// <param name="body">the text of the bug report</param>
        public BugReport(string title, string category, string body)
        {
            this.Title = title;
            this.Category = category;
            this.Body = body;
            this.DateAndTime = $"{DateTime.Now.ToShortDateString()} at {DateTime.Now.ToLongTimeString()}";
            this.ReportID = this.ToString().GetHashCode();
        }

        /// <summary>
        /// Default constructor for a bug report. not used
        /// </summary>
        public BugReport()
        {
            this.ReportID = this.ToString().GetHashCode();
        }

        /// <summary>
        /// Title of the bug report
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Part of the app that the bug report is about
        /// </summary>
        public string Category { get; set; }
        /// <summary>
        /// Text of the bug report
        /// </summary>
        public string Body { get; set; }
        /// <summary>
        /// If an image is required, where to find it
        /// </summary>
        public string ImagePath { get; set; }
        /// <summary>
        /// Date and time the bug report was created
        /// </summary>
        public string DateAndTime { get; set; }

        /// <summary>
        /// unique hash of the bug report, used to differentiate
        /// </summary>
        [PrimaryKey]
        public int ReportID { get; private set; }

        /// <summary>
        /// Turns the bug report into a string for sending to server
        /// </summary>
        /// <returns>A string representing the bug report</returns>
        public override string ToString()
        {
            return $"{this.DateAndTime}\n{this.Title}\n{this.Category}\n{this.ReportID}\n{this.Body}";
        }
    }
}