using ScottPlot.Plottable;
using System.Windows;

namespace NeuroBox
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Windows.Threading.DispatcherTimer dispatcherTimer;

        private ScatterPlotList signalPlot;

        public string SelectionCondition { get; set; } = @"
// You may re-use any of those code to change the selection or write your own

// Use this to select only those which are on the 1/3 left part
// return critter.X < 30;

// Use this to select only those which are on the 1/3 right part
// return critter.X > 70;

// Use this to select only those which are on the 1/3 top part
// return critter.Y < 30;

// Use this to select only those which are on the 1/3 bottom part
// return critter.Y > 70;

// Use this to select only those which are near the corners
// Works better without DNA mixing
// return (critter.X < 20 && critter.Y < 20) || (critter.X > 80 && critter.Y < 20) || (critter.X < 20 && critter.Y > 80) || (critter.X > 80 && critter.Y > 80);

// Use this to select only those which are in center
var a = 50 - critter.X;
var b = 50 - critter.Y;
var d=Math.Sqrt(b * b + a * a);
return (d < 20);";

        public string SpawnCoordinate { get; set; } = @"
// You may re-use any of those code to change the selection or write your own

// Random position within a circle of 20 starting at 20,20 
// var r = 20 * Math.Sqrt(rnd.NextDouble());
// var theta = rnd.NextDouble() * 2.0 * Math.PI;
// return ((int)(20 + r * Math.Cos(theta)),(int)(20 + r * Math.Sin(theta)));

// Random position within the world grid
return (rnd.Next(worldGrid.GridSize-1),rnd.Next(worldGrid.GridSize-1));
";

        public WorldGrid WorldGrid => worldGrid;

        public MainWindow()
        {
            InitializeComponent();
            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(500);
            dispatcherTimer.Start();
            worldGrid.GenerationSurvivalEvent += WorldGrid_GenerationSurvivalEvent;
            signalPlot = new ScottPlot.Plottable.ScatterPlotList();
            signalPlot.Label = "Survival Rate";
            signalPlot.Color = System.Drawing.Color.Red;
            survivalPlot.Plot.Legend();
            survivalPlot.Plot.Add(signalPlot);
            survivalPlot.Refresh();
            simulationSettings.MainWindow = this;

            // Pre-run the code parsing to speed up on run
            var w = CSParsing.LoadAndExecute(@"using NeuroBox; using System; public static class EvalClass { public static bool EvalFunction(Critter critter) { return true; } } ");
            var assembly = ((CSParsing.SimpleUnloadableAssemblyLoadContext)w.Target).Assemblies.First();
            var method = assembly.GetType("EvalClass").GetMethod("EvalFunction", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            method.Invoke(null, new object[] { null });
        }

        private void WorldGrid_GenerationSurvivalEvent(object? sender, double survivalRate)
        {
            signalPlot.Add(worldGrid.Generation, survivalRate * 100);
            survivalPlot.Plot.SetAxisLimits(signalPlot.GetAxisLimits());
            survivalPlot.Refresh();
        }

        private void dispatcherTimer_Tick(object? sender, EventArgs e)
        {
            simulationSettings.Survival.Text = (worldGrid.SurvivalRate * 100).ToString("F2") + "%";
            statusSurvival.Text = "Survival: " + simulationSettings.Survival.Text;
            simulationSettings.Generation.Text = worldGrid.Generation.ToString();
            simulationSettings.TimePerGeneration.Text = worldGrid.TimePerGeneration.ToString(@"hh\:mm\:ss\.ff");
            statusTimePerGeneration.Text = "Time: " + simulationSettings.TimePerGeneration.Text;
            simulationSettings.GeneticSimilarities.Text = (worldGrid.DNASimilarity * 100).ToString("F2") + "%";

            networkPreview.Render(worldGrid.TopMostUsed);

        }

        WeakReference[] weakReference = new WeakReference[] { null, null };
        public void StartStop(object sender, RoutedEventArgs e)
        {
            if (worldGrid.SimulationRunning)
            {
                simulationSettings.SimulationIdle();
                worldGrid.Stop();
                statusSimultation.Text = "Status: Idle";
                toolRun.Content = "Run";

                while (worldGrid.SimulationRunning)
                    System.Threading.Thread.Sleep(100);
                return;
            }

            simulationSettings.SimulationRunning();
            mainTabControl.SelectedItem = simultationTab;
            statusSimultation.Text = "Status: Running...";
            toolRun.Content = "Stop";

            worldGrid.SelectionFunction = null;

            // Unload the previous weakReference
            for (var i = 0; i < weakReference.Length; i++)
            {
                if (weakReference[i] == null)
                    continue;
                CSParsing.UnloadAssembly(weakReference[i]);
                weakReference[i] = null;
            }

            try
            {
                weakReference[0] = CSParsing.LoadAndExecute("using NeuroBox;using System;public static class EvalClass{ public static bool EvalFunction(Critter critter){" + SelectionCondition + "}}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error in the Selection Condition code");
                return;
            }

            var assembly = ((CSParsing.SimpleUnloadableAssemblyLoadContext)weakReference[0].Target).Assemblies.First();
            var method1 = assembly.GetType("EvalClass").GetMethod("EvalFunction", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            worldGrid.SelectionFunction = (critter) => !(bool)method1.Invoke(null, new object[] { critter });

            try
            {
                weakReference[1] = CSParsing.LoadAndExecute("using NeuroBox;using System; public static class EvalSpawnClass { public static (int,int) EvalFunction(Random rnd, WorldGrid worldGrid) {" + SpawnCoordinate + "}}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error in the Spawn code");
                return;
            }

            assembly = ((CSParsing.SimpleUnloadableAssemblyLoadContext)weakReference[1].Target).Assemblies.First();
            var method2 = assembly.GetType("EvalSpawnClass").GetMethod("EvalFunction", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            worldGrid.SpawnCoordinateFunction = (worldGrid) => ((int, int))method2.Invoke(null, new object[] { WorldGrid.Random, worldGrid });

            signalPlot.Clear();
            survivalPlot.Plot.SetAxisLimits(signalPlot.GetAxisLimits());
            survivalPlot.Refresh();

            worldGrid.LifeSpan = simulationSettings.LifeSpan;
            worldGrid.InternalNeurons = simulationSettings.InternalNeurons;
            worldGrid.NetworkConnections = simulationSettings.NetworkConnections;
            worldGrid.GridSize = simulationSettings.GridSize;
            worldGrid.NumberCritter = simulationSettings.NumberCritter;
            worldGrid.MutationRate = Math.Min(1, Math.Max(0, simulationSettings.MutationRate));
            worldGrid.MinReproductionFactor = Math.Min(1, Math.Max(0, simulationSettings.MinReproductionFactor));
            worldGrid.DnaMixing = simulationSettings.DnaMixing;

            worldGrid.Start();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Activate();
        }
    }
}
