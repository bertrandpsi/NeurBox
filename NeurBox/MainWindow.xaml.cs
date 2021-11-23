using CSScriptLib;
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

        private bool inRealTime = false;
        private ScatterPlotList signalPlot;

        public string SelectionCondition { get; set; } = @"var a = 50 - critter.X;
var b = 50 - critter.Y;
var d=Math.Sqrt(b * b + a * a);
return (d > 30);";

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
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            //https://github.com/oleg-shilo/cs-script/wiki
            dynamic script = CSScript.Evaluator.LoadMethod(@"
public bool Evaluate(NeurBox.Critter critter)
{" + SelectionCondition + @"}");
            worldGrid.SelectionFunction = (critter) => script.Evaluate(critter);            

            signalPlot.Clear();
            survivalPlot.Plot.SetAxisLimits(signalPlot.GetAxisLimits());
            survivalPlot.Refresh();

            if (startButton.Content.ToString() == "Stop")
            {
                worldGrid.Stop();
                startButton.Content = "Start";
                foreach (var t in parameterGrid.Children.OfType<TextBox>())
                    t.IsEnabled = true;
            }
            else
            {
                worldGrid.LifeSpan = LifeSpan;
                worldGrid.InternalNeurons = InternalNeurons;
                worldGrid.NetworkConnections = NetworkConnections;
                worldGrid.GridSize = GridSize;
                worldGrid.NumberCritter = NumberCritter;
                worldGrid.MutationRate = MutationRate;
                worldGrid.Reset();
                worldGrid.Spawn();
                worldGrid.Start();
                startButton.Content = "Stop";
                foreach (var t in parameterGrid.Children.OfType<TextBox>())
                    t.IsEnabled = false;
            }
        }
    }
}
