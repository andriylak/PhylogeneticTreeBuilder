namespace ConsoleApp1
{
    public class TreeNode
    {
        public string? Name { get; set; }

        public List<TreeNode> Children { get; set; }
        public List<double> Heights { get; set; }
        public double MaxHeight { get; private set; }

        // Constructor for leaf nodes (single specimen)
        public TreeNode(string name)
        {
            Name = name;
            Children = new List<TreeNode>();
            Heights = new List<double>();
            MaxHeight = 0;
        }

        // Constructor for internal (merged) nodes
        public TreeNode(List<TreeNode> children, List<double> heights)
        {
            Name = "o";
            Children = new List<TreeNode>();
            Heights = new List<double>();
            for (int i = 0; i < children.Count; i++)
            {
                AddChild(children[i], heights[i]);
            }
            MaxHeight = CalculateMaxHeight();

        }

        private void AddChild(TreeNode child, double height)
        {
            Children.Add(child);
            Heights.Add(height);
        }
        private double CalculateMaxHeight()
        {
            double max = double.MinValue;
            for (int i = 0; i < Children.Count; i++)
            {
                if (Children[i].MaxHeight + Heights[i] > max)
                {
                    max = Children[i].MaxHeight + Heights[i];
                }
            }
            return max;
        }
        public bool IsLeaf => Children.Count == 0;

        public double GetMaxHeight => MaxHeight;

        public string ToNewick(double? lengthFromParent = null, bool first = false)
        {
            if (this.Children.Count == 0) // Leaf
            {
                return $"{this.Name}:{lengthFromParent:0.###}";
            }
            else
            {
                var parts = new List<string>();
                for (int i = 0; i < this.Children.Count; i++)
                {
                    var child = this.Children[i];
                    var len = this.Heights[i];
                    parts.Add(child.ToNewick(len));
                }
                if (first) return $"({string.Join(",", parts)});";
                else return $"({string.Join(",", parts)}):{lengthFromParent:0.###}";
            }

        }
    }
}
