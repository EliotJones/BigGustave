using Xunit;

namespace BigGustave.Tests
{
    using System.IO;

    public class PngTests
    {
        [Fact]
        public void Test()
        {
            using (var stream = File.OpenRead(@"C:\Users\eliot\OneDrive\Pictures\g5319.png"))
            {
                Png.Open(stream);
            }
        }
    }
}
