using Realms;
using System;

namespace appFBLA2019
{
    public class LevelInfo : RealmObject
    {
        public string DBId { get; set; }
        public string AuthorName { get; set; }
        public string LevelName { get; set; }
        public string Category { get; set; }
        public string LastModifiedDate { get; set; }
        public bool NeedUploadSync { get; set; }
        public bool NeedDownloadSync { get; set; }
        public bool IsDeleted { get; set; }
        
        public LevelInfo() { }

        public LevelInfo(string authorName, string levelName, string category)
        {
            this.DBId = Guid.NewGuid().ToString();
            this.AuthorName = authorName;
            this.LevelName = levelName;
            this.Category = category;

            this.LastModifiedDate = DateTime.Now.ToString();
            this.NeedUploadSync = true;
            this.NeedDownloadSync = false;
            this.IsDeleted = false;
        }
    }
}
