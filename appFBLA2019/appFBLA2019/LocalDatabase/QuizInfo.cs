using Realms;
using System;

namespace appFBLA2019
{
    public class QuizInfo : RealmObject
    {
        [PrimaryKey]
        public string DBId { get; set; }
        public string AuthorName { get; set; }
        public string QuizName { get; set; }
        public string Category { get; set; }
        public string LastModifiedDate { get; set; }

        /// <summary>
        ///  0 = Need Download, 1 = Need Upload, 2 = Synced, 3 = Offline, 4 = Not downloaded + Need download
        /// </summary>
        public int SyncStatus { get; set; }
        public bool IsDeletedLocally { get; set; }
        public bool IsDeletedOnServer { get; set; }
        
        public QuizInfo() { }

        public QuizInfo(string authorName, string quizName, string category)
        {
            this.DBId = Guid.NewGuid().ToString();
            this.AuthorName = authorName;
            this.QuizName = quizName;
            this.Category = category;

            this.LastModifiedDate = DateTime.Now.ToString();
            this.SyncStatus = 1;
            this.IsDeletedLocally = false;
            this.IsDeletedOnServer = false;
        }

        public QuizInfo(QuizInfo quizInfoToCopy)
        {
            this.DBId = quizInfoToCopy.DBId;
            this.AuthorName = quizInfoToCopy.AuthorName;
            this.QuizName = quizInfoToCopy.QuizName;
            this.Category = quizInfoToCopy.Category;

            this.LastModifiedDate = quizInfoToCopy.LastModifiedDate;
            this.SyncStatus = quizInfoToCopy.SyncStatus;
            this.IsDeletedLocally = quizInfoToCopy.IsDeletedLocally;
            this.IsDeletedOnServer = quizInfoToCopy.IsDeletedOnServer;
        }
    }
}
