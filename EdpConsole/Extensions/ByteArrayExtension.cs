using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EdpConsole.Extensions
{
    public static class ByteArrayExtension
    {
        public static byte[] WithCRC(this byte[] value)
        {
            var result = new byte[value.Length + 2];
            var crc16 = 0xFFFF;

            for (int i = 0; i < value.Length; i++)
            {
                var byteFromArray = (byte)value[i];
                result[i] = byteFromArray;
                crc16 = CalculateCRC(byteFromArray, crc16);
            }

            result[result.Length - 2] = (byte)(crc16 % 0x100);
            result[result.Length - 1] = (byte)(crc16 / 0x100);

            return result;
        }

        private static int CalculateCRC(byte dchar, int crc16)
        {
            var mask = dchar & 0x00FF;
            crc16 = crc16 ^ mask;
            for (int i = 0; i < 8; i++)
            {
                if ((crc16 & 0x0001) == 1)
                {
                    mask = crc16 / 2;
                    crc16 = mask ^ 0xA001;
                }
                else
                {
                    mask = crc16 / 2;
                    crc16 = mask;
                }
            }
            return crc16;
        }

        public static string ToHexString(this byte[] value)
        {
            var hex = new StringBuilder(value.Length * 2);
            foreach (byte byteFromArray in value)
            {
                hex.AppendFormat("0x{0:x2} ", byteFromArray);
            }

            return hex.ToString();
        }

        public static DateTime ToDateTime(this byte[] bytes)
        {
            var year = bytes.ToInt(0, 2);
            var month = (int)bytes[2];
            var day = (int)bytes[3];
            var hour = (int)bytes[5];
            var minute = (int)bytes[6];
            var second = (int)bytes[7];

            return new DateTime(year, month, day, hour, minute, second);
        }

        public static int ToInt(this byte[] bytes, int start, int end)
        {
            if (end - start == 1)
            {
                return (int)bytes.Skip(start).Take(end).First();
            }

            var data = bytes.Skip(start).Take(end).ToArray();

            if (BitConverter.IsLittleEndian)
                Array.Reverse(data);

            if (data.Length == 2)
            {
                return BitConverter.ToInt16(data, 0);
            }
            else
            {
                return BitConverter.ToInt32(data, 0);
            }
        }
    }
}
