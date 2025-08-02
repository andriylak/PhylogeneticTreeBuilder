namespace ConsoleApp1
{
    internal class UPGMA
    {
        public UPGMA() { }

        public TreeNode BuildTree(DistanceMatrix matrix)
        {
            int next_id = matrix.Size + 1;
            while (matrix.Size > 1)
            {
                var (i, j) = matrix.GetMinDistance();
                var clusterA = matrix.Clusters[i];
                var clusterB = matrix.Clusters[j];

                // Compute new height
                double distance = matrix.GetDistance(i, j);
                double heightA = distance / 2.0 - clusterA.Node.GetMaxHeight;
                double heightB = distance / 2.0 - clusterB.Node.GetMaxHeight;

                // Create new TreeNode
                List<TreeNode> children = new List<TreeNode>() { clusterA.Node, clusterB.Node };
                List<double> heights = new List<double>() { heightA, heightB };
                TreeNode newNode = new TreeNode(children, heights);

                // Create merged cluster
                var merged = Cluster.Merge(clusterA, clusterB, next_id, newNode);

                // Update matrix
                matrix.UpdateDistances(next_id, clusterA, clusterB);
                matrix.RemoveClusters(i, j);
                matrix.AddCluster(next_id, merged);
                next_id++;
            }

            return matrix.Clusters.Values.First().Node;
        }
    }

    internal class NeighborJoining
    {
        public NeighborJoining()
        {

        }
        public TreeNode BuildTree(DistanceMatrix matrix)
        {
            int next_id = matrix.Size + 1;
            while (matrix.Size > 2)
            {
                (int i_min, int j_min) = matrix.CalculateQMatrix();
                List<double> heights = matrix.ComputeBranchLengths(i_min, j_min);

                Cluster cluster_i = matrix.Clusters[i_min];
                Cluster cluster_j = matrix.Clusters[j_min];
                List<TreeNode> children = new List<TreeNode>() { cluster_i.Node, cluster_j.Node };

                TreeNode newNode = new TreeNode(children, heights);

                var merged = Cluster.Merge(cluster_i, cluster_j, next_id, newNode);

                // Update matrix
                matrix.UpdateDistances(next_id, cluster_i, cluster_j);
                matrix.RemoveClusters(i_min, j_min);
                matrix.AddCluster(next_id, merged);
                next_id++;
            }
            return matrix.Clusters.Values.First().Node;
        }
    }
}

