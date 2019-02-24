using Realms;
using System;

namespace appFBLA2019
{
    public class LevelInfo : RealmObject
    {
        [PrimaryKey]
        public string DBId { get; set; }
        public string AuthorName { get; set; }
        public string LevelName { get; set; }
        public string Category { get; set; }
        public string LastModifiedDate { get; set; }

        /// <summary>
        ///  0 = Need Download, 1 = Need Upload, 2 = Synced, 3 = Offline, 4 = Not downloaded + Need download
        /// </summary>
        public int SyncStatus { get; set; }
        public bool IsDeletedLocally { get; set; }
        public bool IsDeletedOnServer { get; set; }
        
        public LevelInfo() { }

        public LevelInfo(string authorName, string levelName, string category)
        {
            this.DBId = Guid.NewGuid().ToString();
            this.AuthorName = authorName;
            this.LevelName = levelName;
            this.Category = category;

            this.LastModifiedDate = DateTime.Now.ToString();
            this.SyncStatus = 1;
            this.IsDeletedLocally = false;
            this.IsDeletedOnServer = false;
        }

        public LevelInfo(LevelInfo levelInfoToCopy)
        {
            this.DBId = levelInfoToCopy.DBId;
            this.AuthorName = levelInfoToCopy.AuthorName;
            this.LevelName = levelInfoToCopy.LevelName;
            this.Category = levelInfoToCopy.Category;

            this.LastModifiedDate = levelInfoToCopy.LastModifiedDate;
            this.SyncStatus = levelInfoToCopy.SyncStatus;
            this.IsDeletedLocally = levelInfoToCopy.IsDeletedLocally;
            this.IsDeletedOnServer = levelInfoToCopy.IsDeletedOnServer;
        }
    }
}
