using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EdpConsole.Connectors;
using EdpConsole.Connectors.Usb;
using EdpConsole.Core;
using EdpConsole.Extensions;

namespace EdpConsole
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Start");

            //based in http://velocio.net/modbus-example/
            SendMessageTest().Wait();

            Console.ReadLine();
        }

        private async static Task SendMessageTest()
        {
            //deixar como static no server, dai economiza com abertura e fechamento de conexao
            using (IConnector conn = new UsbConnector())
            {
                conn.Open();
                await conn.LoadConfiguration();

                var date = await conn.GetRegistersAddressAsync<DateTime>(RegistersAddressMessage.Clock);
                Console.WriteLine($"date: {date.Value}");

                var data = await conn.GetLastEntriesAsync(6);
                Console.WriteLine($"date: {data.Value}");

                var data2 = await conn.GetLastEnergyConsumeAsync(6);
                Console.WriteLine($"date: {data2}");

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
