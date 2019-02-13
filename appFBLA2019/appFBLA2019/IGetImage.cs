//BizQuiz App 2019

using System.Collections.Generic;
using System;
using System.IO;

namespace appFBLA2019
{
    public interface IGetImage
    {
        Stream GetJPGStreamFromByteArray(byte[] image);
    }
}