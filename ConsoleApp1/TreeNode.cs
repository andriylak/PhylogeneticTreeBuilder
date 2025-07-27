namespace ConsoleApp1
{
    public class TreeNode
    {
        public string? Name { get; set; }
        public TreeNode? LeftChild { get; set; }
        public TreeNode? RightChild { get; set; }
        public double? HeightRight { get; set; }
        public double? HeightLeft { get; set; }

        // Constructor for leaf nodes (single specimen)
        public TreeNode(string name)
        {
            Name = name;
            HeightRight = 0;
            HeightLeft = 0;
            LeftChild = null;
            RightChild = null;
        }

        // Constructor for internal (merged) nodes
        public TreeNode(TreeNode leftChild, TreeNode rightChild, double heightLeft, double heightRight)
        {
            LeftChild = leftChild;
            RightChild = rightChild;
            HeightRight = heightRight;
            HeightLeft = heightLeft;
            Name = "o";
        }

        public double GetMaxHeight()
        {
            return Math.Max((double)(LeftChild != null ? LeftChild.GetMaxHeight() + HeightLeft : 0),
                            (double)(RightChild != null ? RightChild.GetMaxHeight() + HeightRight : 0));
        }
        public bool IsLeaf => LeftChild == null && RightChild == null;

    }
}
