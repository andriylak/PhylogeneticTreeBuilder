using PhylogeneticTreeBuilder;
using Xunit;

public class PhyloListTreeTests
{
    // --- helpers ---

    private static string WriteCsv(string content)
    {
        var path = Path.Combine(Path.GetTempPath(), $"matrix_{Guid.NewGuid():N}.csv");
        File.WriteAllText(path, content.Trim() + Environment.NewLine);
        return path;
    }

    private static void AssertBinary(TreeNode node)
    {
        if (node.Children == null || node.Children.Count == 0) return;
        Assert.True(node.Children.Count == 2, "UPGMA internal nodes must have exactly 2 children.");
        foreach (var c in node.Children) AssertBinary(c);
    }

    // --- tests ---

    [Fact]
    public void Test_CreateCsvFile_DistanceMatrixLoadsCorrectlyFromCsv3x3()
    {
        var csv = @"
,A,B,C
A,0,4,6
B,4,0,8
C,6,8,0";
        var path = WriteCsv(csv);

        var m = new DistanceMatrix(path);

        Assert.Equal(4, m.GetDistance(1, 2));
        Assert.Equal(6, m.GetDistance(1, 3));
        Assert.Equal(8, m.GetDistance(2, 3));

        // Accept either property or method style
        var size = typeof(DistanceMatrix).GetProperty("Size") != null
            ? (int)typeof(DistanceMatrix).GetProperty("Size")!.GetValue(m)!
            : (int)typeof(DistanceMatrix).GetMethod("Size")!.Invoke(m, null)!;

        Assert.Equal(3, size);
    }

    private static TEx AssertThrowsWithMessage<TEx>(Action act, params string[] expectedParts)
    where TEx : Exception
    {
        var ex = Assert.Throws<TEx>(act);
        foreach (var part in expectedParts)
            Assert.Contains(part, ex.Message, StringComparison.OrdinalIgnoreCase);
        return ex;
    }

