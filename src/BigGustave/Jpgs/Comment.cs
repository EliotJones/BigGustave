namespace BigGustave.Jpgs
{
    using System;
    using System.IO;
    using System.Text;

    /// <summary>
    /// A comment in a JPG file.
    /// </summary>
    public class Comment
    {
        /// <summary>
        /// The bytes of the comment.
        /// </summary>
        public byte[] Bytes { get; }

        /// <summary>
        /// The comment bytes interpreted in ASCII.
        /// </summary>
        public string Text { get; }

        private Comment(byte[] bytes)
        {
            Bytes = bytes;
            Text = Encoding.ASCII.GetString(bytes);
        }

        internal static Comment ReadFromMarker(Stream stream)
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

            return new Comment(bytes);
        }
    }
}