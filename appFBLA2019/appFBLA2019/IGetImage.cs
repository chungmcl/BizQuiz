//BizQuiz App 2019

using System.IO;

namespace appFBLA2019
{
    public interface IGetImage
    {
        /// <summary>
        /// Convert a byte[] into a JPG stream using device-specific APIs
        /// </summary>
        /// <param name="image">Byte[] to convert</param>
        /// <returns>JPG stream result</returns>
        Stream GetJPGStreamFromByteArray(byte[] image);
    }
}