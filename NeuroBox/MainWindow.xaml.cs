using Microsoft.Win32;
using ScottPlot.Plottable;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Serialization;
using System.Xml.Xsl;

namespace NeuroBox
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        System.Windows.Threading.DispatcherTimer dispatcherTimer;

        private ScatterPlotList signalPlot;

        [NotifyParentProperty(true)]
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

        [NotifyParentProperty(true)]
        public string SpawnCoordinate { get; set; } = @"
// You may re-use any of those code to change the selection or write your own

// Random position within a circle of 20 starting at 20,20 
// var r = 20 * Math.Sqrt(rnd.NextDouble());
// var theta = rnd.NextDouble() * 2.0 * Math.PI;
// return ((int)(20 + r * Math.Cos(theta)),(int)(20 + r * Math.Sin(theta)));

// Random position within the world grid
return (rnd.Next(worldGrid.GridSize-1),rnd.Next(worldGrid.GridSize-1));
";

        [NotifyParentProperty(true)]
        public string WorldBlocking { get; set; } = @"
// You may re-use any of those code to change the selection or write your own

// Place some random blocks
// return rnd.NextDouble() < 0.05;

// Place a wall at X 30 and X 70 with an hole at around Y 30
// return ((x>28 && x<32) || (x>68 && x<72))  && (y < 25 || y > 35);

