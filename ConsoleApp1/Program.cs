namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //var a = new UPGMA();
            //string csv_file = "C:\\Users\\LEGION\\Desktop\\Andriy\\CharlesUniversity\\Programming2\\PhylogeneticTreeBuilder\\ConsoleApp1\\test.txt";
            //DistanceMatrix matrix = new DistanceMatrix(csv_file);
            //var b = a.BuildTree(matrix);

            var a = new NeighborJoining();
            string csv_file = "C:\\Users\\LEGION\\Desktop\\Andriy\\CharlesUniversity\\Programming2\\PhylogeneticTreeBuilder\\ConsoleApp1\\test_NJ.txt";
            DistanceMatrix matrix = new DistanceMatrix(csv_file);
            var b = a.BuildTree(matrix);
        }
    }
}