    [Fact]
    public void Test_NoDataRowsCsvFile_ThrowsAnExceptionWithMessage()
    {
        var path = WriteCsv(@"
,A
");
        AssertThrowsWithMessage<InvalidDataException>(
            () => new DistanceMatrix(path),
            "CSV", "header", "data row");
    }

    [Fact]
    public void HeaderTooShort_Message()
    {
        var path = WriteCsv(@"
A
A,0
");
        AssertThrowsWithMessage<InvalidDataException>(
            () => new DistanceMatrix(path),
            "Header", "corner", "label");
    }

    [Fact]
    public void Test_NotSquareMatrix_ThrowsAnExceptionWithMessage()
    {
        var path = WriteCsv(@"
,A,B
A,0,1
");
        AssertThrowsWithMessage<InvalidDataException>(
            () => new DistanceMatrix(path),
            "Header", "labels", "rows", "square");
    }

    [Fact]
    public void Test_EmptyHeaderLabel_ThrowsAnExceptionWithMessage()
    {
        var path = WriteCsv(@"
,A, 
A,0,1
 ,1,0
");
        AssertThrowsWithMessage<InvalidDataException>(
            () => new DistanceMatrix(path),
            "Empty", "column label", "header");
    }

    [Fact]
    public void Test_DuplicateHeaderLabel_ThrowsAnExceptionWithMessage()
    {
        var path = WriteCsv(@"
,A,A
A,0,1
A,1,0
");
        AssertThrowsWithMessage<InvalidDataException>(
            () => new DistanceMatrix(path),
            "Duplicate", "label", "header", "A");
    }

    [Fact]
    public void Test_RowWidthMismatch_ThrowsAnExceptionWithMessage()
    {
        var path = WriteCsv(@"
,A,B
A,0
B,1,0
");
        AssertThrowsWithMessage<InvalidDataException>(
            () => new DistanceMatrix(path),
            "Row 1", "numeric", "expected 2");
    }

    [Fact]
    public void Test_RowLabelMismatch_ThrowsAnExceptionWithMessage()
    {
        var path = WriteCsv(@"
,A,B
X,0,1
B,1,0
");
        AssertThrowsWithMessage<InvalidDataException>(
            () => new DistanceMatrix(path),
            "Row label mismatch", "row 1", "expected 'A'", "found 'X'");
    }

    [Fact]
    public void Test_NonNumericCell_ThrowsAnExceptionWithMessage()
    {
        var path = WriteCsv(@"
,A,B
A,0,abc
B,1,0
");
        AssertThrowsWithMessage<InvalidDataException>(
            () => new DistanceMatrix(path),
            "Invalid number", "row 1", "column 2", "label 'B'", "abc");
    }

    [Fact]
    public void NegativeDistance_Message()
    {
        var path = WriteCsv(@"
,A,B
A,0,-1
B,1,0
");
        AssertThrowsWithMessage<InvalidDataException>(
            () => new DistanceMatrix(path),
            "Negative distance", "(A,B)", "-1");
    }

    [Fact]
    public void Test_DiagonalNotZero_ThrowsAnExceptionWithMessage()
    {
        var path = WriteCsv(@"
,A,B
A,0.1,4
B,4,0
");
        AssertThrowsWithMessage<InvalidDataException>(
            () => new DistanceMatrix(path),
            "Diagonal", "must be zero", "(A,A)", "0.1");
    }

    [Fact]
    public void NotSymmetric_Message()
    {
        var path = WriteCsv(@"
,A,B
A,0,4
B,5,0
");
        AssertThrowsWithMessage<InvalidDataException>(
            () => new DistanceMatrix(path),
            "not symmetric", "between A and B", "4", "5");
    }

    [Fact]
    public void Test_ValidMatrix_DoesNotThrowAnException()
    {
        var path = WriteCsv(@"
,A,B,C
A,0,4,6
B,4,0,8
C,6,8,0
");
        var ex = Record.Exception(() => new DistanceMatrix(path));
        Assert.Null(ex);
    }

    [Fact]
    public void Test_RemoveClusters_RemovesFromDictionary()
    {
        var csv = @"
,A,B,C
A,0,4,6
B,4,0,8
C,6,8,0";
        var path = WriteCsv(csv);
        var m = new DistanceMatrix(path);

        var beforeCount = m.Clusters.Count;

        var (i, j) = m.GetMinDistance();

        Assert.True(m.Clusters.ContainsKey(i));
        Assert.True(m.Clusters.ContainsKey(j));

        m.RemoveClusters(i, j);

        Assert.False(m.Clusters.ContainsKey(i));
        Assert.False(m.Clusters.ContainsKey(j));
        Assert.Equal(beforeCount - 2, m.Clusters.Count);
    }

    [Fact]
    public void Test_DeleteFromDictionary_DeletedSuccessfully()
    {
        var csv = @"
,A,B,C
A,0,4,6
B,4,0,8
C,6,8,0";
        var path = WriteCsv(csv);
        var m = new DistanceMatrix(path);

        var (i, j) = m.GetMinDistance();

        var keyWith_IandJ = m.Distances.Keys.
            Where(k => k.Item1 == i || k.Item2 == i || k.Item1 == j || k.Item2 == j).
            ToList();


        var beforeCount = m.Distances.Count;

        m.RemoveKeysFromDictionary(i, j);
        foreach (var key in keyWith_IandJ)
        {
            Assert.False(m.Distances.ContainsKey(key));
        }
        Assert.Equal(beforeCount - m.Size, m.Distances.Count);

    }
    [Fact]
    public void GetMinDistance_Returns_Smallest_Pair()
    {
        var csv = @"
,A,B,C
A,0,4,6
B,4,0,8
C,6,8,0";
        var path = WriteCsv(csv);
        var m = new DistanceMatrix(path);

        var (i, j) = m.GetMinDistance();
        Assert.True((i == 1 && j == 2) || (i == 2 && j == 1));
    }

    [Fact]
    public void Test_RunUPGNA_CreatesBinaryTree()
    {
        var csv = @"
,A,B,C
A,0,4,6
B,4,0,8
C,6,8,0";
        var path = WriteCsv(csv);
        var m = new DistanceMatrix(path);
        var engine = new UPGMA();

        var root = engine.BuildTree(m);

        Assert.NotNull(root);
        AssertBinary(root);

        var newick = root.ToNewick();
        Assert.StartsWith("(", newick);
        Assert.EndsWith(";", newick);
        Assert.Contains("A:", newick);
        Assert.Contains("B:", newick);
        Assert.Contains("C:", newick);
    }

    [Fact]
    public void Test_UPGMACreateTree_ReturnsCorrectNewickFormat()
    {
        // Build a small list-based tree manually:
        // root
        //  ├─ A (0.4)
        //  └─ U  (0.6)
        //       ├─ B (0.3)
        //       └─ C (0.3)

        var A = new TreeNode(name: "A", children: new List<TreeNode>(), heights: new List<double>());
        var B = new TreeNode(name: "B", children: new List<TreeNode>(), heights: new List<double>());
        var C = new TreeNode(name: "C", children: new List<TreeNode>(), heights: new List<double>());

        var U = new TreeNode(name: "U", children: new List<TreeNode>(), heights: new List<double>());
        U.Children.Add(B);
        U.Heights.Add(0.3);
        U.Children.Add(C);
        U.Heights.Add(0.3);

        var root = new TreeNode(name: "U", children: new List<TreeNode>(), heights: new List<double>());
        root.Children.Add(A);
        root.Heights.Add(0.4);
        root.Children.Add(U);
        root.Heights.Add(0.6);

        var newick = root.ToNewick();

        Assert.Contains("A:0.4", newick);
        Assert.Contains("B:0.3", newick);
        Assert.Contains("C:0.3", newick);
        Assert.Contains("):0.6", newick);
        Assert.EndsWith(";", newick);
    }




    [Fact]
    public void Test_UPGMABuildTree_ReturnsCorrectNewickFormat()
    {
        var csv = @"
,a,b,c,d,e
a,0,17,21,31,23
b,17,0,30,34,21
c,21,30,0,28,39
d,31,34,28,0,43
e,23,21,39,43,0";
        var path = WriteCsv(csv);

        var matrix = new DistanceMatrix(path);
        var engine = new UPGMA();

        var root = engine.BuildTree(matrix);

        var newick = root.ToNewick();

        var ab1 = "(a:8.5,b:8.5)";
        var ab2 = "(b:8.5,a:8.5)";
        var cd1 = "(c:14,d:14)";
        var cd2 = "(d:14,c:14)";

        var eab1 = $"e:11,{ab1}:2.5";
        var eab2 = $"e:11,{ab2}:2.5";

        var top1 = $"(({eab1}):5.5,{cd1}:2.5);";
        var top2 = $"(({eab1}):5.5,{cd2}:2.5);";
        var top3 = $"(({eab2}):5.5,{cd1}:2.5);";
        var top4 = $"(({eab2}):5.5,{cd2}:2.5);";

        var top5 = $"({cd1}:2.5,({eab1}):5.5);";
        var top6 = $"({cd2}:2.5,({eab1}):5.5);";
        var top7 = $"({cd1}:2.5,({eab2}):5.5);";
        var top8 = $"({cd2}:2.5,({eab2}):5.5);";

        var expectedVariants = new HashSet<string>(StringComparer.Ordinal)
        {
            top1, top2, top3, top4, top5, top6, top7, top8
        };

        Assert.True(expectedVariants.Contains(newick));
    }

    [Fact]
    public void Test_NeighbourJoiningBuildTree_ReturnsCorrectNewickFormat()
    {
        var csv = @"
,a,b,c,d,e
a,0,5,9,9,8
b,5,0,10,10,9
c,9,10,0,8,7
d,9,10,8,0,3
e,8,9,7,3,0";
        var path = WriteCsv(csv);

        var matrix = new DistanceMatrix(path);

        var engine = new NeighbourJoining();

        var root = engine.BuildTree(matrix);

        var newick = root.ToNewick();

        Assert.Equal("(d:2,(c:4,(a:2,b:3):3):2,e:1);", newick);
    }

}