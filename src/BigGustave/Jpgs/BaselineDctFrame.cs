namespace BigGustave.Jpgs
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    internal class BaselineDctFrame
    {
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

        public static BaselineDctFrame ReadFromMarker(Stream stream, bool strictMode)
        {
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

            var scans = new List<Scan>();

            // TODO: return at this point and read scans in the main image parsing.
            JpgMarkers scanMarker;
            while ((scanMarker = (JpgMarkers)stream.ReadSegmentMarker(true)) == JpgMarkers.StartOfScan)
            {
                var scanOffset = stream.Position;
                var scanHeaderLength = stream.ReadShort();

                var numberOfImageComponentsInScan = stream.ReadByteActual();

                var scanComponents = new ScanComponentSpecificationParameters[numberOfImageComponentsInScan];

                for (var i = 0; i < scanComponents.Length; i++)
                {
                    var cid = stream.ReadByteActual();
                    var (dc, ac) = stream.ReadNibblePair();

                    scanComponents[i] = new ScanComponentSpecificationParameters(cid, dc, ac);
                }

                var spectralOrPredictorSelectionStart = stream.ReadByteActual();

                var spectralSelectionEnd = stream.ReadByteActual();

                var (successiveApproximationHigh, successiveApproximationLow) = stream.ReadNibblePair();

                scans.Add(new Scan(scanOffset, scanHeaderLength, scanComponents,
                    spectralOrPredictorSelectionStart, spectralSelectionEnd,
                    successiveApproximationHigh, successiveApproximationLow));
            }

            // Rewind marker.
            stream.Seek(-2, SeekOrigin.Current);
            
            return new BaselineDctFrame
            {
                FrameComponentSpecifications = frameComponents,
                NumberOfImageComponentsInFrame = numberOfImageComponentsInFrame,
                NumberOfLines = numberOfLines,
                NumberOfSamplesPerLine = numberOfSamplesPerLine
            };
        }

        public class Scan
        {
            public long Offset { get; }

            public short Length { get; }

            public ScanComponentSpecificationParameters[] ScanComponents { get; }

            public byte StartSpectralOrPredictorSelection { get; }

            public byte EndSpectralSelection { get; }

            public byte SuccessiveApproximationBitHigh { get; }

            public byte SuccessiveApproximationBitLow { get; }

            public Scan(long offset, short length, ScanComponentSpecificationParameters[] scanComponents,
                byte startSpectralOrPredictorSelection,
                byte endSpectralSelection,
                byte successiveApproximationBitHigh,
                byte successiveApproximationBitLow)
            {
                Offset = offset;
                Length = length;
                ScanComponents = scanComponents ?? throw new ArgumentNullException(nameof(scanComponents));
                StartSpectralOrPredictorSelection = startSpectralOrPredictorSelection;
                EndSpectralSelection = endSpectralSelection;
                SuccessiveApproximationBitHigh = successiveApproximationBitHigh;
                SuccessiveApproximationBitLow = successiveApproximationBitLow;
            }
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

        public readonly struct ScanComponentSpecificationParameters
        {
            /// <summary>
            /// Scan component selector - selects which of the image components specified in
            /// the frame parameters shall be this component in the scan.
            /// </summary>
            public byte ComponentSelector { get; }

            /// <summary>
            /// Specifies 1 of 4 possible DC entropy coding table destinations
            /// from which the entropy table needed for decoding the DC coefficients
            /// of this component is retrieved.
            /// </summary>
            public byte DCEntropyCodingTableSelector { get; }

            /// <summary>
            /// Specifies 1 of 4 possible AC entropy coding table destinations
            /// from which the entropy table needed for decoding the AC coefficients
            /// of this component is retrieved.
            /// </summary>
            public byte ACEntropyCodingTableSelector { get; }

            public ScanComponentSpecificationParameters(byte componentSelector,
                byte dcEntropyCodingTableSelector,
                byte acEntropyCodingTableSelector)
            {
                ComponentSelector = componentSelector;
                DCEntropyCodingTableSelector = dcEntropyCodingTableSelector;
                ACEntropyCodingTableSelector = acEntropyCodingTableSelector;
            }
        }
    }
}
