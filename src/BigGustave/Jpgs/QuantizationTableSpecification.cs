namespace BigGustave.Jpgs
{
    using System;
    using System.IO;

    internal class QuantizationTableSpecification
    {
        /// <summary>
        /// Offset from the start of the file to this table's marker.
        /// </summary>
        public long Offset { get; }

        /// <summary>
        /// Quantization table definition length – Specifies the length of all quantization table parameters 
        /// </summary>
        public short Length { get; }

        /// <summary>
        /// Specifies the bit-precision of quantization table elements. 
        /// Value 0 indicates 1 byte values.
        /// Value 1 indicates 2 byte values.
        /// </summary>
        public byte ElementPrecision { get; }

        /// <summary>
        /// Specifies one of four possible destinations at the decoder into which the quantization table shall be installed.
        /// </summary>
        public byte TableDestinationIdentifier { get; }

        /// <summary>
        /// Specifies the 64 elements for the quantization table in zig-zag ordering.
        /// </summary>
        public short[] QuantizationTableElements { get; }

        /// <summary>
        /// Whether the elements in <see cref="QuantizationTableElements"/> are in 16-bit precision.
        /// </summary>
        public bool Uses16BitElements => ElementPrecision == 1;

        public QuantizationTableSpecification(long offset, short length, byte elementPrecision, 
            byte tableDestinationIdentifier, 
            short[] quantizationTableElements)
        {
            Offset = offset;
            Length = length;
            ElementPrecision = elementPrecision;
            TableDestinationIdentifier = tableDestinationIdentifier;
            QuantizationTableElements = quantizationTableElements ?? throw new ArgumentNullException(nameof(quantizationTableElements));

            if (elementPrecision > 1)
            {
                throw new ArgumentException($"Invalid element precision ({elementPrecision}) in quantization table at offset: {offset}.");
            }

            if (QuantizationTableElements.Length != 64)
            {
                throw new ArgumentException($"Invalid quantization table length, should be 64 but got: {QuantizationTableElements.Length}.");
            }
        }

        /// <summary>
        /// Reads the quantization table starting from the marker byte <see cref="JpgMarkers.DefineQuantizationTable"/>.
        /// </summary>
        public static QuantizationTableSpecification[] ReadFromMarker(Stream stream, bool strictMode)
        {
            var offset = stream.Position;
            var length = stream.ReadShort();

            // Section may contain multiple quantization tables, each with its own information byte.
            var quantizationTableCount = length / 65;

            var result = new QuantizationTableSpecification[quantizationTableCount];

            for (var qtIndex = 0; qtIndex < quantizationTableCount; qtIndex++)
            {
                var (elementPrecision, destinationId) = stream.ReadNibblePair();


                var uses16BitValues = false;
                if (elementPrecision == 1)
                {
                    uses16BitValues = true;
                }
                else if (elementPrecision != 0)
                {
                    throw new InvalidOperationException(
                        "Invalid value for quantization table element precision, should be 0 or 1, " +
                        $"got: {elementPrecision} at offset {stream.Position}.");
                }

                var data = new short[64];
                for (var i = 0; i < data.Length; i++)
                {
                    data[i] = uses16BitValues ? stream.ReadShort() : stream.ReadByteActual();
                }

                result[qtIndex] = new QuantizationTableSpecification(offset, length, elementPrecision, destinationId, data);
            }

            var lengthRead = stream.Position - offset;

            if (lengthRead != length && strictMode)
            {
                throw new InvalidOperationException($"Length read ({lengthRead}) for quantization table at offset {offset} " +
                                                    $"did not match length specified ({length}). Set strictMode to false to ignore this.");
            }

            return result;
        }
    }
}