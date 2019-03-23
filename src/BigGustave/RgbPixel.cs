namespace BigGustave
{
    public readonly struct RgbPixel : IPixel
    {
        public byte R { get; }

        public byte G { get; }

        public byte B { get; }

        public RgbPixel(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }
    }
}