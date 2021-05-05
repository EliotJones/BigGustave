namespace BigGustave.Jpgs
{
    internal static class JpgDecodeUtil
    {
        public static readonly byte[] ZigZagPattern = new byte[]
        {
            0,  1,  8, 16,  9,  2,  3, 10,
            17, 24, 32, 25, 18, 11,  4,  5,
            12, 19, 26, 33, 40, 48, 41, 34,
            27, 20, 13,  6,  7, 14, 21, 28,
            35, 42, 49, 56, 57, 50, 43, 36,
            29, 22, 15, 23, 30, 37, 44, 51,
            58, 59, 52, 45, 38, 31, 39, 46,
            53, 60, 61, 54, 47, 55, 62, 63
        };

        public static int GetDcDifferenceOrAcCoefficient(int category, int value)
        {
            /*
             * This code is a bit confusing, basically the DC coefficient in an MCU is encoded first as a category then as the bits being the value of the difference.
             * The relationship between the category and the value is a bit unclear, but the difference magnitude categories for DC coding are explained in more
             * detail in the "Value Category and Bitstream Table" section here: https://koushtav.me/jpeg/tutorial/2017/11/25/lets-write-a-simple-jpeg-library-part-1/#encoding-the-dc-coeffs.
             */
            if (category == 0)
            {
                return 0;
            }

            // The most significant bit is 1 for positive and 0 for negative values.
            var isPositive = (value >> (category - 1)) > 0;

            if (isPositive)
            {
                return value;
            }

            var lower = -(2 << (category - 1)) + 1;

            return lower + value;
        }
    }
}