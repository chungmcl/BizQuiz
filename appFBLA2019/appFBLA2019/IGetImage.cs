using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace appFBLA2019
{
    public interface IGetImage
    {
        Stream GetJPGStreamFromByteArray(byte[] image);
    }
}
