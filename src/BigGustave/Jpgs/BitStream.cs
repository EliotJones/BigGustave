namespace BigGustave.Jpgs
{
    using System;
    using System.Collections.Generic;

    internal class BitStream
    {
        private int bitOffset;
        private readonly IReadOnlyList<byte> data;

        public BitStream(IReadOnlyList<byte> data)
        {
            this.data = data;
        }

        public int Read()
        {
            var byteIndex = bitOffset / 8;

            if (byteIndex >= data.Count)
            {
                return -1;
            }

            var byteVal = data[byteIndex];

            var withinByteIndex = bitOffset - (byteIndex * 8);

            bitOffset++;

            // TODO: LSB?
            return ((1 << (7 - withinByteIndex)) & byteVal) > 0 ? 1 : 0;
        }

        public int ReadNBits(int length)
        {
            var result = 0;
            for (var i = 0; i < length; i++)
            {
                var bit = Read();

                if (bit < 0)
                {
                    return 0;
                    throw new InvalidOperationException($"Encountered end of bit stream while trying to read {length} bytes.");
                }

                result = (result << 1) + bit;
            }

            return result;
        }
    }
}