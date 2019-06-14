using EdpConsole.Connectors;
using EdpConsole.Core;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EdpConsole.Extensions
{
    public static class ConnectorExtensions
    {
        public static async Task<ModbusResponse<TResponse>> GetRegistersAddressAsync<TResponse>(this IConnector conn, RegistersAddressMessage registersAddress)
        {
            var message = ModbusMessage.BuildGetRegistersAddressMessage(registersAddress);
            return await conn.SendMessageAsync<TResponse>(message);
        }

        public static async Task<Measurements> GetLastEntriesAsync(this IConnector conn, int resultLength = 6)
        {
            var message = ModbusMessage.BuildGetLastEntriesMessage(resultLength);
            return (await conn.SendMessageAsync<Measurements>(message)).Value;
        }

        public static async Task<Measurements> GetEntriesAsync(this IConnector conn, int resultLength)
        {
            var parcelLength = 6;
            var start = (int)conn.EntriesInUse + 1;
            var measurements = new Measurements();

            for (int i = 0; i < resultLength / parcelLength; i++)
            {
                start = start - parcelLength;
                var message = ModbusMessage.BuildGetEntriesMessage(parcelLength, start);
                measurements.AddRange((await conn.SendMessageAsync<Measurements>(message)).Value);
            }

            return measurements;

            //TODO: tamanho de acordo com o resultLength
            //var start = 1;// (int)conn.EntriesInUse - 7;
            //var message = ModbusMessage.BuildGetEntriesMessage(resultLength, start);
            //return await conn.SendMessageAsync<Measurements>(message);
        }

        public static async Task<UInt32> GetLastEnergyConsumeAsync(this IConnector conn)
        {
            var energyTypes = conn.MeasurementConfiguration.AllEnergies();
            var measurements = await conn.GetEntriesAsync(96); //96 são os dados de 1 dia inteiro

            foreach (var m in measurements.OrderByDescending(e => e.Clock))
            {
                Console.WriteLine(m);
            }

            return measurements.SumByType(energyTypes);
        }


        //public static async Task<ModbusResponse<TResponse>> GetLastEntriesAsync<TResponse>(this IConnector conn, MeasurementMessage measurement, int resultLength)
        //{
        //    var message = ModbusMessage.BuildGetLastEntriesMessage(measurement, resultLength);
        //    return await conn.SendMessageAsync<TResponse>(message);
        //}

        //public static async Task<ModbusResponse<TResponse>> GetEntriesAsync<TResponse>(this IConnector conn, MeasurementMessage measurement, int resultLength, int start = 1)
        //{
        //    var message = ModbusMessage.BuildGetEntriesMessage(measurement, resultLength, start);
        //    return await conn.SendMessageAsync<TResponse>(message);
        //}

        //public static async Task<ModbusResponse<object>> GetRegistersAddressAsync(this IConnector conn, RegistersAddressMessage registersAddress)
        //{
        //    var message = ModbusMessage.BuildGetRegistersAddressMessage(registersAddress);
        //    return await conn.SendMessageAsync<object>(message);
        //}

        //public static async Task<ModbusResponse<object>> GetLastEntriesAsync(this IConnector conn, MeasurementMessage measurement, int resultLength)
        //{
        //    var message = ModbusMessage.BuildGetLastEntriesMessage(measurement, resultLength);
        //    return await conn.SendMessageAsync<object>(message);
        //}

        //public static async Task<ModbusResponse<object>> GetEntriesAsync(this IConnector conn, MeasurementMessage measurement, int resultLength, int start = 1)
        //{
        //    var message = ModbusMessage.BuildGetEntriesMessage(measurement, resultLength, start);
        //    return await conn.SendMessageAsync<object>(message);
        //}

        //public static async Task<UInt32> GetLastEnergyConsumeAsync(this IConnector conn, MeasurementMessage measurement, int resultLength, int start = 1)
        //{
        //    var energies = conn.MeasurementConfiguration.AllEnergies();

        //    foreach (var energy in energies)
        //    {

        //    }

        //    var message = ModbusMessage.BuildGetLastEntriesMessage(measurement, resultLength, start);
        //    return await SendMessageAsync<object>(message);
        //}
    }
}
