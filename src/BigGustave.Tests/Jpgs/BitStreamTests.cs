namespace BigGustave.Tests.Jpgs
{
    using System.Collections.Generic;
    using System.Linq;
    using BigGustave.Jpgs;
    using Xunit;

    public class BitStreamTests
    {
        [Fact]
        public void ReadsExpectedBitStream()
        {
            var bytes = new[] {(byte) 'a', (byte) 'b', (byte) 'c'};

            var output = new List<int>();
            var str = new BitStream(bytes);

            int current;
            do
            {
                current = str.Read();
                if (current > -1)
                {
                    output.Add(current);
                }
            } while (current > -1);

            var expected = new[]
            {
                0, 1, 1, 0, 0, 0, 0, 1, 0, 1, 1, 0, 0, 0, 1, 0, 0, 1, 1, 0, 0, 0,
            };

            Assert.Equal(expected, output.Take(expected.Length));
            Assert.Equal(3 * 8, output.Count);
            Assert.Equal(-1, str.Read());
        }
    }
}