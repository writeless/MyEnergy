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
        event ConnectorDataReceivedEventHandler DataReceived;

        void SendMessage(ModbusMessage message);

        Task<ModbusResponse> SendMessageAsync(ModbusMessage message);
    }
}
