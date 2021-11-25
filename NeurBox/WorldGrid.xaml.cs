using NeurBox.Utilities;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
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

        public bool SimulationRunning => SimulationMustRun || simulationRunner != null;

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
        List<CritterDisplay> CrittersDisplay { get; set; } = new List<CritterDisplay>();
        public Predicate<Critter> SelectionFunction;

        public int[,] Grid;
        int simulationTime = 0;
        public int Generation { get; set; } = 0;
        public double SurvivalRate { get; private set; } = 1;
        public double MutationRate { get; internal set; }
        public Stopwatch TimeWatch { get; private set; }
        public TimeSpan TimePerGeneration { get; private set; }
        public bool DnaMixing { get; internal set; }
        public double DNASimilarity { get; private set; }
        List<Critter> topMostUsed = new List<Critter>();
        public List<Critter> TopMostUsed
        {
            get
            {
                lock (topMostLock)
                    return topMostUsed.ToList();
            }
            private set
            {
                topMostUsed = value;
            }
        }
        public double MinReproductionFactor { get; set; }

        public void PaintSafeArea()
        {
            worldCanvas.Children.Clear();
            var critter = new Critter { X = 0, Y = 0 };
            var fill = new SolidColorBrush(Color.FromRgb(180, 255, 180));
            for (int x = 0; x < GridSize; x += 2)
            {
                for (int y = 0; y < GridSize; y += 2)
                {
                    critter.X = x;
                    critter.Y = y;
                    if (!SelectionFunction(critter))
                    {
                        var r = new Rectangle { Width = 8.5, Height = 8.5, Fill = fill };
                        r.SetValue(Canvas.LeftProperty, x * 4.0);
                        r.SetValue(Canvas.TopProperty, y * 4.0);
                        worldCanvas.Children.Add(r);
                    }
                }
            }
        }

        internal void Reset()
        {
            simulationTime = 0;
            var toClear = worldCanvas.Children.OfType<CritterDisplay>().ToList();
            toClear.ForEach(critter => worldCanvas.Children.Remove(critter));
            Critters.Clear();
            CrittersDisplay.Clear();
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
                    c.X = Random.Next(0, GridSize - 1);
                    c.Y = Random.Next(0, GridSize - 1);
                } while (Grid[c.X, c.Y] != -1);
                Grid[c.X, c.Y] = 1;
                c.Build();
            });

            // Place the critter on the screen
            Critters.ForEach(c =>
            {
                var d = new CritterDisplay { Critter = c };
                d.CalculateColor();
                worldCanvas.Children.Add(d);
                d.SetValue(Canvas.LeftProperty, (double)c.X * 4);
                d.SetValue(Canvas.TopProperty, (double)c.Y * 4);
                CrittersDisplay.Add(d);
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
                CrittersDisplay.ForEach(c =>
                {
                    c.SetValue(Canvas.LeftProperty, (double)c.X * 4);
                    c.SetValue(Canvas.TopProperty, (double)c.Y * 4);
                });
            }
        }

        private List<string> MixDNA(List<string> dnas)
        {
            var nbToDo = dnas.Count;
            var result = new List<string>();
            for (int i = 0; i < nbToDo; i++)
            {
                var a = dnas[Random.Next(nbToDo)];
                var b = dnas[Random.Next(nbToDo)];
                if (a == b)
                    result.Add(a);
                else
                {
                    var pa = a.Split(' ');
                    var pb = b.Split(' ');
                    var genes = new List<string>();
                    for (int j = 0; j < pa.Length; j++)
                    {
                        if (Random.Next(2) == 0)
                            genes.Add(pa[j]);
                        else
                            genes.Add(pb[j]);
                    }
                    result.Add(string.Join(" ", genes));
                }
            }
            return result;
        }

        bool isCalculatingSimilarities = false;
        private object topMostLock = new object();

        void CalculatingSimilarities(List<Critter> critters)
        {
            if (isCalculatingSimilarities)
                return;
            isCalculatingSimilarities = true;
            // In a background thread as it's really slow to check all the permutations
            Task.Run(() =>
            {
                var allCouples = critters.AllCouplePermutations().Select(couple => new
                {
                    Couple = couple,
                    Similarity = couple.Item1.CompareDNA(couple.Item2),
                }).ToList();
                DNASimilarity = allCouples.Average(row => row.Similarity);
                lock (topMostLock)
                {
                    TopMostUsed = allCouples.OrderByDescending(row => row.Similarity).Take(3).Select(row => row.Couple.Item1).ToList();
                }
                isCalculatingSimilarities = false;
            });
        }

        private void NextGeneration()
        {
            Generation++;
            TimePerGeneration = TimeWatch.Elapsed / Generation;
            lock (Critters)
            {
                CrittersDisplay.ForEach(c =>
                {
                    c.SetValue(Canvas.LeftProperty, (double)c.X * 4);
                    c.SetValue(Canvas.TopProperty, (double)c.Y * 4);
                });

                CalculatingSimilarities(Critters.ToList());

                var dnas = SelectionCriteria();

                Reset();

                if (DnaMixing)
                    dnas = MixDNA(dnas);

                if (dnas.Count > 0)
                    Critters.AddRange(Enumerable.Range(0, (int)Math.Max(NumberCritter * MinReproductionFactor, Critters.Count)).Select(_ => Critter.FromDNA(dnas[Random.Next(0, dnas.Count)], MutationRate)));
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
            if (SelectionFunction != null)
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

