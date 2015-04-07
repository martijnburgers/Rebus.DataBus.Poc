using System;
using System.IO;
using Rebus.DataBus.Util.IO;
using Rebus.Logging;

namespace Rebus.DataBus
{
    //todo use System.IO.Abstractions?
    //todo support crypto
    public class FileShareDataBus : IDataBus
    {
        private static ILog _log;
        private readonly string _basePath;

        public FileShareDataBus(string basePath)
        {
            if (basePath == null) throw new ArgumentNullException("basePath");

            _basePath = basePath;

            _log.Info("{0} created with basePath: '{1}'.", GetType().FullName, basePath);
        }

        static FileShareDataBus()
        {
            RebusLoggerFactory.Changed += f => _log = f.GetCurrentClassLogger();
        }

        //todo remove the need for passing through the removeCompression flag.
        public Stream Get(string key, bool removeCompression)
        {
            _log.Info("Incoming GET request for key: '{0}'.", key);

            string path = Path.Combine(_basePath, key);

            _log.Info("Opening filestream for path: '{0}'.", path);

            var inputStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            return removeCompression
                ? inputStream.RemoveCompression()
                : inputStream;                  
        }

        public string Put(Stream stream, bool addCompression)
        {
            string claimKey = GenerateKey();

            _log.Info("Generated claim key '{0}' for incoming PUT request.", claimKey);

            string combinedPath = Path.Combine(_basePath, claimKey);

            string directoryName = Path.GetDirectoryName(combinedPath);

            if (directoryName == null)
            {
                _log.Warn("Unable to create directory name for path '{0}'.", combinedPath);
            }

            Directory.CreateDirectory(directoryName);

            using (Stream outputStream = CreateOutputStream(combinedPath, addCompression))
            {                         
                stream.CopyTo(outputStream);                    
            }

            _log.Info("Done copying stream to databus, file '{0}'.", combinedPath);
            
            return claimKey;
        }

        private Stream CreateOutputStream(string path, bool compressStream)
        {
            FileStream outputStream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read);

            return compressStream
                ? outputStream.AddCompressor()
                : outputStream;
        }

        private string GenerateKey()
        {
            var current = DateTime.Now;

            return Path.Combine(current.ToString("yyyy-MM-dd"), current.ToString("HH"), Guid.NewGuid().ToString());
        }  
    }
}