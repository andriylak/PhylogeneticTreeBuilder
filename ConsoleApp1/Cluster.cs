namespace ConsoleApp1
{
    public class Cluster
    {
        public int Id { get; set; }
        public TreeNode Node { get; set; }
        public double Height => Node.GetMaxHeight();

        public int Size { get; private set; }
        public List<(int, int)> MemberIndices { get; set; }

        public Cluster(int id, TreeNode node, double height, List<(int, int)> indices)
        {
            Id = id;
            Node = node;
            MemberIndices = indices;
        }

        public Cluster(int id, TreeNode node)
        {
            Id = id;
            Node = node;
            Size = 1;
        }

        public static Cluster Merge(Cluster a, Cluster b, int newId, TreeNode node)
        {
            return new Cluster(newId, node)
            {
                Size = a.Size + b.Size
            };
        }
    }

}

