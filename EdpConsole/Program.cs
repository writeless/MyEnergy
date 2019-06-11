using EdpConsole.Connectors;
using EdpConsole.Connectors.Usb;
using EdpConsole.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Threading;
using System.Threading.Tasks;

namespace EdpConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            //(meaning: V00110)
            //and a typical answer should be: 0x01, 0x04, 0x06, 0x56, 0x30, 0x30, 0x31, 0x31, 0x0, 0x67, 0xfe
            //                0x01, 0x04, 0x00, 0x04, 0x00, 0x01, 0x3b, 0x49
            //var request = new int[] { 0x01, 0x04, 0x00, 0x04, 0x00, 0x01, 0x70, 0x0b };

            Console.WriteLine("Start");

            //based in https://gist.github.com/liuyork/6978474
            //CRCTest();

            //based in http://velocio.net/modbus-example/
            SendMessageTest();

            Console.Read();
        }

        private static void SendMessageTest()
        {            
            ThreadPool.QueueUserWorkItem(x =>
            {
                var message = new byte[] { 0x01, 0x04, 0x00, 0x04, 0x00, 0x01, 0x70, 0x0b };

                IConnector conn = new UsbConnector();
                conn.DataReceived += Conn_DataReceived;
                conn.Open();
                conn.SendMessage(message);

                int count = 0;
                while (true)
                {
                    //Task.Delay(1000);
                    Thread.Sleep(1000);
                    Console.Write(".");

                    count++;
                    if (count == 10)
                    {
                        Console.WriteLine(".");
                        conn.SendMessageWithCRC(message);
                    }

                    if (count == 20)
                    {
                        Console.WriteLine(".");
                        conn.SendMessage(message);
                        count = 0;
                    }
                }
            });
        }

        private static void Conn_DataReceived(IConnector sender, byte[] dataReceived)
        {
            Console.WriteLine($"Conn_DataReceived: {dataReceived.ToHexString()}");
        }

        private static void CRCTest()
        {
            CRC16Modbus crc = new CRC16Modbus();
            //int[] data = new int[] { -86, 90, -79, -125, 1, 0, 30, -92, 0, 76, 113, 1, 2, 6, 82, 116, 92, -126, 76, -126, 90, -116, 80, 61, 68, 0, 0, 0, 0, 0, 0, 0, 0 };
            int[] data = new int[] { 0x01, 0x04, 0x00, 0x04, 0x00, 0x01, 0x70, 0x0b };
            foreach (var d in data)
            {
                crc.update(d);
            }

            Console.WriteLine($"0x{crc.getValue():X}");

            byte[] byteStr = new byte[2];
            byteStr[0] = (byte)((crc.getValue() & 0x000000ff));
            byteStr[1] = (byte)((crc.getValue() & 0x0000ff00) >> 8);

            Console.WriteLine($"0x{byteStr[0]:X} and 0x{byteStr[1]:X}");
            Console.WriteLine($"{byteStr[0]:d} and {byteStr[1]:d}");
        }
    }
}
