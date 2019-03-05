//BizQuiz App 2019

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace appFBLA2019
{
    public interface IGetStorage
    {
       string GetStorage();

       Task SetupDefaultLevelsAsync(string userpath);
    }
}