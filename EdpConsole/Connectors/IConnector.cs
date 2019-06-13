using EdpConsole.Connectors.Usb;
using EdpConsole.Core;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;

namespace EdpConsole.Connectors
{
    public interface IConnector
    {
        event ConnectorDataReceivedEventHandler DataReceived;

        void Open();

        void SendMessage(ModbusMessage message);
    }
}
