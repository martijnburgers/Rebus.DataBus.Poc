using System.Transactions;

namespace Rebus.DataBus.Configuration
{
    public interface IDataBusSettings
    {
        bool IsChecksummingEnabled { get; set; }
        bool IsCompressionEnabled { get; set; }
        TransactionScopeOption TransactionScope { get; set; }
    }

    public class DataBusSettings : IDataBusSettings
    {
        public DataBusSettings()
        {
            TransactionScope = TransactionScopeOption.Suppress;
        }

        public bool IsChecksummingEnabled { get; set; }
        public bool IsCompressionEnabled { get; set; }
        public TransactionScopeOption TransactionScope { get; set; }
    }
}