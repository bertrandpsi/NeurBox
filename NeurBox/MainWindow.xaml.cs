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
        public int InternalNeurons { get; set; } = 2;
        public int NetworkConnections { get; set; } = 10;
        public int GridSize { get; set; } = 100;
        public int LifeSpan { get; set; } = 300;
        public int NumberCritter { get; set; } = 100;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            worldGrid.Reset();
            worldGrid.LifeSpan = LifeSpan;
            worldGrid.InternalNeurons = InternalNeurons;
            worldGrid.NetworkConnections = NetworkConnections;
            worldGrid.GridSize = GridSize;
            worldGrid.Spawn(NumberCritter);
            worldGrid.Start();
        }
    }
}
