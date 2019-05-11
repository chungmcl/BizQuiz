//BizQuiz App 2019

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace appFBLA2019
{
    public interface IGetStorage
    {
        /// <summary>
        /// Gets the local path for device storage
        /// </summary>
        /// <returns></returns>
        string GetStorage();

        /// <summary>
        /// Copies prepacked levels into local storage
        /// </summary>
        /// <param name="userpath">Path to the user folder</param>
        /// <returns></returns>
        Task SetupDefaultQuizzesAsync(string userpath);


        /// <summary>
        /// Check if an Android package is installed on the current device
        /// </summary>
        /// <param name="packageName">A bool stating whether the package specified is installed or not</param>
        /// <returns></returns>
        bool IsPackageInstalled(string packageName);
    }
}