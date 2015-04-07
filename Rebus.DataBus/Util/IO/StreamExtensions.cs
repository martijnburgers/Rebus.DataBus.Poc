using System.IO;
using System.IO.Compression;
using Rebus.Logging;

namespace Rebus.DataBus.Util.IO
{
    public static class StreamExtensions
    {
        private static ILog _log;

        static StreamExtensions()
        {
            RebusLoggerFactory.Changed += f => _log = f.GetCurrentClassLogger();
        }

        public static Stream RemoveCompression(this FileStream compressedFileStream)
        {
            MemoryStream uncompressed = new MemoryStream();

            _log.Info("Decompressing filestream: {0}", compressedFileStream.Name);

            using (compressedFileStream)
            using (GZipStream decompressor = new GZipStream(compressedFileStream, CompressionMode.Decompress))
            {
                decompressor.CopyTo(uncompressed);
            }

            return uncompressed;
        }

        public static Stream AddCompressor(this FileStream fileStream)
        {

            _log.Info("Adding compression to filestream: {0}", fileStream.Name);

            return new GZipStream(fileStream, CompressionMode.Compress);
        }
    }
}