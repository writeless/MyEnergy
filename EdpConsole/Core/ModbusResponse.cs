using EdpConsole.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EdpConsole.Core
{
    public class ModbusResponse
    {
        public byte Address { get; }

        public FunctionCode FunctionCode { get; }

        public int ByteCount { get; }

        public byte[] Data { get; }

        public ModbusResponse(List<byte> response)
        {
            Address = response[0];
            FunctionCode = (FunctionCode)response[1];
            ByteCount = (int)response[2];

            Data = response
                    .Take(response.Count - 2)
                    .Skip(3)
                    .ToArray();
        }

        public override string ToString()
        {
            return Data.ToHexString();
        }
    }
}
