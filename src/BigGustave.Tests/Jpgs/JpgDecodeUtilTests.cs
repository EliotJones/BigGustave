namespace BigGustave.Tests.Jpgs
{
    using System.Collections.Generic;
    using BigGustave.Jpgs;
    using Xunit;

    public class JpgDecodeUtilTests
    {
        [Fact]
        public void GetDifferenceTableValueFromExampleBitStream()
        {
            // Example from decode scan data section of https://koushtav.me/jpeg/tutorial/c++/decoder/2019/03/02/lets-write-a-simple-jpeg-library-part-2/#implementing-the-decoder
            var data = new byte[] {0b11000001};

            var bitStream = new BitStream(data);

            var rawRead = bitStream.ReadNBits(3);

            Assert.Equal(0b110, rawRead);

            const int actualCategoryFromHuffman = 5;

            var binaryAsInt = bitStream.ReadNBits(actualCategoryFromHuffman);

            Assert.Equal(0b00001, binaryAsInt);

            var differenceValue = JpgDecodeUtil.GetDcDifference(actualCategoryFromHuffman, binaryAsInt);

            Assert.Equal(-30, differenceValue);
        }

        [Fact]
        public void DifferenceMagnitudeCategoriesCorrectlyCalculated()
        {
            // Difference table from page 93 of https://www.w3.org/Graphics/JPEG/itu-t81.pdf
            var items = new Dictionary<int, List<(int expected, int input)>>
            {
                {
                    0, new List<(int expected, int input)>
                    {
                        (0, 0)
                    }
                },
                {
                    1, new List<(int expected, int input)>
                    {
                        (-1, 0b0),
                        (1, 0b1)
                    }
                },
                {
                    2, new List<(int expected, int input)>
                    {
                        (-3, 0b00),
                        (-2, 0b01),
                        (2, 0b10),
                        (3, 0b11)
                    }
                },
                {
                    3, new List<(int expected, int input)>
                    {
                        (-7, 0b000),
                        (-6, 0b001),
                        (-5, 0b010),
                        (-4, 0b011),
                        (4, 0b100),
                        (5, 0b101),
                        (6, 0b110),
                        (7, 0b111)
                    }
                },
                {
                    4, new List<(int expected, int input)>
                    {
                        (-15, 0b0000),
                        (-14, 0b0001),
                        (-13, 0b0010),
                        (-8, 0b0111),
                        (8, 0b1000),
                        (9, 0b1001),
                        (15, 0b1111)
                    }
                }
            };

            foreach (var item in items)
            {
                var category = item.Key;

                foreach (var (expected, input) in item.Value)
                {
                    var actual = JpgDecodeUtil.GetDcDifference(category, input);

                    Assert.Equal(expected, actual);
                }
            }
        }
    }
}
