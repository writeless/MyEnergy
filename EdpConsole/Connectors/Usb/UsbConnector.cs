using EdpConsole.Core;
using EdpConsole.Extensions;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Threading;
using System.Threading.Tasks;

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
        private SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        public uint EntriesInUse { get; private set; }
        public MeasurementConfiguration MeasurementConfiguration { get; private set; }

        public UsbConnector()
        {
            _baudRate = baudRate;
            _parity = parity;
            _dataBits = dataBits;
            _stopBits = stopBits;
            _handshake = handshake;
        }

        public async Task<bool> Open()
        {
            try
            {
                if (IsOpen())
                {
                    Close();
                }

                //TODO: poderia ser feito um teste na conexao para ter certeza que a porta é uma comunicacao com o medidor
                var comPort = await GetCOMPort();

                if (comPort == null)
                {
                    return false;
                }

                _serialPort = BuildSerialPort(comPort);
                _serialPort.Open();
                await LoadConfiguration();

                Console.WriteLine($"Connection Open in Port {comPort}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in '{nameof(UsbConnector)}.{nameof(Open)}':{ex.Message}");
                return false;
            }
        }

        private SerialPort BuildSerialPort(string comPort)
        {
            var serialPort = new SerialPort(comPort, _baudRate, _parity, _dataBits, _stopBits);
            serialPort.Handshake = _handshake;
            return serialPort;
        }

        private async Task CheckDevice(string comPort)
        {
            if (IsOpen())
            {
                Close();
            }

            _serialPort = BuildSerialPort(comPort);
            _serialPort.Open();

            //Check if device is a smart meter
            MeasurementConfiguration = (await ConnectorExtensions.GetRegistersAddressAsync<MeasurementConfiguration>(this, RegistersAddressMessage.ConfiguredMeasurements)).Value;
        }

        private async Task LoadConfiguration()
        {
            MeasurementConfiguration = (await ConnectorExtensions.GetRegistersAddressAsync<MeasurementConfiguration>(this, RegistersAddressMessage.ConfiguredMeasurements)).Value;
            EntriesInUse = (await ConnectorExtensions.GetRegistersAddressAsync<uint>(this, RegistersAddressMessage.EntriesInUse)).Value;
        }

        public async Task<ModbusResponse<object>> SendMessageAsync(ModbusMessage message)
        {
            return await SendMessageAsync<object>(message);
        }

        public async Task<ModbusResponse<TResponse>> SendMessageAsync<TResponse>(ModbusMessage message)
        {
            if (_serialPort == null && message.Length == 0) return null;

            await semaphoreSlim.WaitAsync();

            try
            {
                //TODO: configurar retry

                SendMessage(message);

                return await GetDataReceivedAsync<TResponse>(message);
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

        private void SendMessage(ModbusMessage message)
        {
            if (_serialPort == null && message.Length == 0) return;

            try
            {
                if (!IsOpen())
                {
                    Open();
                }

                //Console.WriteLine($"Message Sent: {message.ToHexString()}");

                _serialPort.Write(message.Value, 0, message.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in '{nameof(UsbConnector)}.{nameof(SendMessage)}':{ex.Message}");
            }
        }

        private async Task<ModbusResponse<TResponse>> GetDataReceivedAsync<TResponse>(ModbusMessage message)
        {
            try
            {
                var dataReceived = new List<byte>();

                //TODO: criar um timeout
                var timeout = 2 * 1000;
                var startTime = DateTime.Now;
                while (HasDataToReceive(dataReceived))
                {
                    var dataSize = _serialPort.BytesToRead;
                    if (dataSize > 0)
                    {
                        var data = new byte[dataSize];
                        await _serialPort.BaseStream.ReadAsync(data, 0, dataSize);
                        dataReceived.AddRange(data);
                    }

                    if ((DateTime.Now - startTime).TotalMilliseconds > timeout)
                    {
                        throw new Exception("timeout");
                    }
                }

                return new ModbusResponse<TResponse>(MeasurementConfiguration, message, dataReceived);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in '{nameof(UsbConnector)}.{nameof(SendMessageAsync)}':{ex.Message}");
                return null;
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in '{nameof(UsbConnector)}.{nameof(Close)}':{ex.Message}");
            }
        }

        private bool HasDataToReceive(List<byte> dataReceived)
        {
            int dataLengthIndex = 2;
            int headerSize = 3;
            int crcSize = 2;

            //TODO: em mensagens do tipo 0x44 e 0x45 considerar mais 12 bytes, pois não são contabilizados no bytecount
            //atenção pois pode ser requisitado mais de 1 registro, e neste caso talvez tenha que contabilizar a quantidade de registros vezes 12 bytes
            var hasDataLength = dataReceived.Count > dataLengthIndex;
            var dataLength = dataReceived.Count - crcSize - headerSize;
            return !(hasDataLength && dataReceived[dataLengthIndex] == dataLength);
        }

        private async Task<string> GetCOMPort()
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

                var comPorts = SerialPort.GetPortNames().ToList();
                var portsToTry = usbPorts.Where(x => comPorts.Contains(x)).ToList();
                portsToTry.AddRange(comPorts.Except(portsToTry));

                using (var conn = new UsbConnector())
                {
                    foreach (var portToTry in portsToTry)
                    {
                        try
                        {
                            Console.WriteLine($"Try port {portToTry}");
                            await conn.CheckDevice(portToTry);
                            return portToTry;
                        }
                        catch
                        {
                        }
                    }
                }

                Console.WriteLine($"Error in '{nameof(UsbConnector)}.{nameof(GetCOMPort)}': serial port not found");

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in '{nameof(UsbConnector)}.{nameof(GetCOMPort)}':{ex.Message}");
                return string.Empty;
            }
        }

        public void Dispose()
        {
            Close();
        }
    }
}