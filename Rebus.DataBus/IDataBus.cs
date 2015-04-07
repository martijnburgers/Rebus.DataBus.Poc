using System.IO;

namespace Rebus.DataBus
{    
    public interface IDataBus
    {       
        Stream Get(string key, bool removeCompression);       
        string Put(Stream stream, bool addCompression);        
    }
}