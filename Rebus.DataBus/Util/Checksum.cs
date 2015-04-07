using System;
using System.IO;
using System.Security.Cryptography;

namespace Rebus.DataBus.Util
{
    public static class Checksum
    {
        /// <summary>
        /// Get Sha256 with a buffered stream, THAT WILL NOT BE CLOSED!
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static string GetSha256HashBuffered(Stream stream)
        {
            var bufferedStream = new BufferedStream(stream, 1024*32); //todo check if this is the best buffersize
            
            var sha = new SHA256Managed();
            byte[] checksum = sha.ComputeHash(bufferedStream);
            return BitConverter.ToString(checksum).Replace("-", String.Empty);            
        }
    }
}
        