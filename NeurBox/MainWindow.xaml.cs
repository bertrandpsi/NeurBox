using ScottPlot.Plottable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeurBox
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Windows.Threading.DispatcherTimer dispatcherTimer;

        public int InternalNeurons { get; set; } = 2;
        public int NetworkConnections { get; set; } = 6;
        public int GridSize { get; set; } = 100;
        public int LifeSpan { get; set; } = 300;
        public int NumberCritter { get; set; } = 500;
        public double MutationRate { get; set; } = 0.01;
        public double MinReproductionFactor { get; set; } = 0.8;

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
return (d < 30);";

        public bool DnaMixing { get; set; } = true;

        private bool inRealTime = false;
        public bool InRealTime
        {
            get
            {
                return inRealTime;
            }
            set
            {
                inRealTime = value;
                worldGrid.InRealTime = value;
            }
        }

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
        }

        private void WorldGrid_GenerationSurvivalEvent(object? sender, double survivalRate)
        {
            signalPlot.Add(worldGrid.Generation, survivalRate * 100);
            survivalPlot.Plot.SetAxisLimits(signalPlot.GetAxisLimits());
            survivalPlot.Refresh();
        }

        private void dispatcherTimer_Tick(object? sender, EventArgs e)
        {
            survival.Text = (worldGrid.SurvivalRate * 100).ToString("F2") + "%";
            generation.Text = worldGrid.Generation.ToString();
            timePerGeneration.Text = worldGrid.TimePerGeneration.ToString();
            geneticSimilarities.Text = (worldGrid.DNASimilarity * 100).ToString("F2") + "%";
        }

        WeakReference weakReference = null;
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (startButton.Content.ToString() == "Stop")
            {
                worldGrid.Stop();
                startButton.Content = "Start";
                ((Label)toolRun.Content).Content = "Run";
                dnaCheckBox.IsEnabled = true;
                foreach (var t in parameterGrid.Children.OfType<TextBox>())
                    t.IsEnabled = true;
                return;
            }

            // Unload the previous weakReference
            if (weakReference != null)
            {
                worldGrid.SelectionFunction = null;
                CSParsing.UnloadAssembly(weakReference);
                weakReference = null;
            }

            try
            {
                weakReference = CSParsing.LoadAndExecute(@"using NeurBox;
using System;
public static class EvalClass
{
    public static bool EvalFunction(Critter critter)
{
" + SelectionCondition + @"
}
}
");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error in the Selection Condition code");
                return;
            }

            var assembly = ((SimpleUnloadableAssemblyLoadContext)weakReference.Target).Assemblies.First();
            var method = assembly.GetType("EvalClass").GetMethod("EvalFunction", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            worldGrid.SelectionFunction = (critter) => !(bool)method.Invoke(null, new object[] { critter });

            signalPlot.Clear();
            survivalPlot.Plot.SetAxisLimits(signalPlot.GetAxisLimits());
            survivalPlot.Refresh();


            worldGrid.LifeSpan = LifeSpan;
            worldGrid.InternalNeurons = InternalNeurons;
            worldGrid.NetworkConnections = NetworkConnections;
            worldGrid.GridSize = GridSize;
            worldGrid.NumberCritter = NumberCritter;
            worldGrid.MutationRate = Math.Min(1, Math.Max(0, MutationRate));
            worldGrid.MinReproductionFactor = Math.Min(1, Math.Max(0, MinReproductionFactor));
            worldGrid.DnaMixing = DnaMixing;

            worldGrid.PaintSafeArea();

            worldGrid.Reset();
            worldGrid.Spawn();
            worldGrid.Start();
            startButton.Content = "Stop";
            ((Label)toolRun.Content).Content = "Stop";
            dnaCheckBox.IsEnabled = false;
            foreach (var t in parameterGrid.Children.OfType<TextBox>())
                t.IsEnabled = false;
        }
    }
}
