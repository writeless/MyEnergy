using EdpConsole.Connectors.Usb;
using EdpConsole.Core;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading.Tasks;

namespace EdpConsole.Connectors
{
    public interface IConnector : IDisposable
    {
        uint EntriesInUse { get; }
        MeasurementConfiguration MeasurementConfiguration { get; }

        void Open();

        Task LoadConfiguration();

        Task<ModbusResponse<TResponse>> SendMessageAsync<TResponse>(ModbusMessage message);
    }
}
