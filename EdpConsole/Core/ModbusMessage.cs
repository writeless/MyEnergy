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
        public static implicit operator ModbusMessage(RegistersAddressMessage type)
        {
            var address = (byte)0x01;
            var functionCode = FunctionCode.ReadRegistersAddress;
            var startingAddress = new byte[] { 0x00, (byte)type };
            var quantityInputRegisters = new byte[] { 0x00, 0x01 };

            return new ModbusMessage(
                functionCode,
                type,
                address,
                (byte)functionCode,
                startingAddress[0],
                startingAddress[1],
                quantityInputRegisters[0],
                quantityInputRegisters[1]);
        }

        public byte[] Value { get; }

        public FunctionCode FunctionCode { get; }

        public RegistersAddressMessage RegistersAddressMessage { get; }

        //public MeasurementType Measurement { get; }

        public int Length { get { return Value.Length; } }

        public ModbusMessage(FunctionCode functionCode, RegistersAddressMessage registersAddressMessage, params byte[] message)
        {
            Value = message.WithCRC();
            FunctionCode = functionCode;
            RegistersAddressMessage = registersAddressMessage;
        }

        public string ToHexString()
        {
            return Value.ToHexString();
        }
    }
}
