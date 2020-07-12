using System.Collections.Generic;

namespace BigGustave.Jpgs
{
    using System;
    using System.IO;

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

    internal class Frame
    {
        public FrameType FrameType { get; set; }

        /// <summary>
        /// Offset from the start of the file to this table's marker.
        /// </summary>
        public long Offset { get; set; }

        public short Length { get; set; }

        public byte SamplePrecision { get; set; }

        public short NumberOfLines { get; set; }

        public short NumberOfSamplesPerLine { get; set; }

        public byte NumberOfImageComponentsInFrame { get; set; }

        public FrameComponentSpecificationParameters[] FrameComponentSpecifications { get; set; }

        public List<Scan> Scans { get; } = new List<Scan>();

        public static Frame ReadFromMarker(Stream stream, bool strictMode, byte markerByte)
        {
            var frameType = (FrameType)markerByte;
            var offset = stream.Position;
            var length = stream.ReadShort();
            var samplePrecision = stream.ReadByteActual();

            if (strictMode && samplePrecision != 8)
            {
                throw new InvalidOperationException($"Sample precision should be 8 for baseline DCT frames. Got: {samplePrecision} at offset {stream.Position}.");
            }

            var numberOfLines = stream.ReadShort();
            var numberOfSamplesPerLine = stream.ReadShort();
            var numberOfImageComponentsInFrame = stream.ReadByteActual();

            var frameComponents = new FrameComponentSpecificationParameters[numberOfImageComponentsInFrame];

            for (var i = 0; i < frameComponents.Length; i++)
            {
                var identifier = stream.ReadByteActual();
                var (horizontal, vertical) = stream.ReadNibblePair();
                var quantizationTableSelector = stream.ReadByteActual();

                frameComponents[i] = new FrameComponentSpecificationParameters(identifier, horizontal, vertical, quantizationTableSelector);
            }

            if (strictMode && stream.Position - offset != length)
            {
                throw new InvalidOperationException($"Read incorrect number of bytes for frame header ({stream.Position - offset})," +
                                                    $" should have read {length} bytes at offset {offset}..");
            }
            
            return new Frame
            {
                Offset = offset,
                FrameType = frameType,
                Length = length,
                SamplePrecision = samplePrecision,
                FrameComponentSpecifications = frameComponents,
                NumberOfImageComponentsInFrame = numberOfImageComponentsInFrame,
                NumberOfLines = numberOfLines,
                NumberOfSamplesPerLine = numberOfSamplesPerLine
            };
        }

        public readonly struct FrameComponentSpecificationParameters
        {
            /// <summary>
            /// Assigns a unique label to the ith component in the sequence of frame component specification parameters.
            /// These values shall be used in the scan headers to identify the components in the scan.
            /// The value of Ci shall be different from the values of C1 through Ci − 1.
            /// </summary>
            public byte ComponentIdentifier { get; }

            /// <summary>
            /// Specifies the relationship between the component horizontal dimension and maximum image dimension X; 
            /// also specifies the number of horizontal data units of component Ci in each MCU, when more than one component is encoded in a scan.
            /// </summary>
            public byte HorizontalSamplingFactor { get; }

            /// <summary>
            /// Specifies the relationship between the component vertical dimension and maximum image dimension Y ; 
            /// also specifies the number of vertical data units of component Ci in each MCU, when more than one component is encoded in a scan.
            /// </summary>
            public byte VerticalSamplingFactor { get; }

            /// <summary>
            /// Specifies one of four possible quantization table destinations
            /// from which the quantization table to use for dequantization of DCT coefficients of component Ci is retrieved. If
            /// the decoding process uses the dequantization procedure, this table shall have been installed in this destination
            /// by the time the decoder is ready to decode the scan(s) containing component Ci.
            /// The destination shall not be re-specified, or its contents changed, until all scans containing Ci have been completed.
            /// </summary>
            public byte DestinationQuantizationTableSelector { get; }

            public FrameComponentSpecificationParameters(byte componentIdentifier, byte horizontalSamplingFactor,
                byte verticalSamplingFactor,
                byte destinationQuantizationTableSelector)
            {
                ComponentIdentifier = componentIdentifier;
                HorizontalSamplingFactor = horizontalSamplingFactor;
                VerticalSamplingFactor = verticalSamplingFactor;
                DestinationQuantizationTableSelector = destinationQuantizationTableSelector;
            }
        }
    }
}
