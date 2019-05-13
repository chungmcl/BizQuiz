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
        /// The path to the Quiz folder. This property is variable and is dependent on the device BizQuiz is installed on.
        /// This property is not persisted in the Realm file.
        /// </summary>
        [Ignored]
        public string RelativePath { get { return $"{App.UserPath}/{this.Category}/{this.DBId}/"; } }

        /// <summary>
        /// The total number of subscribers to the quiz this QuizInfo is associated with.
        /// (This field is persisted within the realm file BUT is NOT updated on the server's copy
        /// of the realm file. Update the roster and refer to the roster's data on subscriber count for
        /// an accurate number.)
        /// </summary>
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
            this.SyncStatus = (int)SyncStatusEnum.NeedUpload;
            this.IsDeletedLocally = false;
            this.IsDeletedOnServer = false;
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

            this.SubscriberCount = quizInfoToCopy.SubscriberCount;
        }
    }
}

/// <summary>
/// Enumerator detailing sync status
///  0 = Need Download, 1 = Need Upload, 2 = Synced, 3 = Offline, 4 = Not downloaded + Need download
/// </summary>
public enum SyncStatusEnum { NeedDownload, NeedUpload, Synced, Offline, NotDownloadedAndNeedDownload }


/// <summary>
/// The UI representation of sync status of a quiz
/// </summary>
public enum UISyncStatus { Offline = 1, Upload, Download, NoChange, Syncing };
