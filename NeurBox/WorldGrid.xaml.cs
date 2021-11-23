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
    /// Interaction logic for WorldGrid.xaml
    /// </summary>
    public partial class WorldGrid : UserControl
    {
        public static Random Random = new Random();

        public WorldGrid()
        {
            InitializeComponent();
        }

        public int LifeSpan { get; set; }
        public int InternalNeurons { get; set; }
        public int NetworkConnections { get; set; }
        public int GridSize { get; internal set; }
        List<Critter> Critters { get; set; } = new List<Critter>();
        public int[,] Grid;

        internal void Reset()
        {
            Critters.Clear();
            worldCanvas.Children.Clear();
        }

        internal void Spawn(int numberCritter)
        {
            Grid = new int[GridSize, GridSize];
            for (int i = 0; i < GridSize; i++)
                for (int j = 0; j < GridSize; j++)
                    Grid[i, j] = -1;

            Critters = Enumerable.Range(0, numberCritter).Select(cId =>
            {
                var result = new Critter
                {
                    Id = cId,
                    MaxLifeSpan = LifeSpan,
                    InternalNeurons = InternalNeurons,
                    NetworkConnections = NetworkConnections,
                    X = Random.Next(0, GridSize),
                    Y = Random.Next(0, GridSize),
                    GridSize = GridSize,
                    World = this
                };
                while (Grid[result.X, result.Y] != -1)
                {
                    result.X = Random.Next(0, GridSize);
                    result.Y = Random.Next(0, GridSize);
                }
                Grid[result.X, result.Y] = result.Id;
                return result;
            }).ToList();

            Critters.ForEach(c =>
            {
                worldCanvas.Children.Add(c);
                c.SetValue(Canvas.LeftProperty, (double)c.X * 4);
                c.SetValue(Canvas.TopProperty, (double)c.Y * 4);
                c.Build();
            });
        }

        internal void LogicLoop()
        {
            Critters.ForEach(c =>
            {
                c.Execute();
                c.SetValue(Canvas.LeftProperty, (double)c.X * 4);
                c.SetValue(Canvas.TopProperty, (double)c.Y * 4);
            });
        }

        internal void Start()
        {
        }
    }
}
