using NeurBox.NeuronalNet;
using ScottPlot.Plottable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        public double MutationRate { get; set; } = 0.005;
        public double MinReproductionFactor { get; set; } = 0.9;

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

            // Pre-run the code parsing to speed up on run
            var w = CSParsing.LoadAndExecute(@"using NeurBox; using System; public static class EvalClass { public static bool EvalFunction(Critter critter) { return true; } } ");
            var assembly = ((SimpleUnloadableAssemblyLoadContext)w.Target).Assemblies.First();
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
            survival.Text = (worldGrid.SurvivalRate * 100).ToString("F2") + "%";
            statusSurvival.Text = "Survival: " + survival.Text;
            generation.Text = worldGrid.Generation.ToString();
            timePerGeneration.Text = worldGrid.TimePerGeneration.ToString(@"hh\:mm\:ss\.ff");
            statusTimePerGeneration.Text = "Time: " + timePerGeneration.Text;
            geneticSimilarities.Text = (worldGrid.DNASimilarity * 100).ToString("F2") + "%";

            networkPreview.Children.Clear();
            var pos = 0;
            foreach (var t in worldGrid.TopMostUsed.ToList())
            {
                var title = new TextBlock { Text = "Position " + (pos + 1), Width = double.NaN, TextAlignment = TextAlignment.Left, FontSize = 20, FontWeight = FontWeights.Bold, Margin = new Thickness(20) };
                networkPreview.Children.Add(title);
                networkPreview.Children.Add(DrawNeuronalNet(t));
                pos++;
            }
        }

        private Canvas DrawNeuronalNet(Critter critter)
        {
            var canvas = new Canvas { Height = 300 };
            var pos = new int[] { 10, 10, 10 };
            var visitedNode = new List<Neuron>();
            var toVisit = new Queue<Neuron>();

            // All the output nodes
            var usedOutputs = critter.Neurons.OfType<OutputNeuron>().Where(row => row.HasConnections);
            toVisit = new Queue<Neuron>(usedOutputs);
            visitedNode.AddRange(usedOutputs);
            var connections = new List<NeuronalConnection>();
            var neuronLookup = new Dictionary<Neuron, Grid>();

            // Draw all the neurons
            while (toVisit.Count > 0)
            {
                var n = toVisit.Dequeue();

                var g = new Grid();
                neuronLookup.Add(n, g);
                var e = new Ellipse();
                g.Children.Add(e);
                var t = new TextBlock { Text = TypeTitle(n) };
                g.Children.Add(t);
                canvas.Children.Add(g);
                if (n is OutputNeuron)
                {
                    g.SetValue(Canvas.LeftProperty, (double)pos[2]);
                    g.SetValue(Canvas.TopProperty, (double)220);
                    pos[2] += 110;
                }
                else if (n is InputNeuron)
                {
                    g.SetValue(Canvas.LeftProperty, (double)pos[0]);
                    g.SetValue(Canvas.TopProperty, (double)10);
                    pos[0] += 110;
                }
                else
                {
                    g.SetValue(Canvas.LeftProperty, (double)pos[1]);
                    g.SetValue(Canvas.TopProperty, (double)110);
                    pos[1] += 110;
                }
                foreach (var c in n.Connections)
                {
                    connections.Add(c);
                    if (visitedNode.Contains(c.From))
                        continue;
                    visitedNode.Add(c.From);
                    toVisit.Enqueue(c.From);
                }
            }

            // Draw all the connections
            foreach (var c in connections)
            {
                if (c.From == c.To)
                    continue;
                if (!neuronLookup.ContainsKey(c.From) || !neuronLookup.ContainsKey(c.To))
                    continue;
                var l = new Line();
                l.X1 = (double)neuronLookup[c.From].GetValue(Canvas.LeftProperty) + 35.0;
                l.Y1 = (double)neuronLookup[c.From].GetValue(Canvas.TopProperty) + 35.0;
                l.X2 = (double)neuronLookup[c.To].GetValue(Canvas.LeftProperty) + 35.0;
                l.Y2 = (double)neuronLookup[c.To].GetValue(Canvas.TopProperty) + 35.0;
                l.StrokeThickness = Math.Max(0.5, Math.Abs(c.Intensity) * 3);
                l.Stroke = c.Intensity < 0 ? Brushes.Red : Brushes.Green;
                canvas.Children.Insert(0, l);
            }

            return canvas;
        }

        private string TypeTitle(object o)
        {
            var exp = new Regex("([a-z])([A-Z])");
            return exp.Replace(o.GetType().Name, "$1 $2");
        }

        private void HandleLinkClick(object sender, RoutedEventArgs e)
        {
            Hyperlink hl = (Hyperlink)sender;
            string navigateUri = hl.NavigateUri.ToString();
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("cmd", $"/c start {navigateUri}") { CreateNoWindow = true });
            e.Handled = true;
        }

        WeakReference weakReference = null;
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (startButton.Content.ToString() == "Stop")
            {
                worldGrid.Stop();
                startButton.Content = "Start";
                statusSimultation.Text = "Status: Idle";
                toolRun.Content = "Run";
                dnaCheckBox.IsEnabled = true;
                foreach (var t in parameterGrid.Children.OfType<TextBox>())
                    t.IsEnabled = true;
                return;
            }

            mainTabControl.SelectedItem = simultationTab;
            statusSimultation.Text = "Status: Running...";
            startButton.Content = "Stop";
            toolRun.Content = "Stop";

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
            dnaCheckBox.IsEnabled = false;
            foreach (var t in parameterGrid.Children.OfType<TextBox>())
                t.IsEnabled = false;
        }
    }
}
