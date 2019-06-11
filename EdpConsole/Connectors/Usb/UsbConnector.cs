using EdpConsole.Extensions;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;

namespace EdpConsole.Connectors.Usb
{
    public class UsbConnector : IConnector
    {
        private const int baudRate = 115200;
        private const Parity parity = Parity.None;
        private const int dataBits = 8;
        private const StopBits stopBits = StopBits.One;

        private readonly int _baudRate;
        private readonly Parity _parity;
        private readonly int _dataBits;
        private readonly StopBits _stopBits;

        private SerialPort _serialPort;

        public event ConnectorDataReceivedEventHandler DataReceived;

        public UsbConnector()
        {
            _baudRate = baudRate;
            _parity = parity;
            _dataBits = dataBits;
            _stopBits = stopBits;
        }

        public UsbConnector(
            int baudRate,
            Parity parity,
            int dataBits,
            StopBits stopBits)
        {
            _baudRate = baudRate;
            _parity = parity;
            _dataBits = dataBits;
            _stopBits = stopBits;
        }

        public void Open()
        {
            try
            {
                if (IsOpen())
                {
                    Close();
                }

                var comPort = GetUsbCOMPort();

                _serialPort = new SerialPort(comPort, _baudRate, _parity, _dataBits, _stopBits);
                _serialPort.Handshake = Handshake.None;

                if (!_serialPort.IsOpen)
                {
                    _serialPort.Open();
                    _serialPort.DataReceived += SerialPortDataReceived; ;
                    Console.WriteLine($"Connection Open in Port {comPort}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in '{nameof(UsbConnector)}.{nameof(Open)}':{ex.Message}");
            }
        }

        public void SendMessage(byte[] message)
        {
            if (_serialPort == null && message.Length == 0) return;

            try
            {

                if (!IsOpen())
                {
                    Open();
                }

                Console.WriteLine($"Message Sent: {message.ToHexString()}");

                _serialPort.Write(message, 0, message.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in '{nameof(UsbConnector)}.{nameof(SendMessage)}':{ex.Message}");
            }
        }

        public void SendMessageWithCRC(byte[] message)
        {
            if (_serialPort == null && message.Length == 0) return;

            try
            {
                var messageWithCRC = message.CloneWithCRC();
                SendMessage(messageWithCRC);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in '{nameof(UsbConnector)}.{nameof(SendMessageWithCRC)}':{ex.Message}");
            }
        }

        private bool IsOpen()
        {
            return _serialPort != null && _serialPort.IsOpen;
        }

        private void Close()
        {
            try
            {
                _serialPort.Close();
                _serialPort.Dispose();
                Console.WriteLine("Connection Close");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in '{nameof(UsbConnector)}.{nameof(Close)}':{ex.Message}");
            }
        }

        private void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var serialPort = sender as SerialPort;
            if (serialPort == null || DataReceived == null) return;

            try
            {
                var dataSize = serialPort.BytesToRead;
                var data = new byte[dataSize];
                serialPort.Read(data, 0, dataSize);

                Console.WriteLine($"Data Received: {data.ToHexString()}");

                DataReceived(this, data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in '{nameof(UsbConnector)}.{nameof(SerialPortDataReceived)}':{ex.Message}");
            }
        }

        private string GetUsbCOMPort()
        {
            try
            {
                var usbPorts = new List<string>();
                var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity");
                foreach (var mo in searcher.Get())
                {
                    var name = mo["Name"]?.ToString();
                    if (name != null && name.Contains("(COM"))
                    {
                        var start = name.IndexOf("(COM") + 1;
                        var end = name.IndexOf(")", start + 3);
                        var cname = name.Substring(start, end - start);
                        usbPorts.Add(cname);
                    }
                }

                var portNames = SerialPort.GetPortNames().ToList();

                return usbPorts.First(x => portNames.Contains(x));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in '{nameof(UsbConnector)}.{nameof(GetUsbCOMPort)}':{ex.Message}");
                return string.Empty;
            }
        }
    }
}
