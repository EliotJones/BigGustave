namespace BigGustave
{
    public readonly struct Pixel
    {
        public byte R { get; }

        public byte G { get; }

        public byte B { get; }

        public byte A { get; }

        public bool IsGrayscale { get; }

        public Pixel(byte r, byte g, byte b, byte a, bool isGrayscale)
        {
            R = r;
            G = g;
            B = b;
            A = a;
            IsGrayscale = isGrayscale;
        }

        public override bool Equals(object obj)
        {
            if (obj is Pixel pixel)
            {
                return IsGrayscale == pixel.IsGrayscale
                       && A == pixel.A
                       && R == pixel.R
                       && G == pixel.G
                       && B == pixel.B;
            }

            return false;
        }

        public bool Equals(Pixel other)
        {
            return R == other.R && G == other.G && B == other.B && A == other.A && IsGrayscale == other.IsGrayscale;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = R.GetHashCode();
                hashCode = (hashCode * 397) ^ G.GetHashCode();
                hashCode = (hashCode * 397) ^ B.GetHashCode();
                hashCode = (hashCode * 397) ^ A.GetHashCode();
                hashCode = (hashCode * 397) ^ IsGrayscale.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"({R}, {G}, {B}, {A})";
        }
    }
}