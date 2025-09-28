namespace PhylogeneticTreeBuilder
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
        public TreeNode(List<TreeNode> children, List<double> heights, string name = "o")
        {
            Name = name;
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

        public string ToNewick()
        {
            var s = ToNewickInternal(lengthFromParent: null);
            return s + ";";
        }

        private string ToNewickInternal(double? lengthFromParent)
        {
            if (Children == null || Heights == null)
                throw new InvalidOperationException("Children/Heights not initialized.");
            if (Children.Count != Heights.Count)
                throw new InvalidOperationException("Children and Heights must have same length.");

            //leaf
            if (this.IsLeaf)
                return $"{Sanitize(Name)}:{Format(lengthFromParent)}";

            //internal
            var parts = new List<string>(Children.Count);
            for (int i = 0; i < Children.Count; i++)
                parts.Add(Children[i].ToNewickInternal(Heights[i]));

            var inside = string.Join(",", parts);

            //doesn't branch length if this is the topmost call
            return lengthFromParent is null
                ? $"({inside})"
                : $"({inside}):{Format(lengthFromParent)}";

            //local helpers 
            static string Format(double? x) =>
                (x ?? 0).ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);

            static string Sanitize(string name)
            {
                if (string.IsNullOrEmpty(name)) return "";
                return name.Any(ch => " \t():,;".Contains(ch))
                    ? name.Replace(' ', '_')
                    : name;
            }
        }
    }
}
