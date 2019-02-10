//BizQuiz App 2019

using System;
using System.Collections.Generic;
using System.IO;

namespace appFBLA2019
{
    public interface IGetImage
    {
        Stream GetJPGStreamFromByteArray(byte[] image);
    }
}