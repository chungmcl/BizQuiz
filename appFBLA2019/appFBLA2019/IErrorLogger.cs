//BizQuiz App 2019


namespace appFBLA2019
{
    public interface IErrorLogger
    {
        /// <summary>
        /// Logs an error with device diagnostics
        /// </summary>
        /// <param name="error"></param>
        void LogError(string error);
    }
}