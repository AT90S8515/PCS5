using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PCS5_Firmware
{
    class Program
    {
        private const int blockSize = 512;
        private const int entryMaxCount = 10;
        static EndianIO IO = new EndianIO();
        public static header_t header = new header_t();

        //Vietnamese Text
        [DllImport("kernel32.dll")]
        static extern bool SetConsoleOutputCP(uint wCodePageID);

        public struct header_t
        {
            public string magic; // SLB2
            public ulong version; // 1
            public uint fileCount;
            public uint blockCount;
            public byte[] reserved;
            public file_entry_t[] fileEntry;
        };
        public struct file_entry_t
        {
            public uint offset; // 1 block is header size (512 padded)
            public uint contentSize;
            public ulong reserved; // probably file alignment
            public string fileName;
            public byte[] Data; // Added this to make things easier :fa:
        };
        static void Main(string[] args)
        {
            SetConsoleOutputCP(65001);
            Console.OutputEncoding = Encoding.Unicode;
            Console.InputEncoding = Encoding.Unicode;
            Console.WriteLine("FILE PATH: ");
            string path = Console.ReadLine();
            if (path.Contains("\""))
            {
                path = path.Replace("\"", string.Empty);
            }
            ReadFile(path);
            Console.ReadLine();
        }
        private static void ReadFile(string filename)
        {
            Console.WriteLine("FILENAME: " + new FileInfo(filename).Name);
            Console.WriteLine("SIZE: " + Math.Round(Convert.ToDecimal(new FileInfo(filename).Length / 1024 / 1024), 2) + "MB");
            IO = new EndianIO(filename, EndianType.LittleEndian, true);
            header.magic = IO.In.ReadString(4);
            header.version = IO.In.ReadUInt64();
            Console.WriteLine("MAGIC NUMBER: " + header.magic);
            Console.WriteLine("VERSION: " + header.version);
            Console.WriteLine("VERSION (HEX): " + "0x" + header.version.ToString("X"));
            header.fileCount = IO.In.ReadUInt32();
            header.blockCount = IO.In.ReadUInt32();
            header.reserved = IO.In.ReadBytes(12);
            header.fileEntry = new file_entry_t[header.fileCount];
            Console.WriteLine("FILE NUMBER: " + header.fileCount);
            Console.WriteLine("BLOCK NUMBER: " + header.blockCount);
            for (int i = 0; i < header.fileCount; i++)
            {
                header.fileEntry[i].offset = IO.In.ReadUInt32();
                header.fileEntry[i].contentSize = IO.In.ReadUInt32();
                header.fileEntry[i].reserved = IO.In.ReadUInt64();
                header.fileEntry[i].fileName = IO.In.ReadString(0x20);
            }

            for (int i = 0; i < header.fileCount; i++)
            {
                uint entryOffset = header.fileEntry[i].offset * blockSize;
                IO.Position = header.fileEntry[i].offset * blockSize;
                header.fileEntry[i].Data = IO.In.ReadBytes(header.fileEntry[i].contentSize);
                Console.WriteLine("FILE " + i + ": " + header.fileEntry[i].fileName + ":    " + header.fileEntry[i].contentSize/1024/1024 + " MB");
            }
            Extract();
        }

        public static void Extract()
        {
            for (int i = 0; i < header.fileCount; i++)
            {
                Console.WriteLine("EXTRACTING... ({0}/{1}) {2}mb: " + Environment.CurrentDirectory + "\\Firmware\\" + header.fileEntry[i].fileName,i + 1,header.fileCount, header.fileEntry[i].contentSize/1024/1024);
                File.WriteAllBytes(Environment.CurrentDirectory + "\\Firmware\\" + header.fileEntry[i].fileName, header.fileEntry[i].Data);
            }
            Console.WriteLine("SUCCESS!");
        }
    }
}
