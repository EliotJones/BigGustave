namespace BigGustave.Tests.Jpgs
{
    using BigGustave.Jpgs;
    using Xunit;

    public class HuffmanTableTests
    {
        [Fact]
        public void SpecificationToTable()
        {
            var spec = new HuffmanTableSpecification(
                1,
                HuffmanTableSpecification.HuffmanClass.DcTable,
                new byte[]
                {
                    0, 2, 2, 3, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0
                },
                new byte[]
                {
                    5, 6, 3, 4, 2, 7, 8, 1, 0, 9
                });

            var table = HuffmanTable.FromSpecification(spec);

            Assert.Equal(
                5,
                table.Root.Left.Left.Value.GetValueOrDefault());
            Assert.Equal(
                6,
                table.Root.Left.Right.Value.GetValueOrDefault());
        }

        [Fact]
        public void SpecificationToTableComplex()
        {
            var spec = new HuffmanTableSpecification(
                1,
                HuffmanTableSpecification.HuffmanClass.DcTable,
                new byte[]
                {
                    0, 1, 5, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0
                },
                new[]
                {
                    (byte) 'a',
                    (byte) 'b',
                    (byte) 'c',
                    (byte) 'd',
                    (byte) 'e',
                    (byte) 'f',
                    (byte) 'g',
                    (byte) 'h',
                    (byte) 'i',
                    (byte) 'j',
                    (byte) 'k',
                    (byte) 'l'
                }
            );

            var table = HuffmanTable.FromSpecification(spec);
        }
    }
}
