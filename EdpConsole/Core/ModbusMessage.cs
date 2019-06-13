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
        public static ModbusMessage Clock = new ModbusMessage(MessageType.ActiveCoreFirmwareId, 0x01, 0x04, 0x00, 0x01, 0x00, 0x01);

        //tem 2 bytes, o segundo tem a quantidade de registros
        public static ModbusMessage StatusControl = new ModbusMessage(MessageType.ActiveCoreFirmwareId, 0x01, 0x04, 0x00, 0x09, 0x00, 0x01);

        public static ModbusMessage ActiveCoreFirmwareId = new ModbusMessage(MessageType.ActiveCoreFirmwareId, 0x01, 0x04, 0x00, 0x04, 0x00, 0x01);

        public static ModbusMessage LoadProfileConfiguredMeasurements = new ModbusMessage(MessageType.LoadProfileConfiguredMeasurements, 0x01, 0x04, 0x00, 0x80, 0x00, 0x01);

        public static ModbusMessage LoadProfileTotalEntries = new ModbusMessage(MessageType.LoadProfileConfiguredMeasurements, 0x01, 0x04, 0x00, 0x83, 0x00, 0x01);

        public static ModbusMessage LastLoadProfile(MeasurementType measurement)
        {
            return new ModbusMessage(Enum.Parse<MessageType>(measurement.ToString()), measurement, 0x01, 0x44, 0x00, 0x06);
            //return new ModbusMessage(Enum.Parse<MessageType>(measurement.ToString()), measurement, 0x01, 0x44, (byte)measurement, 0x01);
        }

        public static ModbusMessage LoadProfile(MeasurementType measurement)
        {
            return new ModbusMessage(Enum.Parse<MessageType>(measurement.ToString()), measurement, 0x01, 0x45, 0x00, 0x00, 0x00, 0x00, 0x01, 0x06);
            //return new ModbusMessage(Enum.Parse<MessageType>(measurement.ToString()), measurement, 0x01, 0x45, (byte)measurement, 0x01);
        }

        public byte[] Value { get; }

        public MessageType MessageType { get; }

        public MeasurementType Measurement { get; }

        public int Length { get { return Value.Length; } }

        private ModbusMessage(MessageType messageType, params byte[] message)
        {
            Value = message.CloneWithCRC();
            MessageType = messageType;
        }

        private ModbusMessage(MessageType messageType, MeasurementType measurement, params byte[] message)
            : this(messageType, message)
        {
            Measurement = measurement;
        }

        public string ToHexString()
        {
            return Value.ToHexString();
        }
    }
}
