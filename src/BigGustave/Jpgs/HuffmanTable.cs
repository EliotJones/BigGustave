namespace BigGustave.Jpgs
{
    using System;

    internal class HuffmanTable
    {
        public Node Root { get; }

        public HuffmanTable(Node root)
        {
            Root = root;
        }

        public static HuffmanTable FromSpecification(HuffmanTableSpecification specification)
        {
            var root = new Node(null);

            var elementIndex = 0;

            for (var lengthIndex = 0; lengthIndex < specification.Lengths.Length; lengthIndex++)
            {
                var length = specification.Lengths[lengthIndex];

                var depth = lengthIndex + 1;

                for (var i = 0; i < length; i++)
                {
                    var element = specification.Elements[elementIndex++];

                    var node = root.GetNextAt(depth);

                    node.SetValue(element);
                }
            }

            return new HuffmanTable(root);
        }

        public byte? Read(BitStream stream)
        {
            var infiniteLoopDetector = 0;
            while (true)
            {
                if (infiniteLoopDetector > 100_000_000)
                {
                    throw new InvalidOperationException();
                }

                var item = Root;
                while (item != null)
                {
                    if (item.Value.HasValue)
                    {
                        return item.Value;
                    }

                    var direction = stream.Read();
                    item = direction == 1 ? item.Right : item.Left;
                }

                infiniteLoopDetector++;
            }
        }

        public class Node
        {
            private bool isFull;

            private readonly int level;

            private readonly Node parent;

            public bool IsRoot => parent == null;

            public bool IsLeaf => Value.HasValue;

            public Node Left { get; private set; }

            public Node Right { get; private set; }

            public byte? Value { get; private set; }

            public Node(Node parent)
            {
                this.parent = parent;
                level = parent == null ? 0 : this.parent.level + 1;
            }

            public Node AddLeft()
            {
                Left = new Node(this);
                return Left;
            }

            public Node AddRight()
            {
                Right = new Node(this);
                return Right;
            }

            public void SetValue(byte value)
            {
                if (Value.HasValue)
                {
                    throw new InvalidOperationException("Cannot overwrite a leaf value.");
                }

                if (Left != null || Right != null)
                {
                    throw new InvalidOperationException("Cannot set value on non-leaf node.");
                }

                isFull = true;
                Value = value;

                parent.ChildOccupied();
            }

            private void ChildOccupied()
            {
                if (Left?.isFull == true && Right?.isFull == true)
                {
                    isFull = true;
                    parent?.ChildOccupied();
                }
            }

            public Node GetNextAt(int depth)
            {
                if (depth < level || IsLeaf || isFull)
                {
                    return null;
                }

                if (depth == level)
                {
                    // not a leaf node.
                    if (Left != null || Right != null)
                    {
                        return null;
                    }

                    return this;
                }

                if (Left == null)
                {
                    AddLeft();
                }

                var result = Left.GetNextAt(depth);

                if (result != null)
                {
                    return result;
                }

                if (Right == null)
                {
                    AddRight();
                }

                result = Right.GetNextAt(depth);

                return result;
            }

            public override string ToString()
            {
                if (Value.HasValue)
                {
                    return $"{Value}";
                }

                return $"[{Left}, {Right}]";
            }
        }
    }
}