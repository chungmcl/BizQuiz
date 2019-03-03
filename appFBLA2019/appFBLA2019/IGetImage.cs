//BizQuiz App 2019

using System.IO;

namespace appFBLA2019
{
    public interface IGetImage
    {
        Stream GetJPGStreamFromByteArray(byte[] image);
    }
}