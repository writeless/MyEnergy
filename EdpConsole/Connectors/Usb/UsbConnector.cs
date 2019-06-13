using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;
using EdpConsole.Core;
using EdpConsole.Extensions;

namespace EdpConsole.Connectors.Usb
{
    public class UsbConnector : IConnector
    {
        private const int baudRate = 9600;
        private const Parity parity = Parity.None;
        private const int dataBits = 8;
        private const StopBits stopBits = StopBits.Two;
        private readonly Handshake handshake = Handshake.None;

        private readonly int _baudRate;
        private readonly Parity _parity;
        private readonly int _dataBits;
        private readonly StopBits _stopBits;
        private readonly Handshake _handshake;

        private SerialPort _serialPort;

        private const int _dataSizeIndex = 2;
        private const int _headerSize = 3;
        private const int _crcSize = 2;
        private List<byte> _dataReceived = new List<byte>();
        private MessageType _messageType = MessageType.None;
        private object lockObject = new object();

        public event ConnectorDataReceivedEventHandler DataReceived;

        public UsbConnector()
        {
            _baudRate = baudRate;
            _parity = parity;
            _dataBits = dataBits;
            _stopBits = stopBits;
            _handshake = handshake;
        }

        public UsbConnector(
            int baudRate,
            Parity parity,
            int dataBits,
            StopBits stopBits,
            Handshake handshake)
        {
            _baudRate = baudRate;
            _parity = parity;
            _dataBits = dataBits;
            _stopBits = stopBits;
            _handshake = handshake;
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
                _serialPort.Handshake = _handshake;

                if (!_serialPort.IsOpen)
                {
                    _serialPort.Open();

                    //TODO: fazer configuracao inicial
                    //0x0080 Load profile -Configured measurements
                    //0x0081 Load profile -Capture period
                    //0x0082 Load profile -Entries
                    //0x0083 Load profile -Profile entries

                    _serialPort.DataReceived += SerialPortDataReceived;
                    Console.WriteLine($"Connection Open in Port {comPort}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in '{nameof(UsbConnector)}.{nameof(Open)}':{ex.Message}");
            }
        }

        public void SendMessage(ModbusMessage message)
        {
            if (_serialPort == null && message.Length == 0) return;

            try
            {
                if (!IsOpen())
                {
                    Open();
                }

                Console.WriteLine($"Message Sent: {message.ToHexString()}");

                _messageType = message.MessageType;
                _serialPort.Write(message.Value, 0, message.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in '{nameof(UsbConnector)}.{nameof(SendMessage)}':{ex.Message}");
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
                lock (lockObject)
                {
                    var dataSize = serialPort.BytesToRead;
                    var data = new byte[dataSize];
                    serialPort.Read(data, 0, dataSize);
                    
                    _dataReceived.AddRange(data);

                    Console.WriteLine($"Raw Data Received: {data.ToHexString()}");

                    //TODO: em mensagens do tipo 0x44 e 0x45 considerar mais 12 bytes, pois não são contabilizados no bytecount
                    //atenção pois pode ser requisitado mais de 1 registro, e neste caso talvez tenha que contabilizar a quantidade de registros vezes 12 bytes
                    if (_dataReceived.Count > _dataSizeIndex && (int)_dataReceived[_dataSizeIndex] == _dataReceived.Count - _crcSize - _headerSize)
                    {
                        var response = new ModbusResponse(_dataReceived);
                        DataReceived(this, response);
                        _dataReceived = new List<byte>();
                    }
                }
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
                    //if (name != null && name.Contains("(COM"))
                    if (name != null && name.StartsWith("USB Serial Port (COM"))
                    {
                        var start = name.IndexOf("(COM") + 1;
                        var end = name.IndexOf(")", start + 3);
                        var cname = name.Substring(start, end - start);
                        usbPorts.Add(cname);
                    }
                }

                var portNames = SerialPort.GetPortNames().ToList();

                var result = usbPorts.FirstOrDefault(x => portNames.Contains(x));

                if (string.IsNullOrWhiteSpace(result))
                {
                    Console.WriteLine($"Warning in '{nameof(UsbConnector)}.{nameof(GetUsbCOMPort)}': USB serial port not found");
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in '{nameof(UsbConnector)}.{nameof(GetUsbCOMPort)}':{ex.Message}");
                return string.Empty;
            }
        }
    }
}