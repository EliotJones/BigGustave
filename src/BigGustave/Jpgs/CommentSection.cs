namespace BigGustave.Jpgs
{
    using System;
    using System.IO;
    using System.Text;

    /// <summary>
    /// A comment in a JPG file.
    /// </summary>
    internal class CommentSection
    {
        /// <summary>
        /// Offset from the start of the file to this table's marker.
        /// </summary>
        public long Offset { get; }

        /// <summary>
        /// Length of the comment in bytes.
        /// </summary>
        public short Length { get; }

        /// <summary>
        /// The bytes of the comment.
        /// </summary>
        public byte[] Bytes { get; }

        /// <summary>
        /// The comment bytes interpreted in ASCII.
        /// </summary>
        public string Text { get; }

        private CommentSection(long offset, short length, byte[] bytes)
        {
            Offset = offset;
            Length = length;
            Bytes = bytes;
            Text = Encoding.ASCII.GetString(bytes);
        }

        public static CommentSection ReadFromMarker(Stream stream)
        {
            var offset = stream.Position;
            var length = stream.ReadShort();

            // Read comment text.
            var bytes = new byte[length];
            var read = stream.Read(bytes, 0, bytes.Length);

            if (read != bytes.Length)
            {
                throw new InvalidOperationException($"Failed to read comment of length {length} at offset {offset}. Read {read} bytes instead.");
            }

            return new CommentSection(offset, length, bytes);
        }
    }
}