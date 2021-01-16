using Mpx;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Threading;

namespace MpxMc3030
{
    class Transmitter : IDisposable
    {
        public void Connect(string port)
        {
            Console.WriteLine($"Opening COM port '{port}'...");
            serialPort.PortName = port;
            serialPort.BaudRate = 9600;
            serialPort.DataBits = 8;
            serialPort.StopBits = StopBits.One;
            serialPort.Parity = Parity.None;
            serialPort.ReadTimeout = 100;
            serialPort.WriteTimeout = 1000;
            serialPort.Open();
        }

        public void PrintId()
        {
            try
            {
                var block73 = ReadBlock(0x73);
                var block7F = ReadBlock(0x7F);
                Console.WriteLine();
                Console.WriteLine($"Memory Size: {block73[1]}");
                Console.WriteLine($"Owner: '{ExtractString(block73, 0x30, 16)}'");
                Console.WriteLine(ExtractString(block7F, 0xBD, 15));
                Console.WriteLine(ExtractString(block7F, 0xCD, 15));
                Console.WriteLine(ExtractString(block7F, 0xDD, 15));
                Console.WriteLine(ExtractString(block7F, 0xED, 15));

            }
            catch (TimeoutException e)
            {
                using (var coloredConsole = new ColoredConsole(ConsoleColor.Red))
                {
                    Console.Error.WriteLine($"ERROR: {e.Message}");
                }

                Console.WriteLine("The transmitter does not repond to a connection request. Check the connection, or try to power-cycle the transmitter.");
            }
        }

        public void DumpBlock(uint block)
        {
            var buffer = ReadBlock(block);
            Tools.DumpBuffer(buffer, 0, buffer.Length - 1);
        }

        public void DiffBlock(uint block)
        {
            var old = ReadBlock(block);
            Console.WriteLine("Press CTRL-C to abort.");

            while (true)
            {
                var buffer = ReadBlock(block, false);
                if (old.Length < 256 || buffer.Length < 256)
                    break;

                for (int i = 0; i < 256; i++)
                {
                    if (old[i] != buffer[i])
                    {
                        Console.WriteLine($"[{block:X02}{i:X02}]: {old[i]:X02} -> {buffer[i]:X02}");
                    }
                }

                old = buffer;

                Thread.Sleep(1000);
            }
        }

        public void SaveMemoryToFile(string filename, uint memory)
        {
            Console.WriteLine($"Creating file {filename}...");
            using (var stream = new FileStream(filename, FileMode.Create))
            {
                var buffer = ReadBlock(memory);
                stream.Write(buffer, 0, buffer.Length - 1);
                stream.WriteByte((byte)memory);
            }
        }

        public void LoadMemoryFromFile(string filename, uint memory)
        {
            Console.WriteLine($"Opening file {filename}...");
            using (var stream = new FileStream(filename, FileMode.Open))
            {
                var buffer = new byte[pageSize];
                int size = stream.Read(buffer, 0, buffer.Length);
                if (size != buffer.Length)
                {
                    throw new ApplicationException("Model file must be 257 bytes in size.");
                }

                WriteBlock(memory, buffer);
            }
        }

        public void SaveBlocksToFile(string filename, uint firstBlock, uint lastBlock)
        {
            Console.WriteLine($"Creating file {filename}...");
            using (var stream = new FileStream(filename, FileMode.Create))
            {
                for (uint block = firstBlock; block <= lastBlock; block++)
                {
                    var buffer = ReadBlock(block);
                    stream.WriteByte((byte)block);
                    stream.Write(buffer, 0, buffer.Length - 1);
                }
            }
        }

        public void LoadBlocksFromFile(string filename)
        {
            Console.WriteLine($"Opening file {filename}...");
            using (var stream = new FileStream(filename, FileMode.Open))
            {
                while (true)
                {
                    int block = stream.ReadByte();
                    if (block < 0)
                    {
                        break;
                    }

                    var buffer = new byte[pageSize];
                    int size = stream.Read(buffer, 0, buffer.Length);
                    if (size != buffer.Length)
                    {
                        break;
                    }

                    WriteBlock((uint)block, buffer);
                }
            }
        }

        byte[] ReadBlock(uint block, bool verbose = true)
        {
            if (verbose)
            {
                Console.Write($"Reading block {block}: ");
            }

            WriteData(new byte[] { (byte)block, 0xCF, 0xCF });
            byte code = ReadByte();
            WriteByte(0x14);
            var buffer = ReadData(257);
            var checksum = buffer.Length > 0 ? buffer[buffer.Length - 1] : 0xFF;

            if (verbose)
            {
                Console.WriteLine($"Received {buffer.Length} bytes, Code: 0x{code:X02}, Checksum: 0x{checksum:X02} ({(CheckChecksum(buffer) ? "OK" : "FAILED")})");
            }

            return buffer;
        }

        void WriteBlock(uint block, byte[] buffer, bool verbose = true)
        {
            if (verbose)
            {
                Console.Write($"Writing block {block}: ");
            }

            var checksum = CalculateChecksum(buffer);
            WriteData(new byte[] { (byte)block, 0x8F, 0x8F });
            byte code = ReadByte();
            WriteData(buffer);
            WriteByte(checksum);
            var response = ReadData(2);

            if (verbose)
            {
                Console.WriteLine($"Sent {buffer.Length} bytes, Code: 0x{code:X02}, Checksum: 0x{checksum:X02}, Response: {Tools.FormatBuffer(response)}");
            }
        }

        byte ReadByte()
        {
            return (byte)serialPort.ReadByte();
        }

        byte[] ReadData(int size)
        {
            var bytes = new List<byte>(size);

            for (int i = 0; i < size; i++)
            {
                var ch = serialPort.ReadByte();
                if (ch < 0)
                    break;

                bytes.Add((byte)ch);
            }

            return bytes.ToArray();
        }

        void WriteByte(byte data)
        {
            WriteData(new byte[] { data });
        }

        void WriteData(byte[] buffer)
        {
            serialPort.Write(buffer, 0, buffer.Length);
        }

        void IDisposable.Dispose()
        {
            if (serialPort.IsOpen)
            {
                serialPort.Close();
            }
        }

        static bool CheckChecksum(byte[] buffer)
        {
            return CalculateChecksum(buffer) == 0;
        }

        static byte CalculateChecksum(byte[] buffer)
        {
            byte checksum = 0;

            for (int i = 0; i < buffer.Length; i++)
            {
                checksum ^= buffer[i];
            }

            return checksum;
        }

        static string ExtractString(byte[] buffer, int offset, int count)
        {
            string result = "";

            for (int i = 0; i < count; i++)
            {
                result += MapChar((char)buffer[offset + i]);
            }

            return result;
        }

        static char MapChar(char ch)
        {
            switch (ch)
            {
                case '@':
                    return ' ';
                default:
                    return ch;
            }
        }

        SerialPort serialPort = new SerialPort();

        const int pageSize = 256;
    }
}
