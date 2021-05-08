namespace BigGustave.Jpgs
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    internal class Frame
    {
        public FrameType FrameType { get; }

        /// <summary>
        /// Offset from the start of the file to this table's marker.
        /// </summary>
        public long Offset { get; }

        public byte BitsPerSample { get; }

        public short ImageHeight { get; }

        public short ImageWidth { get; }

        /// <summary>
        /// Generally 1 = grayscale, 3 = YCbCr or YIQ, 4 = CMYK.
        /// </summary>
        public byte NumberOfComponents { get; }

        public FrameComponentSpecificationParameters[] Components { get; }

        public int McusPerX { get; }

        public int McusPerY { get; }

        public int MaxHorizontalSamplingFactor { get; }

        public int MaxVerticalSamplingFactor { get; }

        public List<Scan> Scans { get; } = new List<Scan>();

        public Frame(
            FrameType frameType,
            long offset,
            byte bitsPerSample,
            short imageHeight,
            short imageWidth,
            byte numberOfComponents,
            FrameComponentSpecificationParameters[] components,
            int mcusPerX,
            int mcusPerY,
            int maxHorizontalSamplingFactor,
            int maxVerticalSamplingFactor)
        {
            FrameType = frameType;
            Offset = offset;
            BitsPerSample = bitsPerSample;
            ImageHeight = imageHeight;
            ImageWidth = imageWidth;
            NumberOfComponents = numberOfComponents;
            Components = components;
            McusPerX = mcusPerX;
            McusPerY = mcusPerY;
            MaxHorizontalSamplingFactor = maxHorizontalSamplingFactor;
            MaxVerticalSamplingFactor = maxVerticalSamplingFactor;
        }

        public static Frame ReadFromMarker(Stream stream, bool strictMode, byte markerByte)
        {
            var frameType = (FrameType)markerByte;
            var offset = stream.Position;
            var length = stream.ReadShort();
            var bitsPerSample = stream.ReadByteActual();

            if (strictMode && bitsPerSample != 8)
            {
                throw new InvalidOperationException($"Sample precision should be 8 for baseline DCT frames. Got: {bitsPerSample} at offset {stream.Position}.");
            }

            var imageHeight = stream.ReadShort();
            var imageWidth = stream.ReadShort();
            var numberOfComponents = stream.ReadByteActual();

            var frameComponents = new FrameComponentSpecificationParameters[numberOfComponents];

            var maxHorizontalFactor = 0;
            var maxVerticalFactor = 0;
            for (var i = 0; i < frameComponents.Length; i++)
            {
                var identifier = stream.ReadByteActual();
                var (horizontal, vertical) = stream.ReadNibblePair();
                var quantizationTableSelector = stream.ReadByteActual();

                if (horizontal > maxHorizontalFactor)
                {
                    maxHorizontalFactor = horizontal;
                }

                if (vertical > maxVerticalFactor)
                {
                    maxVerticalFactor = vertical;
                }

                frameComponents[i] = new FrameComponentSpecificationParameters(identifier, horizontal, vertical, quantizationTableSelector);
            }

            if (strictMode && stream.Position - offset != length)
            {
                throw new InvalidOperationException($"Read incorrect number of bytes for frame header ({stream.Position - offset})," +
                                                    $" should have read {length} bytes at offset {offset}..");
            }

            // When a frame contains more than 1 component (?).


            var adjustX = imageWidth % 8 == 0 ? 0 : 1;
            var adjustY = imageHeight % 8 == 0 ? 0 : 1;

            maxVerticalFactor = maxVerticalFactor > 0 ? maxVerticalFactor : 1;
            maxHorizontalFactor = maxHorizontalFactor > 0 ? maxHorizontalFactor : 1;

            var mcusPerX = (imageWidth / 8 + adjustX) / (double)maxHorizontalFactor;
            var mcusPerY = (imageHeight / 8 + adjustY) / (double)maxVerticalFactor;

            return new Frame(
                frameType,
                offset,
                bitsPerSample,
                imageHeight,
                imageWidth,
                numberOfComponents,
                frameComponents,
                (byte)Math.Ceiling(mcusPerX),
                (byte)Math.Ceiling(mcusPerY),
                maxHorizontalFactor,
                maxVerticalFactor);
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
            /// from which the quantization table to use for de-quantization of DCT coefficients of component Ci is retrieved. If
            /// the decoding process uses the de-quantization procedure, this table shall have been installed in this destination
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
