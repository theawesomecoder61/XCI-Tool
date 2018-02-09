using System;
using System.Collections.Generic;
using theawesomecoder61.Helpers;

namespace XCI_Tool
{
    public class HFS0
    {
        public static byte[] MAGIC_HEAD = new byte[] { 0x48, 0x45, 0x41, 0x44 };
        public static byte[] MAGIC_HFS0 = new byte[] { 0x48, 0x46, 0x53, 0x30 };

        public struct HFS0_ENTRY
        {
            public long Offset; // relative to end of hfs0 header
            public long Size;
            public int ofsNameTable; // relative to start of Name Table
            public int szHeader; // size of the file header, default = 0 or 0x200
            public byte[] padding;
            public byte[] hash;

            public HFS0_ENTRY(BetterBinaryReader bbr)
            {
                Offset = bbr.ReadInt64();
                Size = bbr.ReadInt64();
                ofsNameTable = bbr.ReadInt32();
                szHeader = bbr.ReadInt32();
                padding = bbr.ReadBytes(0x8);
                hash = bbr.ReadBytes(0x20); // hash over the start of file, with szHeader as end
            }
        }

        public List<HFS0_ENTRY> entries;
        public List<string> names;
        public int FileCount;

        public HFS0()
        {
            entries = new List<HFS0_ENTRY>();
            names = new List<string>();
        }

        public void ReadData(BetterBinaryReader bbr)
        {
            int szNameTable;
            Console.WriteLine(bbr.Position());

            //read Header
            byte[] magic = bbr.ReadBytes(4);
            if (magic[0] != ToDec(MAGIC_HFS0[0]) && magic[1] != ToDec(MAGIC_HFS0[1]) && magic[2] != ToDec(MAGIC_HFS0[2]) && magic[3] != ToDec(MAGIC_HFS0[3]))
            {
                throw new Exception("Invalid magic!");
            }

            FileCount = bbr.ReadInt32();
            szNameTable = bbr.ReadInt32();
            bbr.ReadInt32(); //padding

            //read entries
            for (int i = 0; i < FileCount; i++)
            {
                entries.Add(new HFS0_ENTRY(bbr));
            }

            //read names
            long posNT = bbr.Position();

            for (int i = 0; i < FileCount; i++)
            {
                bbr.Seek(posNT + entries[i].ofsNameTable);

                string name = "";
                List<char> chars = new List<char>();
                int c;
                while ((c = bbr.Read()) != 0)
                {
                    if (c == 0)
                    {
                        break;
                    }
                    else
                    {
                        chars.Add((char)c);
                    }
                }

                Console.WriteLine(bbr.Position());

                names.Add(new string(chars.ToArray()));
            }

            //jump to end of HFS0 header
            bbr.Seek(posNT + szNameTable);
        }

        private int ToDec(byte b)
        {
            return Convert.ToInt32(b);
        }
    }
}