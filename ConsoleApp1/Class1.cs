using System.Globalization;

public class DistanceMatrix
{
    public List<string> Labels { get; private set; }
    public double[,] Distances { get; private set; }
    private PriorityQueue<(int, int), double> Queue;

    public DistanceMatrix(string csvPath)
    {
        Labels = new List<string>();
        Queue = new PriorityQueue<(int, int), double>();

        var lines = File.ReadAllLines(csvPath);
        int n = lines.Length;

        // Parse the header to get the labels
        var headers = lines[0].Split(',').Skip(1).ToArray();
        Labels.AddRange(headers);

        Distances = new double[n - 1, n - 1];

        for (int i = 1; i < n; i++)
        {
            var parts = lines[i].Split(',');

            string rowLabel = parts[0];
            string expectedLabel = Labels[i - 1];

            if (rowLabel != expectedLabel)
            {
                throw new InvalidDataException(
                    $"Row label mismatch at row {i}: expected '{expectedLabel}', found '{rowLabel}'."
                );
            }

            for (int j = 1; j < parts.Length; j++)
            {
                double value = double.Parse(parts[j], CultureInfo.InvariantCulture);
                Distances[i - 1, j - 1] = value;

                if (i > j)
                {
                    Queue.Enqueue((i - 1, j - 1), value);
                }
            }
        }
    }

    public double GetDistance(int i, int j) => Distances[i, j];

    public (int, int) GetMinDistance()
    {
        return Queue.Dequeue();
    }
}
