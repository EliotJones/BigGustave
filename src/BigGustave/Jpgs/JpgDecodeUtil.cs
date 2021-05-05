namespace BigGustave.Jpgs
{
    internal static class JpgDecodeUtil
    {
        public static int GetDcDifference(int category, int value)
        {
            if (category == 0)
            {
                return 0;
            }

            var isNormalValue = (value >> (category - 1)) > 0;

            if (isNormalValue)
            {
                return value;
            }

            var lower = -((int) (2 << (category - 1))) + 1;

            return lower + value;
        }
    }
}