namespace BigGustave.Jpgs
{
    internal enum FrameType : byte
    {
        BaselineHuffman = 0xC0,
        ExtendedSequentialHuffman = 0xC1,
        ProgressiveHuffman = 0xC2,
        LosslessHuffman = 0xC3,
        DifferentialSequentialDctHuffman = 0xC5,
        DifferentialProgressiveDctHuffman = 0xC6,
        DifferentialLosslessHuffman = 0xC7,
        ExtendedSequentialArithmetic = 0xC9,
        ProgressiveArithmetic = 0xCA,
        LosslessArithmetic = 0xCB,
        DifferentialSequentialDctArithmetic = 0xCD,
        DifferentialProgressiveDctArithmetic = 0xCE,
        DifferentialLosslessArithmetic = 0xCF,
    }
}