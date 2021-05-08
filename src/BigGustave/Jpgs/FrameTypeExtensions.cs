namespace BigGustave.Jpgs
{
    internal static class FrameTypeExtensions
    {
        public static bool IsHuffman(FrameType frameType)
        {
            var b = (byte) frameType;
            return b >= 0xC0 && b <= 0xC7 && b != 0xC4;
        }

        public static bool IsArithmetic(FrameType frameType)
        {
            var b = (byte)frameType;
            return b >= 0xC9 && b <= 0xCF && b != 0xCC;
        }
    }
}