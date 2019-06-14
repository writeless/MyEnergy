using EdpConsole.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EdpConsole.Core
{
    public class ModbusResponse<TResponse>
    {
        public byte Address { get; }

        public FunctionCode FunctionCode { get; }

        public int ByteCount { get; }

        public byte[] Data { get; }

        public TResponse Value { get; }

        public ModbusResponse(ModbusMessage request, List<byte> response)
        {
            Address = response[0];
            FunctionCode = (FunctionCode)response[1];
            ByteCount = (int)response[2];

            Data = response
                    .Take(response.Count - 2)
                    .Skip(3)
                    .ToArray();

            Value = BuildValue(request, Data);
        }

        public TResponse BuildValue(ModbusMessage request, byte[] data)
        {
            switch (request.FunctionCode)
            {
                case FunctionCode.ReadRegistersAddress:
                    return BuildRegistersAddressValue(request.RegistersAddress, data);

                case FunctionCode.ReadLastEntries:
                case FunctionCode.ReadEntries:
                    return BuildMeasurementValue(request.Measurement, data);

                case FunctionCode.None:
                default:
                    return default(TResponse);
            }
        }

        private TResponse BuildRegistersAddressValue(RegistersAddressMessage registersAddress, byte[] data)
        {
            object result = null;

            switch (registersAddress)
            {
                case RegistersAddressMessage.Clock:
                    result = data.ToDateTime();
                    break;
                case RegistersAddressMessage.ConfiguredMeasurements:
                    result = new MeasurementConfiguration(data);
                    break;
                default:
                    result = data.ToUInt32();
                    break;
            }

            return (TResponse)result;
        }

        private TResponse BuildMeasurementValue(MeasurementMessage measurement, byte[] data)
        {
            object result = null;

            switch (measurement)
            {
                case MeasurementMessage.Clock:
                    result = data.ToDateTime();
                    break;
                default:
                    result = data.ToUInt32();
                    break;
            }

            return (TResponse)result;
        }

        public override string ToString()
        {
            return Data.ToHexString();
        }
    }
}
