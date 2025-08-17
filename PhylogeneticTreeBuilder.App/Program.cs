using PhylogeneticTreeBuilder.App;

namespace PhylogeneticTreeBuilde.App
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}
