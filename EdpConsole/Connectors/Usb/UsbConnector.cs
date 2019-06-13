using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
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
        private SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        public event ConnectorDataReceivedEventHandler DataReceived;

        public UsbConnector()
        {
            _baudRate = baudRate;
            _parity = parity;
            _dataBits = dataBits;
            _stopBits = stopBits;
            _handshake = handshake;

            Open();

            //TODO: fazer configuracao inicial
            //0x0080 Load profile -Configured measurements
            //0x0081 Load profile -Capture period
            //0x0082 Load profile -Entries
            //0x0083 Load profile -Profile entries
        }

        private void Open()
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

        public async Task<ModbusResponse> SendMessageAsync(ModbusMessage message)
        {
            if (_serialPort == null && message.Length == 0) return null;

            await semaphoreSlim.WaitAsync();

            try
            {
                SendMessage(message);

                return await GetDataReceivedAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in '{nameof(UsbConnector)}.{nameof(SendMessageAsync)}':{ex.Message}");
                return null;
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        private async Task<ModbusResponse> GetDataReceivedAsync()
        {
            try
            {
                while (HasDataToReceive())
                {
                    var dataSize = _serialPort.BytesToRead;
                    if (dataSize > 0)
                    {
                        var data = new byte[dataSize];
                        await _serialPort.BaseStream.ReadAsync(data, 0, dataSize);
                        _dataReceived.AddRange(data);
                    }
                }

                return new ModbusResponse(_dataReceived);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in '{nameof(UsbConnector)}.{nameof(SendMessageAsync)}':{ex.Message}");
                return null;
            }
            finally
            {
                _dataReceived = new List<byte>();
            }
        }


        private bool IsOpen()
        {
            return _serialPort != null && _serialPort.IsOpen;
        }

        public void Close()
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
            if (serialPort == null || DataReceived == null || DataReceived.GetInvocationList().Length == 0) return;

            try
            {
                lock (lockObject)
                {
                    var dataSize = serialPort.BytesToRead;
                    var data = new byte[dataSize];
                    serialPort.Read(data, 0, dataSize);

                    _dataReceived.AddRange(data);

                    Console.WriteLine($"Raw Data Received: {data.ToHexString()}");

                    if (!HasDataToReceive())
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

        private bool HasDataToReceive()
        {
            //TODO: em mensagens do tipo 0x44 e 0x45 considerar mais 12 bytes, pois não são contabilizados no bytecount
            //atenção pois pode ser requisitado mais de 1 registro, e neste caso talvez tenha que contabilizar a quantidade de registros vezes 12 bytes
            return !(_dataReceived.Count > _dataSizeIndex && (int)_dataReceived[_dataSizeIndex] == _dataReceived.Count - _crcSize - _headerSize);
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
                    var status = mo["Status"]?.ToString();

                    if (name != null && name.StartsWith("USB Serial Port (COM") && status != null && status == "OK")
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

        public void Dispose()
        {
            Close();
        }
    }
}