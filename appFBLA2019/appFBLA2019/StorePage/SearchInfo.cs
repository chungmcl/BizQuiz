namespace appFBLA2019
{
    /// <summary>
    /// Data usefull to the Search and Featured Pages
    /// </summary>
    public class SearchInfo
    {
        /// <summary>
        /// title of the search result
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// DBID of the search result
        /// </summary>
        public string DBId { get; set; }
        /// <summary>
        /// Author of the search result
        /// </summary>
        public string Author { get; set; }
        /// <summary>
        /// name of the search result
        /// </summary>
        public string QuizName { get; set; }
        /// <summary>
        /// category of the search result
        /// </summary>
        public string Category { get; set; }
        /// <summary>
        /// subscriber count of the search result
        /// </summary>
        public int SubCount { get; set; }

        /// <summary>
        /// Creates a search info
        /// </summary>
        /// <param name="dbId">ID of the search info</param>
        /// <param name="author">Author of the search info</param>
        /// <param name="quizName">Name of the quiz that is tied to this search info</param>
        /// <param name="category">category of this search info</param>
        /// <param name="subCount">sub count for this search info</param>
        public SearchInfo(string dbId, string author, string quizName, string category, string subCount)
        {
            this.DBId = dbId;
            this.Author = author;
            this.QuizName = quizName;
            this.Category = category;
            this.SubCount = int.Parse(subCount);
        }

        /// <summary>
        /// default constructor
        /// </summary>
        public SearchInfo()
        { }
    }
}
