using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
        System.Windows.Threading.DispatcherTimer dispatcherTimer;
        Thread? simulationRunner;
        bool SimulationMustRun = true;

        public event EventHandler<double> GenerationSurvivalEvent;

        public WorldGrid()
        {
            InitializeComponent();
            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(20);
        }

        public bool InRealTime { get; set; }

        public int LifeSpan { get; set; }
        public int InternalNeurons { get; set; }
        public int NetworkConnections { get; set; }
        public int GridSize { get; internal set; }
        public int NumberCritter { get; set; }
        List<Critter> Critters { get; set; } = new List<Critter>();
        public Predicate<Critter> SelectionFunction;

        public int[,] Grid;
        int simulationTime = 0;
        public int Generation { get; set; } = 0;
        public double SurvivalRate { get; private set; } = 1;
        public double MutationRate { get; internal set; }
        public Stopwatch TimeWatch { get; private set; }
        public TimeSpan TimePerGeneration { get; private set; }

        internal void Reset()
        {
            simulationTime = 0;
            Critters.Clear();
            worldCanvas.Children.Clear();
            Grid = new int[GridSize, GridSize];
            for (int i = 0; i < GridSize; i++)
                for (int j = 0; j < GridSize; j++)
                    Grid[i, j] = -1;
        }

        internal void Stop()
        {
            SimulationMustRun = false;
            while (simulationRunner != null)
                Thread.Sleep(100);
            TimeWatch.Stop();
            dispatcherTimer.Stop();
        }

        internal void Spawn()
        {
            // Generate some random Critter
            Critters.AddRange(Enumerable.Range(0, NumberCritter - Critters.Count).Select(cId => new Critter
            {
                Id = cId,
                MaxLifeSpan = LifeSpan,
                InternalNeurons = InternalNeurons,
                NetworkConnections = NetworkConnections,
                GridSize = GridSize,
                World = this
            }));

            // Place the critter on the world map
            Critters.ForEach(c =>
            {
                do
                {
                    c.X = Random.Next(0, GridSize);
                    c.Y = Random.Next(0, GridSize);
                } while (Grid[c.X, c.Y] != -1);
                Grid[c.X, c.Y] = 1;
                c.Build();
            });

            // Place the critter on the screen
            Critters.ForEach(c =>
            {
                worldCanvas.Children.Add(c);
                c.SetValue(Canvas.LeftProperty, (double)c.X * 4);
                c.SetValue(Canvas.TopProperty, (double)c.Y * 4);
            });
        }

        void SimulationThread()
        {
            while (SimulationMustRun)
            {
                if (InRealTime)
                {
                    Thread.Sleep(10);
                    continue;
                }

                ParallelLogicLoop();
                if (!SimulationMustRun)
                    break;
                simulationTime++;
                if (simulationTime >= LifeSpan)
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        NextGeneration();
                    }));
                    simulationTime = 0;
                }
            }
            simulationRunner = null;
        }

        internal void LogicLoop()
        {
            lock (Critters)
                Critters.ForEach(c => c.Execute());
        }

        internal void ParallelLogicLoop()
        {
            lock (Critters)
                Parallel.ForEach(Critters, c => c.Execute());
        }

        private void dispatcherTimer_Tick(object? sender, EventArgs e)
        {
            if (InRealTime)
            {
                LogicLoop();
                simulationTime++;
                if (simulationTime >= LifeSpan)
                    NextGeneration();
            }

            lock (Critters)
            {
                Critters.ForEach(c =>
                {
                    c.SetValue(Canvas.LeftProperty, (double)c.X * 4);
                    c.SetValue(Canvas.TopProperty, (double)c.Y * 4);
                });
            }
        }

        private void NextGeneration()
        {
            Generation++;
            TimePerGeneration = TimeWatch.Elapsed / Generation;
            lock (Critters)
            {
                Critters.ForEach(c =>
                {
                    c.SetValue(Canvas.LeftProperty, (double)c.X * 4);
                    c.SetValue(Canvas.TopProperty, (double)c.Y * 4);
                });

                var dnas = SelectionCriteria();

                Reset();

                if (dnas.Count > 0)
                    Critters.AddRange(Enumerable.Range(0, NumberCritter).Select(_ => Critter.FromDNA(dnas[Random.Next(0, dnas.Count)], MutationRate)));
                Critters.ForEach(c =>
                {
                    c.MaxLifeSpan = LifeSpan;
                    c.InternalNeurons = InternalNeurons;
                    c.NetworkConnections = NetworkConnections;
                    c.X = Random.Next(0, GridSize);
                    c.Y = Random.Next(0, GridSize);
                    c.GridSize = GridSize;
                    c.World = this;
                });
                Spawn();
            }
        }

        private List<string> SelectionCriteria()
        {
            /*Critters.RemoveAll(row =>
            {
                var a = 50 - row.X;
                var b = 50 - row.Y;
                var d=Math.Sqrt(b * b + a * a);
                return (d > 30);
            } );*/
            Critters.RemoveAll(SelectionFunction);
            SurvivalRate = (double)Critters.Count / (double)NumberCritter;
            GenerationSurvivalEvent?.Invoke(this, SurvivalRate);
            var dnas = Critters.Select(row => row.DNA).ToList();
            return dnas;
        }

        internal void Start()
        {
            while (simulationRunner != null)
                Thread.Sleep(10);

            simulationTime = 0;
            Generation = 0;
            SimulationMustRun = true;

            TimeWatch = new Stopwatch();
            TimeWatch.Start();

            simulationRunner = new Thread(SimulationThread);
            simulationRunner.IsBackground = true;
            simulationRunner.Start();
            dispatcherTimer.Start();
        }
    }
}

