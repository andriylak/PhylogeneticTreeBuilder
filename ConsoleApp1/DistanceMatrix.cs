using ConsoleApp1;
using System.Globalization;

public class DistanceMatrix
{
    public Dictionary<(int, int), double> Distances { get; private set; }
    public Dictionary<int, Cluster> Clusters { get; private set; }
    private PriorityQueue<(int, int), double> Queue;
    public int Size() => Clusters.Count();
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

    public void UpdateDistances(int newId, Cluster a, Cluster b)
    {
        var copy = new Dictionary<(int, int), double>(Distances);

        var toRemove = Distances.Keys
            .Where(k => k.Item1 == a.Id || k.Item2 == a.Id || k.Item1 == b.Id || k.Item2 == b.Id)
            .ToList();

        foreach (var key in toRemove)
        {
            Distances.Remove(key);
        }

        foreach (var (k, cluster) in Clusters)
        {
            if (k == a.Id || k == b.Id) ;

            var keyA = k < a.Id ? (k, a.Id) : (a.Id, k);
            var keyB = k < b.Id ? (k, b.Id) : (b.Id, k);

            double distToA = copy.ContainsKey(keyA) ? copy[keyA] : 0.0;
            double distToB = copy.ContainsKey(keyB) ? copy[keyB] : 0.0;

            double newDistance = (a.Size * distToA + b.Size * distToB) / (a.Size + b.Size);

            var newKey = k < newId ? (k, newId) : (newId, k);
            Distances[newKey] = newDistance;
            Queue.Enqueue(newKey, newDistance);
        }
    }
}