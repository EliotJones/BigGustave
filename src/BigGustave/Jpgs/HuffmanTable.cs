namespace BigGustave.Jpgs
{
    using System;

    internal class HuffmanTable
    {
        public BinaryTree Tree { get; }

        public HuffmanTable(BinaryTree tree)
        {
            Tree = tree;
        }

        public static HuffmanTable FromSpecification(HuffmanTableSpecification specification)
        {
            if (specification.Lengths.Length != 16)
            {
                throw new ArgumentException($"Lengths array must contain 16 items, got: {specification.Lengths.Length}.", nameof(specification));
            }

            //var root = new Node
            //{
            //    Left = new Node(),
            //    Right = new Node()
            //};

            //var leftMost = root.Left;

            //for (var i = 0; i < specification.Lengths.Length; i++)
            //{
            //    var count = specification.Lengths[i];

            //    if (count == 0)
            //    {
            //        var current = leftMost;

            //        while (current != null)
            //        {
            //            current.Left = new Node();
            //            current.Right = new Node();
            //        }

            //    }
            //}

            var tree = new BinaryTree();

            var elementIndex = 0;
            for (var i = 0; i < specification.Lengths.Length; i++)
            {
                var length = specification.Lengths[i];
                for (var j = 0; j < length; j++)
                {
                    tree.Add(specification.Elements[elementIndex], i);
                    elementIndex++;
                }
            }

            return new HuffmanTable(tree);
        }

        public byte? GetCode(BitStream str)
        {
            byte? result;
            while (true)
            {
                result = Find(str);

                if (result.HasValue)
                {
                    return result;
                }
            }

            return null;
        }

        private byte? Find(BitStream str)
        {
            var item = Tree.Root;
            while (item != null)
            {
                if (item.Value.HasValue)
                {
                    return item.Value;
                }

                var direction = str.Read();
                item = direction ? item.Right : item.Left;
            }

            return null;
        }

        public class Node
        {
            public Node Left { get; set; }

            public Node Right { get; set; }

            public byte? Value { get; }

            public Node()
            {
            }

            public Node(byte b)
            {
                Value = b;
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

        public class BinaryTree
        {
            public Node Root { get; } = new Node();

            public void Add(byte value, int index)
            {
                AddToNodeRecursive(Root, value, index);
            }

            private bool AddToNodeRecursive(Node current, byte value, int index)
            {
                if (current.Value.HasValue)
                {
                    return false;
                }

                if (index == 0)
                {
                    if (current.Left == null)
                    {
                        current.Left = new Node(value);
                        return true;
                    }

                    if (current.Right == null)
                    {
                        current.Right = new Node(value);
                        return true;
                    }

                    return false;
                }

                for (var i = 0; i < 2; i++)
                {
                    var side = i == 0 ? current.Left : current.Right;
                    if (side == null)
                    {
                        side = new Node();
                        if (i == 0)
                        {
                            current.Left = side;
                        }
                        else
                        {
                            current.Right = side;
                        }
                    }

                    if (AddToNodeRecursive(side, value, index - 1))
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}