namespace BigGustave.Tests
{
    using Xunit;

    public class Adler32ChecksumTests
    {
        [Fact]
        public void CalculatesCorrectChecksum()
        {
            var data = new byte[]
            {
                0,
                255, 0, 0,
                0, 0, 0,
                0,
                0, 0, 0,
                255, 0, 0
            };

            var checksum = Adler32Checksum.Calculate(data);

            Assert.Equal(268304895, checksum);
        }
    }
}
