using EdpConsole.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace EdpConsole.Core
{
    //TODO: 3 tipos de mensagens RegistersAddress, LoadProfile(Measurements)0x45 e LastLoadProfile(Measurements)0x44
    //pode passar toda vez o "Measurement ID indexes to retrive" como 0x00, assim retorna todas as medições e fica mais facil trabalhar

    public class ModbusMessage
    {
        public static ModbusMessage BuildGetRegistersAddressMessage(RegistersAddressMessage registersAddress)
        {
            var address = (byte)0x01;
            var functionCode = FunctionCode.ReadRegistersAddress;
            var startingAddress = new byte[] { 0x00, (byte)registersAddress };
            var quantityInputRegisters = new byte[] { 0x00, 0x01 };

            return new ModbusMessage(
                functionCode,
                registersAddress,
                address,
                (byte)functionCode,
                startingAddress[0],
                startingAddress[1],
                quantityInputRegisters[0],
                quantityInputRegisters[1]);
        }

        public static ModbusMessage BuildGetLastEntriesMessage(int resultLength)
        {
            var address = (byte)0x01;
            var functionCode = FunctionCode.ReadLastEntries;
            var allMeasurement = (byte)0x00;

            return new ModbusMessage(
                functionCode,
                resultLength,
                address,
                (byte)functionCode,
                allMeasurement,
                (byte)resultLength);
        }

        public static ModbusMessage BuildGetEntriesMessage(int resultLength, int start = 1)
        {
            var startInBytes = BitConverter.GetBytes(start);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(startInBytes);

            var address = (byte)0x01;
            var functionCode = FunctionCode.ReadEntries;
            var allMeasurement = (byte)0x00;

            return new ModbusMessage(
                functionCode,
                resultLength,
                address,
                (byte)functionCode,
                allMeasurement,
                startInBytes[0], 
                startInBytes[1], 
                startInBytes[2], 
                startInBytes[3],
                (byte)resultLength);
        }

        public byte[] Value { get; }

        public FunctionCode FunctionCode { get; }

        public RegistersAddressMessage RegistersAddress { get; }

        public int ResultLength { get; }

        public int Length { get { return Value.Length; } }

        public ModbusMessage(FunctionCode functionCode, RegistersAddressMessage registersAddress, params byte[] message)
        {
            Value = message.WithCRC();
            FunctionCode = functionCode;
            RegistersAddress = registersAddress;
        }

        public ModbusMessage(FunctionCode functionCode, int resultLength, params byte[] message)
        {
            Value = message.WithCRC();
            FunctionCode = functionCode;
            ResultLength = resultLength;
        }

        public string ToHexString()
        {
            return Value.ToHexString();
        }
    }
}
