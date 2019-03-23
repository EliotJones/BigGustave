namespace BigGustave
{
    public readonly struct GrayscalePixel : IPixel
    {
        public byte Value { get; }

        public GrayscalePixel(byte value)
        {
            Value = value;
        }
    }
}