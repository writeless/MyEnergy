using EdpConsole.Connectors;
using EdpConsole.Connectors.Usb;
using EdpConsole.Core;
using EdpConsole.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EdpConsole
{
    public class Command
    {
        public Command(string meterSerialNumber, List<MeasurementData> measurements)
        {
            MeterSerialNumber = meterSerialNumber;
            Measurements = measurements;
        }

        public string MeterSerialNumber { get; set; }

        public List<MeasurementData> Measurements { get; set; }

        public bool PerMinute { get; set; }

        public class MeasurementData
        {
            public MeasurementData(DateTime clock, Dictionary<string, int> values)
            {
                Clock = clock;
                Values = values;
            }

            public DateTime Clock { get; set; }

            public Dictionary<string, int> Values { get; set; }
        }
    }

    internal class Program
    {
        public static void Main(string[] args)
        {
            var startTime = DateTime.Now;
            Console.WriteLine("Start");

            //based in http://velocio.net/modbus-example/
            SendMessageTest().Wait();

            Console.WriteLine("Tempo: " + (DateTime.Now - startTime).TotalMilliseconds);

            Console.ReadLine();
        }

        private async static Task SendDataToServer(int consume, int power)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var clock = DateTime.UtcNow;
                    var values = new Dictionary<string, int>() { { "InstantaneousActivePowerSumAllPhases", consume }, { "InstantaneousPowerFactor", power } };
                    var command = new Command("num001", new List<Command.MeasurementData>() { new Command.MeasurementData(clock, values) });
                    var body = JsonConvert.SerializeObject(command);
                    var content = new StringContent(body, Encoding.UTF8, "application/json");

                    client.BaseAddress = new Uri("https://localhost:5001/");
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var response = await client.PostAsync("api/measurements/perminute", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var products = response.Content.ReadAsStringAsync().Result;
                    }
                    else
                    {
                        Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SendDataToServer ex: {ex.Message}");
            }
        }

        private async static Task SendMessageTest()
        {
            //deixar como static no server, dai economiza com abertura e fechamento de conexao
            using (IConnector conn = new UsbConnector())
            {
                conn.Open();
                await conn.LoadConfiguration();

                while (true)
                {
                    var consume = await conn.GetRegistersAddressAsync<uint>(RegistersAddressMessage.InstantaneousActivePowerSumAllPhases);
                    Console.WriteLine($"consume: {consume.Value}");

                    var power = await conn.GetRegistersAddressAsync<uint>(RegistersAddressMessage.InstantaneousPowerFactor);
                    Console.WriteLine($"power: {power.Value}");

                    await SendDataToServer((int)consume.Value, (int)power.Value);

                    Thread.Sleep(2000);
                }

                //var data = await conn.GetLastEntriesAsync(6);
                //Console.WriteLine($"date: {data}");

                //var data2 = await conn.GetLastEnergyConsumeAsync();
                //Console.WriteLine($"TOTAL: {data2}");

                //Console.WriteLine($"xxx: {await conn.GetRegistersAddressAsync(RegistersAddressMessage.ConfiguredMeasurements)}");
                //Console.WriteLine($"xxx: {await conn.GetRegistersAddressAsync(RegistersAddressMessage.CapturePeriod)}");
                //Console.WriteLine($"xxx: {await conn.GetRegistersAddressAsync(RegistersAddressMessage.EntriesInUse)}");
                //Console.WriteLine($"xxx: {await conn.GetRegistersAddressAsync(RegistersAddressMessage.ProfileEntries)}");

                //21/05/2019 16:30:00
                //var x = ModbusMessage.RegistersAddress.Clock;
                //var response0 = conn.SendMessageAsync(ModbusMessage.LoadProfile(MeasurementType.ActiveEnergyPositiveA, 1, 1)).Result;
                //var initialDate = ToDate(response0.Data);

                //var response1 = conn.SendMessageAsync(ModbusMessage.LastLoadProfile(MeasurementType.ActiveEnergyPositiveA, 1)).Result;
                //var endDate = ToDate(response1.Data);

                //var minutesIntervalBetweenEntries = 15;
                //var totalLastEntries = 6;
                //var totalEntries = (endDate - initialDate).TotalMinutes / minutesIntervalBetweenEntries - totalLastEntries;

                //var start = 1;// 583;//2230; //4539 registros sao feitos de 15 em 15 minutos, ele nao acessa os ultimos 6 registros
                //for (int i = start; i <= start + 1*6; i += 6)
                //{
                //    var response = conn.SendMessageAsync(ModbusMessage.LoadProfile(MeasurementType.ActiveEnergyPositiveA, i)).Result;
                //    for (int j = 0; j < 6; j++)
                //        Console.WriteLine($"===> Received: {ToDate(response.Data.Skip(j * 37).Take(12).ToArray()).ToString("dd/MM/yyyy HH:mm:ss")} | 0x{response.Data[15 + (j * 37)]:x2} | 0x{response.Data[16 + (j * 37)]:x2}");
                //}

                //var response2 = conn.SendMessageAsync(ModbusMessage.LastLoadProfile(MeasurementType.ActiveEnergyPositiveA)).Result;
                //for (int j = 0; j < 6; j++)
                //    Console.WriteLine($"===> Received: {ToDate(response2.Data.Skip(j * 37).Take(12).ToArray()).ToString("dd/MM/yyyy HH:mm:ss")} | 0x{response2.Data[15 + (j * 37)]:x2} | 0x{response2.Data[16 + (j * 37)]:x2}");
            }
        }
    }
}