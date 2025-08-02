using ConsoleApp1;
using System.Globalization;

public class DistanceMatrix
{
    public Dictionary<(int, int), double> Distances { get; private set; }
    public Dictionary<int, Cluster> Clusters { get; private set; }
    private PriorityQueue<(int, int), double> Queue;
    public int Size => Clusters.Count();
    public Dictionary<(int, int), double> QMatrix { get; private set; }
    public DistanceMatrix(string csvPath)
    {
        List<string> labels = new List<string>();
        Queue = new PriorityQueue<(int, int), double>();
        Distances = new Dictionary<(int, int), double>();
        Clusters = new Dictionary<int, Cluster>();

        var lines = File.ReadAllLines(csvPath);
        int size = lines.Length - 1;

        // Parse the header to get the labels
        var headers = lines[0].Split(',').Skip(1).ToArray();
        labels.AddRange(headers);

        for (int i = 1; i <= size; i++)
        {
            var parts = lines[i].Split(',');

            string rowLabel = parts[0];
            string expectedLabel = labels[i - 1];

            ValidateRowAndColumn(rowLabel, expectedLabel, i);
            TreeNode node = new TreeNode(rowLabel);
            Cluster cluster = new Cluster(i, node);
            AddCluster(i, cluster);

            for (int j = 1; j < parts.Length; j++)
            {
                if (i < j)
                {
                    double value = double.Parse(parts[j], CultureInfo.InvariantCulture);
                    Distances.Add((i, j), value);
                    Queue.Enqueue((i, j), value);
                }
            }
        }
    }

    private void ValidateRowAndColumn(string expected, string actual, int rowIndex)
    {
        if (expected != actual)
        {
            throw new InvalidDataException(
                $"Row label mismatch at row {rowIndex}: expected '{expected}', found '{actual}'.");
        }
    }

    public double GetDistance(int i, int j)
    {
        var key = i < j ? (i, j) : (j, i);
        return Distances[key];
    }

    public (int, int) GetMinDistance()
    {
        while (Queue.Count > 0)
        {
            var (i, j) = Queue.Dequeue();
            if (Clusters.ContainsKey(i) && Clusters.ContainsKey(j))
                return (i, j);
        }
        throw new InvalidOperationException("No valid minimum distance found.");
    }

    public void RemoveClusters(int i, int j)
    {
        Clusters.Remove(i);
        Clusters.Remove(j);

        var keysToRemove = Distances.Keys
            .Where(k => k.Item1 == i || k.Item2 == i || k.Item1 == j || k.Item2 == j)
            .ToList();

        foreach (var key in keysToRemove)
        {
            Distances.Remove(key);
        }
    }

    public void AddCluster(int id, Cluster cluster)
    {
        Clusters[id] = cluster;
    }

    private void RemoveKeysFromDictionary(int i, int j)
    {
        var toRemove = Distances.Keys
    .Where(k => k.Item1 == i || k.Item2 == i || k.Item1 == j || k.Item2 == j)
    .ToList();

        foreach (var key in toRemove)
        {
            Distances.Remove(key);
        }
    }
    public void UpdateDistances(int newId, Cluster a, Cluster b)
    {
        foreach (var (k, cluster) in Clusters)
        {
            //if (k == a.Id || k == b.Id) ;

            //var keyA = k < a.Id ? (k, a.Id) : (a.Id, k);
            //var keyB = k < b.Id ? (k, b.Id) : (b.Id, k);
            if (k != a.Id && k != b.Id)
            {
                double distToA = GetDistance(k, a.Id);
                double distToB = GetDistance(k, b.Id);

                double newDistance = (a.Size * distToA + b.Size * distToB) / (a.Size + b.Size);

                var newKey = (k, newId);
                Distances[newKey] = newDistance;
                Queue.Enqueue(newKey, newDistance);
            }
        }
        RemoveKeysFromDictionary(a.Id, b.Id);
    }

    public (int, int) CalculateQMatrix()
    {
        int sizeOfMatrix = Size;
        Dictionary<(int, int), double> QMatrix = new Dictionary<(int, int), double>();
        double min = double.MaxValue;
        (int i_min, int j_min) = (-1, -1);
        foreach (var (i, cluster1) in Clusters)
        {
            foreach (var (j, cluster2) in Clusters)
            {
                if (i < j)
                {
                    double total1 = CalculateTheTotalDistance(i);
                    double total2 = CalculateTheTotalDistance(j);
                    QMatrix[(i, j)] = (sizeOfMatrix - 2) * GetDistance(i, j) - total1 - total2;
                    if (QMatrix[(i, j)] < min)
                    {
                        min = QMatrix[(i, j)];
                        (i_min, j_min) = (i, j);
                    }
                    Queue.Enqueue((i, j), QMatrix[(i, j)]);
                }
            }
        }
        return (i_min, j_min);
    }

    private double CalculateTheTotalDistance(int i)
    {
        double sum = 0;
        foreach (var (k, cluster) in Clusters)
        {
            if (i != k) sum += GetDistance(i, k);
        }
        return sum;
    }

    public List<double> ComputeBranchLengths(int i, int j)
    {
        double total_i = CalculateTheTotalDistance(i);
        double total_j = CalculateTheTotalDistance(j);
        double distance = GetDistance(i, j);
        double branchLength_i = 1 / 2 * distance + (total_i - total_j) / (2 * Size - 4);
        double branchLength_j = distance - branchLength_i;
        List<double> length = new List<double>() { branchLength_i, branchLength_j };
        return length;
    }

    public void UpdateDistanceMatrix_NJ(int new_id, Cluster i, Cluster j)
    {
        foreach (var (k, cluster) in Clusters)
        {
            if (k != i.Id && k != j.Id)
            {
                double distance = (GetDistance(i.Id, k) + GetDistance(j.Id, k) - GetDistance(i.Id, j.Id) / 2);
                var new_key = (k, new_id);
                Distances[new_key] = distance;
            }
        }
        RemoveKeysFromDictionary(i.Id, j.Id);
    }
}