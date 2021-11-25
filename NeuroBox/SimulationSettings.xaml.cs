using System.Windows;
using System.Windows.Controls;

namespace NeuroBox
{
    /// <summary>
    /// Interaction logic for SimulationSettings.xaml
    /// </summary>
    public partial class SimulationSettings : UserControl
    {
        public int InternalNeurons { get; set; } = 2;
        public int NetworkConnections { get; set; } = 6;
        public int GridSize { get; set; } = 100;
        public int LifeSpan { get; set; } = 300;
        public int NumberCritter { get; set; } = 500;
        public double MutationRate { get; set; } = 0.001;
        public double MinReproductionFactor { get; set; } = 0.9;
        public bool DnaMixing { get; set; } = true;
        public MainWindow MainWindow { get; set; }

        public TextBlock Survival => survival;
        public TextBlock Generation => generation;
        public TextBlock TimePerGeneration => timePerGeneration;
        public TextBlock GeneticSimilarities => geneticSimilarities;

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
                MainWindow.WorldGrid.InRealTime = value;
            }
        }

        public SimulationSettings()
        {
            InitializeComponent();
        }

        public void SimulationRunning()
        {
            startButton.Content = "Stop";
            dnaCheckBox.IsEnabled = false;
            foreach (var t in parameterGrid.Children.OfType<TextBox>())
                t.IsEnabled = false;
        }

        public void SimulationIdle()
        {
            startButton.Content = "Start";
            dnaCheckBox.IsEnabled = true;
            foreach (var t in parameterGrid.Children.OfType<TextBox>())
                t.IsEnabled = true;
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow?.StartStop(sender, e);
        }
    }
}
