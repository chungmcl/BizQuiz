using Realms;
using System;

namespace appFBLA2019
{
    public class QuizInfo : RealmObject
    {
        /// <summary>
        /// The ID associated with the quiz
        /// </summary>
        [PrimaryKey]
        public string DBId { get; set; }
        /// <summary >
        /// The name of the quiz's author
        /// </summary>
        public string AuthorName { get; set; }
        /// <summary>
        /// The name of the quiz
        /// </summary>
        public string QuizName { get; set; }
        /// <summary>
        /// What category the quiz falls into
        /// </summary>
        public string Category { get; set; }
        /// <summary>
        /// The last time the quiz was modified
        /// </summary>
        public string LastModifiedDate { get; set; }

        /// <summary>
        ///  0 = Need Download, 1 = Need Upload, 2 = Synced, 3 = Offline, 4 = Not downloaded + Need download
        /// </summary>
        public int SyncStatus { get; set; }
        /// <summary>
        /// If the quiz has been deleted locally
        /// </summary>
        public bool IsDeletedLocally { get; set; }
        /// <summary>
        /// If the quiz has been deleted from the server
        /// </summary>
        public bool IsDeletedOnServer { get; set; }

        /// <summary>
        /// The relative path to the quiz folder from the application folder.
        /// This property is relative to the device the application is stored on.
        /// </summary>
        public string RelativePath { get; set; }

        public int SubscriberCount { get; set; }
        
        /// <summary>
        /// Default constructor -  doesn't initialize anything
        /// (Required for storage in Realm file)
        /// </summary>
        public QuizInfo() { }

        /// <summary>
        /// Creates a QuizInfo
        /// </summary>
        /// <param name="authorName">Name of author</param>
        /// <param name="quizName">Name of quiz</param>
        /// <param name="category">Category of quiz</param>
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

            this.RelativePath = App.UserPath + $"/{this.Category}/{this.DBId}/";
        }

        /// <summary>
        /// Creates a new QuizInfo based on the information in another
        /// </summary>
        /// <param name="quizInfoToCopy">The quizInfo to make a copy of</param>
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

            this.RelativePath = quizInfoToCopy.RelativePath;
        }
    }
}
