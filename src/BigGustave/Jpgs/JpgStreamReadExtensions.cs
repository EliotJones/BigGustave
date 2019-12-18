namespace BigGustave.Jpgs
{
    using System;
    using System.IO;

    internal static class JpgStreamReadExtensions
    {
        private const byte MarkerStart = 255;

        private static readonly byte[] ShortBuffer = new byte[2];
        private static readonly object ShortLock = new object();

        public static short ReadShort(this Stream stream)
        {
            lock (ShortLock)
            {
                var read = stream.Read(ShortBuffer, 0, ShortBuffer.Length);

                if (read != ShortBuffer.Length)
                {
                    throw new InvalidOperationException();
                }
                
                // For parameters which are 2 bytes in length, the most significant byte shall come first
                // in the compressed data's ordered sequence of bytes.
                return (short)((ShortBuffer[0] << 8) + ShortBuffer[1]);
            }
        }

        public static byte ReadByteActual(this Stream stream)
        {
            var val = stream.ReadByte();

            if (val < 0)
            {
                throw new InvalidOperationException();
            }

            return (byte) val;
        }

        public static (byte, byte) ReadNibblePair(this Stream stream)
        {
            var b = ReadByteActual(stream);

            // Parameters which are 4 bits in length always come in pairs, and the pair shall always be encoded in a single byte.
            // The first 4-bit parameter of the pair shall occupy the most significant 4 bits of the byte.
            return ((byte)(b >> 4), (byte)(b & 0x0F));
        }
        
        public static byte ReadSegmentMarker(this Stream stream, bool skipData = false, string message = null)
        {
            byte? previous = null;
            int currentValue;
            while ((currentValue = stream.ReadByte()) != -1)
            {
                var b = (byte)currentValue;

                if (!skipData)
                {
                    if (!previous.HasValue && b != MarkerStart)
                    {
                        throw new InvalidOperationException();
                    }

                    if (b != MarkerStart)
                    {
                        return b;
                    }
                }

                if (previous.HasValue && previous.Value == MarkerStart && b != MarkerStart)
                {
                    return b;
                }

                previous = b;
            }

            throw new InvalidOperationException();
        }
    }
}