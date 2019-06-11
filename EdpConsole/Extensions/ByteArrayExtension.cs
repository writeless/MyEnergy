using System;
using System.Collections.Generic;
using System.Text;

namespace EdpConsole.Extensions
{
    public static class ByteArrayExtension
    {
        public static byte[] CloneWithCRC(this byte[] value)
        {
            var result = new byte[value.Length + 2];
            UInt16 crc16 = 0xFFFF;

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

        private static UInt16 CalculateCRC(byte dchar, UInt16 crc16)
        {
            UInt16 mask = (UInt16)(dchar & 0x00FF);
            crc16 = (UInt16)(crc16 ^ mask);
            for (int i = 0; i < 8; i++)
            {
                if ((UInt16)(crc16 & 0x0001) == 1)
                {
                    mask = (UInt16)(crc16 / 2);
                    crc16 = (UInt16)(mask ^ 0xA001);
                }
                else
                {
                    mask = (UInt16)(crc16 / 2);
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
    }
}