// All empty
return false;
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
            var w = CSParsing.LoadAndExecute(@"using NeuroBox;using NeuroBox.NeuronalNet; using System; public static class EvalClass { public static bool EvalFunction(Critter critter) { return true; } } ");
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

        public void Open(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.FileName = "Simulation";
            dlg.DefaultExt = ".neuro";
            dlg.Filter = "NeuroBox files (.neuro)|*.neuro";
            if (dlg.ShowDialog() == true)
            {
                var fileName = dlg.FileName;


                XmlSerializer ser = new XmlSerializer(typeof(SaveFile));
                using (var file = File.OpenRead(fileName))
                {
                    var saveFile = (SaveFile)ser.Deserialize(file);

                    simulationSettings.LifeSpan = saveFile.Parameters.LifeSpan;
                    simulationSettings.InternalNeurons = saveFile.Parameters.InternalNeurons;
                    simulationSettings.NetworkConnections = saveFile.Parameters.NetworkConnections;
                    simulationSettings.GridSize = saveFile.Parameters.GridSize;
                    simulationSettings.NumberCritter = saveFile.Parameters.NumberCritter;
                    simulationSettings.MutationRate = saveFile.Parameters.MutationRate;
                    simulationSettings.MinReproductionFactor = saveFile.Parameters.MinReproductionFactor;
                    simulationSettings.DnaMixing = saveFile.Parameters.DnaMixing;
                    SelectionCondition = saveFile.SelectionCondition;
                    SpawnCoordinate = saveFile.SpawnCoordinate;
                    WorldBlocking = saveFile.WorldBlocking;

                    simulationSettings.Changed();
                    Changed();
                }
            }
        }

        string lastSaveFile = null;
        public void Save(object sender, RoutedEventArgs e)
        {
            if (lastSaveFile == null)
                SaveAs(sender, e);
            else
                SaveFile(lastSaveFile);
        }

        public void SaveAs(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.FileName = "Simulation";
            dlg.DefaultExt = ".neuro";
            dlg.Filter = "NeuroBox files (.neuro)|*.neuro";
            if (dlg.ShowDialog() == true)
            {
                lastSaveFile= dlg.FileName;
                var fileName = dlg.FileName;
                SaveFile(fileName);
            }
        }

        private void SaveFile(string fileName)
        {
            var saveFile = new SaveFile();
            saveFile.Parameters.LifeSpan = simulationSettings.LifeSpan;
            saveFile.Parameters.InternalNeurons = simulationSettings.InternalNeurons;
            saveFile.Parameters.NetworkConnections = simulationSettings.NetworkConnections;
            saveFile.Parameters.GridSize = simulationSettings.GridSize;
            saveFile.Parameters.NumberCritter = simulationSettings.NumberCritter;
            saveFile.Parameters.MutationRate = Math.Min(1, Math.Max(0, simulationSettings.MutationRate));
            saveFile.Parameters.MinReproductionFactor = Math.Min(1, Math.Max(0, simulationSettings.MinReproductionFactor));
            saveFile.Parameters.DnaMixing = simulationSettings.DnaMixing;
            saveFile.SelectionCondition = SelectionCondition;
            saveFile.SpawnCoordinate = SpawnCoordinate;
            saveFile.WorldBlocking = WorldBlocking;

            var xns = new XmlSerializerNamespaces();
            xns.Add(string.Empty, string.Empty);
            XmlSerializer ser = new XmlSerializer(typeof(SaveFile));
            var settings = new System.Xml.XmlWriterSettings { Indent = true, NewLineOnAttributes = true, OmitXmlDeclaration = true };
            using (var file = File.Create(fileName))
            {
                using (var writer = System.Xml.XmlWriter.Create(file, settings))
                {
                    ser.Serialize(writer, saveFile, xns);
                }
            }
        }

        public void Changed()
        {
            foreach (var prop in GetType()
                .GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
                .Where(p => p.GetCustomAttributes(typeof(NotifyParentPropertyAttribute), false).Any()))
                PropertyChanged(this, new PropertyChangedEventArgs(prop.Name));
        }

        bool simultationStopped = true;
        public void Stop(object sender, RoutedEventArgs e)
        {
            if (!worldGrid.SimulationRunning && simultationStopped)
                return;

            simulationSettings.SimulationIdle();
            worldGrid.Stop();
            statusSimultation.Text = "Status: Idle";

            btnOpen.IsEnabled = true;
            btnSave.IsEnabled = true;
            btnSaveAs.IsEnabled = true;

            btnRun.IsEnabled = true;
            btnPause.IsEnabled = false;
            btnStop.IsEnabled = false;
            //toolRun.Content = "Run";

            while (worldGrid.SimulationRunning)
                System.Threading.Thread.Sleep(100);
            simultationStopped = true;
        }

        public void Pause(object sender, RoutedEventArgs e)
        {
            if (!worldGrid.SimulationRunning)
                return;

            btnOpen.IsEnabled = true;
            btnSave.IsEnabled = true;
            btnSaveAs.IsEnabled = true;

            worldGrid.Stop();
            statusSimultation.Text = "Status: Paused";
            btnRun.IsEnabled = true;
            btnPause.IsEnabled = false;
            btnStop.IsEnabled = true;

            while (worldGrid.SimulationRunning)
                System.Threading.Thread.Sleep(100);
            simultationStopped = false;
        }

        WeakReference[] weakReference = new WeakReference[] { null, null, null };

        public event PropertyChangedEventHandler? PropertyChanged;

        public void Start(object sender, RoutedEventArgs e)
        {
            btnOpen.IsEnabled = false;
            btnSave.IsEnabled = false;
            btnSaveAs.IsEnabled = false;

            btnRun.IsEnabled = false;
            btnPause.IsEnabled = true;
            btnStop.IsEnabled = true;
            statusSimultation.Text = "Status: Running...";

            if (simultationStopped == false)
            {
                worldGrid.Continue();
                return;
            }

            // In case the focus did not change
            object focusObj = FocusManager.GetFocusedElement(this);
            if (focusObj != null && focusObj is TextBox)
            {
                var binding = (focusObj as TextBox).GetBindingExpression(TextBox.TextProperty);
                binding.UpdateSource();
            }


            simulationSettings.SimulationRunning();
            mainTabControl.SelectedItem = simultationTab;
            //toolRun.Content = "Stop";

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
                weakReference[0] = CSParsing.LoadAndExecute("using NeuroBox;using NeuroBox.NeuronalNet;using System;public static class EvalClass{ public static bool EvalFunction(Critter critter){" + SelectionCondition + "}}");
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
                weakReference[1] = CSParsing.LoadAndExecute("using NeuroBox;using NeuroBox.NeuronalNet;using System; public static class EvalSpawnClass { public static (int,int) EvalFunction(Random rnd, WorldGrid worldGrid) {" + SpawnCoordinate + "}}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error in the Spawn code");
                return;
            }

            assembly = ((CSParsing.SimpleUnloadableAssemblyLoadContext)weakReference[1].Target).Assemblies.First();
            var method2 = assembly.GetType("EvalSpawnClass").GetMethod("EvalFunction", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            worldGrid.SpawnCoordinateFunction = (worldGrid) => ((int, int))method2.Invoke(null, new object[] { WorldGrid.Random, worldGrid });

            try
            {
                weakReference[2] = CSParsing.LoadAndExecute("using NeuroBox;using NeuroBox.NeuronalNet;using System; public static class EvalBlockingClass { public static bool EvalFunction(int x, int y, Random rnd) {" + WorldBlocking + "}}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error in the Spawn code");
                return;
            }

            assembly = ((CSParsing.SimpleUnloadableAssemblyLoadContext)weakReference[2].Target).Assemblies.First();
            var method3 = assembly.GetType("EvalBlockingClass").GetMethod("EvalFunction", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            worldGrid.WorldBlockingFunction = (x, y, rnd) => (bool)method3.Invoke(null, new object[] { x, y, rnd });

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
