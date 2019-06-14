using EdpConsole.Connectors;
using EdpConsole.Core;
using System;
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

        public static async Task<ModbusResponse<Measurements>> GetLastEntriesAsync(this IConnector conn, int resultLength)
        {
            var message = ModbusMessage.BuildGetLastEntriesMessage(resultLength);
            return await conn.SendMessageAsync<Measurements>(message);
        }

        public static async Task<UInt32> GetLastEnergyConsumeAsync(this IConnector conn, int resultLength)
        {
            var energyTypes = conn.MeasurementConfiguration.AllEnergies();
            var message = ModbusMessage.BuildGetLastEntriesMessage(resultLength);
            var measurements = await conn.SendMessageAsync<Measurements>(message);

            return measurements.Value.SumByType(energyTypes);
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
