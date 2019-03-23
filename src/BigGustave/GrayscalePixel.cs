namespace BigGustave
{
    public readonly struct GrayscalePixel : IPixel
    {
        public byte Value { get; }

        public GrayscalePixel(byte value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}