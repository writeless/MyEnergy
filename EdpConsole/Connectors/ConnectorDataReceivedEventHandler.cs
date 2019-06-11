using System;
using System.Collections.Generic;
using System.Text;

namespace EdpConsole.Connectors
{
    public delegate void ConnectorDataReceivedEventHandler(IConnector sender, byte[] dataReceived);
}
