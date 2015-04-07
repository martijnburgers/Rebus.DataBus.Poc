namespace Rebus.DataBus
{
    public interface IDataBusCompressedProperty : IDataBusProperty
    {
        
    }

    public interface IDataBusProperty
    {
        string ClaimKey { get; set; }
        object GetValue();
        void SetValue(object value);
        bool HasValue { get; set; }
        string Checksum { get; set; }        
    }
}