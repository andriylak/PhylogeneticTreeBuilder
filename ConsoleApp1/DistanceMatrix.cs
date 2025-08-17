using PhylogeneticTreeBuilder;
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
        Queue = new PriorityQueue<(int, int), double>();
        Distances = new Dictionary<(int, int), double>();
        Clusters = new Dictionary<int, Cluster>();

        var lines = File.ReadAllLines(csvPath);
        if (lines.Length < 2)
            throw new InvalidDataException("CSV must contain a header and at least one data row.");

        // --- parse header ---
        var headerCells = lines[0].Split(',');
        if (headerCells.Length < 2)
            throw new InvalidDataException("Header must have an empty corner cell and at least one label.");

        // labels are header cells excluding the first corner cell
        var labels = headerCells.Skip(1).Select(s => s.Trim()).ToArray();
        int n = labels.Length;
        if (n != lines.Length - 1)
            throw new InvalidDataException(
                $"Header has {n} labels, but there are {lines.Length - 1} data rows. Matrix must be square.");

        // Use a temporary matrix to validate first; we’ll commit to Distances/Queue only if all checks pass
        var tmp = new double[n, n];

        // For label uniqueness check
        var labelSet = new HashSet<string>(StringComparer.Ordinal);
        foreach (var lab in labels)
        {
            if (string.IsNullOrWhiteSpace(lab))
                throw new InvalidDataException("Empty column label found in header.");
            if (!labelSet.Add(lab))
                throw new InvalidDataException($"Duplicate label in header: '{lab}'.");
        }

        // --- parse & validate rows ---
        const double TOLERANCE = 1e-9;

        for (int i = 1; i <= n; i++)
        {
            var parts = lines[i].Split(',');
            if (parts.Length != n + 1)
                throw new InvalidDataException(
                    $"Row {i} has {parts.Length - 1} numeric cells, expected {n}. Check for missing/extra commas.");

            // row label must match header label at same position
            string rowLabel = parts[0].Trim();
            string expectedLabel = labels[i - 1];
            if (!string.Equals(rowLabel, expectedLabel, StringComparison.Ordinal))
                throw new InvalidDataException(
                    $"Row label mismatch at row {i}: expected '{expectedLabel}', found '{rowLabel}'.");

            for (int j = 1; j <= n; j++)
            {
                string token = parts[j].Trim();
                if (!double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out double v))
                    throw new InvalidDataException(
                        $"Invalid number at row {i}, column {j} (label '{labels[j - 1]}'): '{token}'.");

                if (v < 0)
                    throw new InvalidDataException(
                        $"Negative distance at ({rowLabel},{labels[j - 1]}): {v}.");

                tmp[i - 1, j - 1] = v;
            }
        }

        // --- diagonal & symmetry checks ---
        for (int r = 0; r < n; r++)
        {
            if (Math.Abs(tmp[r, r]) > TOLERANCE)
                throw new InvalidDataException(
                    $"Diagonal must be zero at ({labels[r]},{labels[r]}), found {tmp[r, r].ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)}.");

            for (int c = r + 1; c < n; c++)
            {
                double a = tmp[r, c];
                double b = tmp[c, r];

                if (Math.Abs(a - b) > TOLERANCE)
                    throw new InvalidDataException(
                        $"Matrix is not symmetric between {labels[r]} and {labels[c]}: {a} vs {b}.");
            }
        }

        // --- commit: create clusters and fill Distances / Queue (store only i<j) ---
        for (int i = 0; i < n; i++)
        {
            int id = i + 1;
            var node = new TreeNode(labels[i]);
            var cluster = new Cluster(id, node);
            AddCluster(id, cluster);
        }

        for (int i = 0; i < n; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                double value = tmp[i, j];
                var key = (i + 1, j + 1);
                Distances.Add(key, value);
                Queue.Enqueue(key, value);
            }
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
    }

    public void AddCluster(int id, Cluster cluster)
    {
        Clusters[id] = cluster;
    }

    public void RemoveKeysFromDictionary(int i, int j)
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
        QMatrix = new Dictionary<(int, int), double>();
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
        double branchLength_i = 0.5 * distance + (total_i - total_j) / (2 * Size - 4);
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
                double distance = (GetDistance(i.Id, k) + GetDistance(j.Id, k) - GetDistance(i.Id, j.Id)) / 2;
                var new_key = (k, new_id);
                Distances[new_key] = distance;
            }
        }
        RemoveKeysFromDictionary(i.Id, j.Id);
    }
}