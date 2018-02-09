using System;
using System.Collections.Generic;
using System.IO;

namespace theawesomecoder61.Helpers
{
    public class BetterBinaryReader : IDisposable
    {
        public string FileName;
        public bool Initiated;
        public Stream Stream;

        private BinaryReader br;

        public BetterBinaryReader()
        {
            Initiated = false;
        }

        public BetterBinaryReader(string file)
        {
            Initiated = false;
            Load(file);
        }

        public BetterBinaryReader(Stream s)
        {
            Initiated = false;
            FileName = "";
            Stream = s;
            br = new BinaryReader(Stream);
        }

        public void Dispose()
        {
            Initiated = false;

            br.Close();
            br = null;

            Stream.Close();
            Stream = null;
        }

        /// <summary>
        /// Loads a file.
        /// </summary>
        /// <param name="file">The file name to load.</param>
        public void Load(string file)
        {
            FileName = file;
            Stream = new FileStream(file, FileMode.Open);
            br = new BinaryReader(Stream);

            Initiated = true;
        }

        /// <summary>
        /// Sets the current position.
        /// </summary>
        /// <param name="o">offset as a long, from the beginning of the file</param>
        public void Seek(long o)
        {
            if (o > -1)
            {
                Stream.Seek(o, SeekOrigin.Begin);
            }
        }

        /// <summary>
        /// Increments or decrements the current position.
        /// </summary>
        /// <param name="o">offset as a long, can be negative to go backward</param>
        public void Skip(long o)
        {
            Stream.Seek(o, SeekOrigin.Current);
        }

        /// <summary>
        /// Gets the current position.
        /// </summary>
        /// <returns>The method returns the current position as a long.</returns>
        public long Position()
        {
            return Stream.Position;
        }

        /// <summary>
        /// Returns an int of the current byte.
        /// </summary>
        /// <returns>The method returns an int.</returns>
        public int Read()
        {
            return br.ReadBytes(1)[0]; // br.Read() causes the error "output char buffer is too small..."
        }

        /// <summary>
        /// ???
        /// </summary>
        /// <param name="buffer">the data to read from as a byte array</param>
        /// <param name="index">the offset to read from</param>
        /// <param name="count">the amount of bytes to read</param>
        /// <returns>Why does this return an int?</returns>
        public int Read(byte[] buffer, int index, int count)
        {
            return br.Read(buffer, index, count);
        }
        
        /// <summary>
        /// ???
        /// </summary>
        /// <param name="buffer">the data to read from as a char array</param>
        /// <param name="index">the offset to read from</param>
        /// <param name="count">the amount of bytes to read</param>
        /// <returns>Why does this return an int?</returns>
        public int Read(char[] buffer, int index, int count)
        {
            return br.Read(buffer, index, count);
        }

        /// <summary>
        /// Returns a byte arrary with l bytes from current position. If you plan on reading gigabytes or using longs, use ReadBytesButLonger().
        /// </summary>
        /// <param name="l">length of bytes to read, is an int</param>
        /// <returns>The method returns a byte array.</returns>
        public byte[] ReadBytes(int l)
        {
            if (l < 0 || l > int.MaxValue)
            {
                return new byte[] { };
            }
            else
            {
                return br.ReadBytes(l);
            }
        }

        /// <summary>
        /// Returns a stream with l bytes from current position. Note how this differs from ReadBytes(), as this supports longs.
        /// </summary>
        /// <param name="l">length of bytes to read, is a long</param>
        /// <returns></returns>
        public Stream ReadBytesButLonger(long l)
        {
            MemoryStream ms = new MemoryStream();
            for(long i=0;i<l;i++)
            {
                // ms.WriteByte((byte)Read());
            }
            Console.WriteLine(ms.Length);
            return ms;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="l">length of bytes to read, as an int</param>
        /// <returns>The method returns </returns>
        public string ReadCharsAsString(int l)
        {
            return new string(br.ReadChars(l));
        }

        /// <summary>
        /// Exactly ReadInt16().
        /// </summary>
        /// <returns>The method returns a 16-bit signed integer (aka short).</returns>
        public short ReadShort()
        {
            return br.ReadInt16();
        }

        /// <summary>
        /// Reads 2 bytes as a 16-bit signed integer.
        /// </summary>
        /// <returns>The method returns a 16-bit signed integer (aka short).</returns>
        public short ReadInt16()
        {
            return br.ReadInt16();
        }

        /// <summary>
        /// Exactly ReadInt32().
        /// </summary>
        /// <returns>The method returns a 32-bit signed integer (aka int).</returns>
        public int ReadInt()
        {
            return br.ReadInt32();
        }

        /// <summary>
        /// Reads 4 bytes as a 32-bit signed integer (aka int).
        /// </summary>
        /// <returns>The method returns a 32-bit signed integer (aka int).</returns>
        public int ReadInt32()
        {
            return br.ReadInt32();
        }

        /// <summary>
        /// Exactly ReadInt64().
        /// </summary>
        /// <returns>The method returns a 64-bit signed integer (aka long).</returns>
        public long ReadLong()
        {
            return br.ReadInt64();
        }

        /// <summary>
        /// Reads 8 bytes as a 64-bit signed integer (aka long).
        /// </summary>
        /// <returns>The method returns a 64-bit signed integer (aka long).</returns>
        public long ReadInt64()
        {
            return br.ReadInt64();
        }

        /// <summary>
        /// Reads some bytes as a string.
        /// </summary>
        /// <returns>The method returns a string.</returns>
        public string ReadString()
        {
            return br.ReadString();
        }
        
        private long GreatestDivisor(long n)
        {
            long d = 0;
            for (long i = 1; i < n/64; i++)
            {
                if (n % i == 0 && i != n)
                {
                    d = i;
                }
            }
            return d;
        }
    }
}