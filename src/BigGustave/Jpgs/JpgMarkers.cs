﻿namespace BigGustave.Jpgs
{
    internal enum JpgMarkers : byte
    {
        /// <summary>
        /// Indicates that this is a baseline DCT-based JPEG, and specifies the width, height, number of components, and component subsampling.
        /// </summary>
        StartOfBaselineDctHuffmanFrame = 0xC0,
        /// <summary>
        /// Indicates that this is a extended sequential DCT-based JPEG.
        /// </summary>
        StartOfExtendedSequentialDctHuffmanFrame = 0xC1,
        /// <summary>
        /// Indicates that this is a progressive DCT-based JPEG, and specifies the width, height, number of components, and component subsampling.
        /// </summary>
        StartOfProgressiveDctHuffmanFrame = 0xC2,
        StartOfLosslessHuffmanFrame = 0xC3,
        /// <summary>
        /// Specifies one or more Huffman tables.
        /// </summary>
        DefineHuffmanTable = 0xC4,
        StartOfDifferentialSequentialDctHuffmanFrame = 0xC5,
        StartOfDifferentialProgressiveDctHuffmanFrame = 0xC6,
        StartOfDifferentialLosslessHuffmanFrame = 0xC7,
        StartOfExtendedSequentialDctArithmeticFrame = 0xC9,
        StartOfProgressiveDctArithmeticFrame = 0xCA,
        StartOfLosslessArithmeticFrame = 0xCB,
        DefineArithmeticCodingConditioning = 0xCC,
        StartOfDifferentialSequentialDctArithmeticFrame = 0xCD,
        StartOfDifferentialProgressiveDctArithmeticFrame = 0xCE,
        StartOfDifferentialLosslessArithmeticFrame = 0xCF,
        /// <summary>
        /// Begins a top-to-bottom scan of the image. In baseline images, there is generally a single scan.
        /// Progressive images usually contain multiple scans. 
        /// </summary>
        StartOfScan = 0xDA,
        /// <summary>
        /// Specifies one or more quantization tables. 
        /// </summary>
        DefineQuantizationTable = 0xDB,
        /// <summary>
        /// Specifies the interval between RSTn markers, in Minimum Coded Units (MCUs).
        /// This marker is followed by two bytes indicating the fixed size so it can be treated like any other variable size segment.
        /// </summary>
        DefineRestartInterval = 0xDD,
        /// <summary>
        /// Inserted every r macroblocks.
        /// </summary>
        Restart0 = 0xD0,
        /// <summary>
        /// Inserted every r macroblocks.
        /// </summary>
        Restart1 = 0xD1,
        /// <summary>
        /// Inserted every r macroblocks.
        /// </summary>
        Restart2 = 0xD2,
        /// <summary>
        /// Inserted every r macroblocks.
        /// </summary>
        Restart3 = 0xD3,
        /// <summary>
        /// Inserted every r macroblocks.
        /// </summary>
        Restart4 = 0xD4,
        /// <summary>
        /// Inserted every r macroblocks.
        /// </summary>
        Restart5 = 0xD5,
        /// <summary>
        /// Inserted every r macroblocks.
        /// </summary>
        Restart6 = 0xD6,
        /// <summary>
        /// Inserted every r macroblocks.
        /// </summary>
        Restart7 = 0xD7,
        /// <summary>
        /// Marks the start of a JPEG image file.
        /// </summary>
        StartOfImage = 0xD8,
        /// <summary>
        /// Marks the end of a JPEG image file.
        /// </summary>
        EndOfImage = 0xD9,
        ApplicationSpecific0 = 0xE0,
        ApplicationSpecific1 = 0xE1,
        ApplicationSpecific2 = 0xE2,
        ApplicationSpecific3 = 0xE3,
        ApplicationSpecific4 = 0xE4,
        ApplicationSpecific5 = 0xE5,
        ApplicationSpecific6 = 0xE6,
        ApplicationSpecific7 = 0xE7,
        ApplicationSpecific8 = 0xE8,
        ApplicationSpecific9 = 0xE9,
        /// <summary>
        /// Marks a text comment.
        /// </summary>
        Comment = 0xFE
    }
}