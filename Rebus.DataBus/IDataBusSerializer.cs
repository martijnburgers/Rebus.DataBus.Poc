using System.IO;

namespace Rebus.DataBus
{
    public interface IDataBusSerializer
    {
        void Serialize(object objectToSerialize, Stream stream);
        object Deserialize(Stream stream);
    }
}