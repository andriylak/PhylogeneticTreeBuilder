namespace ConsoleApp1
{
    internal class UPGMA
    {
        public UPGMA() { }

        public TreeNode BuildTree(DistanceMatrix matrix)
        {
            int next_id = matrix.Size() + 1;
            while (matrix.Size() > 1)
            {
                var (i, j) = matrix.GetMinDistance();
                var clusterA = matrix.Clusters[i];
                var clusterB = matrix.Clusters[j];

                // Compute new height
                double distance = matrix.GetDistance(i, j);
                double leftHeight = distance / 2.0 - clusterA.Node.GetMaxHeight();
                double rightHeight = distance / 2.0 - clusterB.Node.GetMaxHeight();

                // Create new TreeNode
                var newNode = new TreeNode(clusterA.Node, clusterB.Node, leftHeight, rightHeight);

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
}

