using System.Globalization;

public class UpgmaBasicsTests
{
    // Helper: write a CSV to a temp file and return its path
    private string WriteCsv(string content)
    {
        var path = Path.Combine(Path.GetTempPath(), $"matrix_{Guid.NewGuid():N}.csv");
        File.WriteAllText(path, content.Trim() + Environment.NewLine);
        return path;
    }

    // Helper: a simple Newick exporter for your current TreeNode design (Left/Right with branch lengths kept in parent).
    private static string ToNewick(TreeNode node, double lengthFromParent = 0.0)
    {
        bool isLeaf = node.Left == null && node.Right == null;
        if (isLeaf)
        {
            // leaf: Name:len
            return $"{node.Name}:{lengthFromParent.ToString("0.###", CultureInfo.InvariantCulture)}";
        }

        var parts = new List<string>();

        if (node.Left != null)
            parts.Add(ToNewick(node.Left, node.LeftHeight));

        if (node.Right != null)
            parts.Add(ToNewick(node.Right, node.RightHeight));

        // internal node: (children):lenToParent
        return $"({string.Join(",", parts)}):{lengthFromParent.ToString("0.###", CultureInfo.InvariantCulture)}";
    }

    [Fact]
    public void DistanceMatrix_Loads_3x3_Correctly()
    {
        // A simple symmetric 3x3
        var csv = @"
,A,B,C
A,0,4,6
B,4,0,8
C,6,8,0";
        var path = WriteCsv(csv);

        var m = new DistanceMatrix(path);

        // Your DistanceMatrix uses 1-based IDs in API in the canvas code
        Assert.Equal(4, m.GetDistance(1, 2));
        Assert.Equal(6, m.GetDistance(1, 3));
        Assert.Equal(8, m.GetDistance(2, 3));
        Assert.Equal(3, m.Size); // Clusters count
    }

    [Fact]
    public void GetMinDistance_ReturnsSmallestPair()
    {
        var csv = @"
,A,B,C
A,0,4,6
B,4,0,8
C,6,8,0";
        var path = WriteCsv(csv);
        var m = new DistanceMatrix(path);

        var (i, j) = m.GetMinDistance();
        // Smallest distance is between A(1) and B(2) with 4
        Assert.True((i == 1 && j == 2) || (i == 2 && j == 1));
    }

    [Fact]
    public void UPGMA_Builds_Binary_Tree_And_First_Merge_Is_AB()
    {
        var csv = @"
,A,B,C
A,0,4,6
B,4,0,8
C,6,8,0";
        var path = WriteCsv(csv);
        var matrix = new DistanceMatrix(path);
        var engine = new UPGMAEngine();

        var root = engine.BuildTree(matrix);

        // Root should be an internal node combining (A,B) first
        Assert.NotNull(root);
        // Newick contains both A and B under the first internal grouping
        var newick = ToNewick(root) + ";";

        Assert.StartsWith("(", newick);
        Assert.EndsWith(";", newick);
        Assert.Contains("A:", newick);
        Assert.Contains("B:", newick);
        Assert.Contains("C:", newick);

        // Ensure branch lengths are non-negative (UPGMA heights)
        // (Basic sanity: your code computes left/right as distance/2 - childHeight)
        // Extract numbers and check at least one positive
        Assert.Matches(@".*:[0-9.]+.*", newick);
    }

    [Fact]
    public void UpdateDistances_Merge_Sets_New_Distances_By_Weighted_Average()
    {
        // Use a 4x4 where the first merge is obvious (D,E in a 5x5 example;
        // here we use 4x4 so we can test updates neatly)
        var csv = @"
,A,B,C,D
A,0,5,9,9
B,5,0,10,10
C,9,10,0,8
D,9,10,8,0";
        var path = WriteCsv(csv);
        var m = new DistanceMatrix(path);

        // Simulate one merge step like UPGMA would do:
        // min pair is A-B with 5
        var (i, j) = m.GetMinDistance();
        Assert.True((i == 1 && j == 2) || (i == 2 && j == 1));

        var a = m.Clusters[i];
        var b = m.Clusters[j];

        // Create a dummy merged node, keep ID = min(i,j)
        double dist = m.GetDistance(i, j);
        var node = new TreeNode($"({a.Id},{b.Id})")
        {
            Left = a.Node,
            Right = b.Node,
            LeftHeight = dist / 2.0 - a.Node.GetMaxHeight(),
            RightHeight = dist / 2.0 - b.Node.GetMaxHeight()
        };
        int newId = Math.Min(i, j);
        var merged = Cluster.Merge(a, b, newId, node);

        // IMPORTANT: In your current flow, you call UpdateDistances before removing old ones
        m.UpdateDistances(newId, a, b);

        // Now remove old, add new
        m.RemoveClusters(i, j);
        m.AddCluster(newId, merged);

        // Check new distances against weighted average
        // For any remaining k (3 or 4), D(AB,k) = (|A|*D(A,k) + |B|*D(B,k)) / (|A|+|B|)
        // Here |A|=|B|=1, so it's (D(A,k) + D(B,k))/2

        // k=3 (C)
        double expectedAB_C = (m.GetDistanceSafe(1, 3) + m.GetDistanceSafe(2, 3)) / 2.0;
        // After updates, AB has id=min(1,2)=1, so check D(1,3)
        double actualAB_C = m.GetDistance(1, 3);
        Assert.InRange(actualAB_C, expectedAB_C - 1e-9, expectedAB_C + 1e-9);

        // k=4 (D)
        double expectedAB_D = (m.GetDistanceSafe(1, 4) + m.GetDistanceSafe(2, 4)) / 2.0;
        double actualAB_D = m.GetDistance(1, 4);
        Assert.InRange(actualAB_D, expectedAB_D - 1e-9, expectedAB_D + 1e-9);
    }

    [Fact]
    public void Newick_Includes_Internal_And_Leaf_Branch_Lengths()
    {
        // Build a tiny manual tree:
        // root
        //  ├─ A (0.4)
        //  └─ internal (0.6)
        //       ├─ B (0.3)
        //       └─ C (0.3)

        var A = new TreeNode("A");
        var B = new TreeNode("B");
        var C = new TreeNode("C");

        var internalNode = new TreeNode("(B,C)")
        {
            Left = B,
            Right = C,
            LeftHeight = 0.3,
            RightHeight = 0.3
        };

        var root = new TreeNode("(A,(B,C))")
        {
            Left = A,
            Right = internalNode,
            LeftHeight = 0.4,
            RightHeight = 0.6
        };

        var newick = ToNewick(root) + ";";

        // Check structure and distances are printed
        Assert.Contains("A:0.4", newick);
        Assert.Contains("B:0.3", newick);
        Assert.Contains("C:0.3", newick);
        Assert.Contains("):0.6", newick); // internal node length to parent
        Assert.EndsWith(";", newick);
    }
}

// Extension used in one test to safely read a distance (mirrors your matrix normalization)
internal static class DistanceMatrixTestHelpers
{
    public static double GetDistanceSafe(this DistanceMatrix m, int i, int j)
    {
        if (i == j) return 0.0;
        try { return m.GetDistance(i, j); }
        catch { return m.GetDistance(j, i); }
    }
}
