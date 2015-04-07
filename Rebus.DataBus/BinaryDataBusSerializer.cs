using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Rebus.DataBus
{
    public class BinaryDataBusSerializer : IDataBusSerializer
    {
        private static readonly BinaryFormatter Formatter = new BinaryFormatter();

        public void Serialize(object databusProperty, Stream stream)
        {
            Formatter.Serialize(stream, databusProperty);
        }

        public object Deserialize(Stream stream)
        {
            return Formatter.Deserialize(stream);
        }
    }
}