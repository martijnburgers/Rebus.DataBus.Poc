using System;
using System.Text;
using Rebus.DataBus;

namespace Messages
{
    public class Job
    {
        public int JobNumber { get; set; }

        public DataBusCompressedProperty<byte[]> CompressedBlob { get; set; }
        public DataBusProperty<byte[]> NotCompressedBlob { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(String.Format("JobNumber: {0}", JobNumber));
            sb.AppendLine(String.Format("CompressedBlob: {0}", GetStringForDataBusProperty(CompressedBlob)));
            sb.AppendLine(String.Format("NotCompressedBlob: {0}", GetStringForDataBusProperty(NotCompressedBlob)));

            return sb.ToString();
        }

        private string GetStringForDataBusProperty(DataBusProperty<byte[]> property)
        {
            if (property == null)
                return "Null";

            return String.Format(
                "\n\r - Length {0} \n\r - Checksum: {1} ",
                property.HasValue
                    ? property.Value.LongLength.ToString()
                    : "Null",
                property.Checksum ?? "Null");
        }
    }
}