namespace BigGustave
{
    public readonly struct GrayscaleAlphaPixel : IPixel
    {
        public byte Value { get; }

        public byte Alpha { get; }

        public GrayscaleAlphaPixel(byte value, byte alpha)
        {
            Value = value;
            Alpha = alpha;
        }
    }
}