using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace NeuroBox
{
    /// <summary>
    /// Interaction logic for SimulationSettings.xaml
    /// </summary>
    public partial class SimulationSettings : UserControl, INotifyPropertyChanged
    {
        [NotifyParentProperty(true)]
        public int InternalNeurons { get; set; } = 2;
        [NotifyParentProperty(true)]
        public int NetworkConnections { get; set; } = 6;
        [NotifyParentProperty(true)]
        public int GridSize { get; set; } = 100;
        [NotifyParentProperty(true)]
        public int LifeSpan { get; set; } = 300;
        [NotifyParentProperty(true)]
        public int NumberCritter { get; set; } = 500;
        [NotifyParentProperty(true)]
        public double MutationRate { get; set; } = 0.001;
        [NotifyParentProperty(true)]
        public double MinReproductionFactor { get; set; } = 0.9;
        [NotifyParentProperty(true)]
        public bool DnaMixing { get; set; } = true;
        public MainWindow MainWindow { get; set; }

        public TextBlock Survival => survival;
        public TextBlock Generation => generation;
        public TextBlock TimePerGeneration => timePerGeneration;
        public TextBlock GeneticSimilarities => geneticSimilarities;

        public void Changed()
        {
            foreach (var prop in GetType()
                .GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
                .Where(p => p.GetCustomAttributes(typeof(NotifyParentPropertyAttribute), false).Any()))
                PropertyChanged(this, new PropertyChangedEventArgs(prop.Name));
        }

        private bool inRealTime = false;

        public event PropertyChangedEventHandler? PropertyChanged;

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
            dnaCheckBox.IsEnabled = false;
            foreach (var t in parameterGrid.Children.OfType<TextBox>())
                t.IsEnabled = false;
        }

        public void SimulationIdle()
        {
            dnaCheckBox.IsEnabled = true;
            foreach (var t in parameterGrid.Children.OfType<TextBox>())
                t.IsEnabled = true;
        }
    }
}
