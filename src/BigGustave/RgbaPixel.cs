namespace BigGustave
{
    public readonly struct RgbaPixel : IPixel
    {
        public byte R { get; }

        public byte G { get; }

        public byte B { get; }

        public byte A { get; }

        public RgbaPixel(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public override string ToString()
        {
            return $"{R}, {G}, {B}, {A}";
        }
    }
}