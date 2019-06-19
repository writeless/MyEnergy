using EdpConsole.Core;
using System;
using System.Threading.Tasks;

namespace EdpConsole.Connectors
{
    public interface IConnector : IDisposable
    {
        uint EntriesInUse { get; }
        MeasurementConfiguration MeasurementConfiguration { get; }

        Task<bool> Open();

        Task<ModbusResponse<TResponse>> SendMessageAsync<TResponse>(ModbusMessage message);
    }
}