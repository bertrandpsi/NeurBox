﻿using System;
using System.Collections.Generic;
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
        Thread simulationRunner;
        bool SimulationMustRun = true;

        public WorldGrid()
        {
            InitializeComponent();
            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(20);
        }

        public int LifeSpan { get; set; }
        public int InternalNeurons { get; set; }
        public int NetworkConnections { get; set; }
        public int GridSize { get; internal set; }
        public int NumberCritter { get; set; }
        List<Critter> Critters { get; set; } = new List<Critter>();

        public int[,] Grid;
        int simulationTime = 0;
        public int Generation { get; set; } = 0;
        public double SurvivalRate { get; private set; } = 1;
        public double MutationRate { get; internal set; }

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
            dispatcherTimer.Stop();
            SimulationMustRun = false;
        }

        internal void Spawn()
        {
            Critters.AddRange(Enumerable.Range(0, NumberCritter - Critters.Count).Select(cId => new Critter
            {
                Id = cId,
                MaxLifeSpan = LifeSpan,
                InternalNeurons = InternalNeurons,
                NetworkConnections = NetworkConnections,
                GridSize = GridSize,
                World = this
            }));

            Critters.ForEach(c =>
            {
                do
                {
                    c.X = Random.Next(0, GridSize);
                    c.Y = Random.Next(0, GridSize);
                } while (Grid[c.X, c.Y] != -1);
                Grid[c.X, c.Y] = 1;
            });

            Critters.ForEach(c =>
            {
                worldCanvas.Children.Add(c);
                c.SetValue(Canvas.LeftProperty, (double)c.X * 4);
                c.SetValue(Canvas.TopProperty, (double)c.Y * 4);
                c.Build();
            });
        }

        void SimulationThread()
        {
            while (SimulationMustRun)
            {
                LogicLoop();
                simulationTime++;
                {
                    if (simulationTime >= LifeSpan)
                    {
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            NextGeneration();
                        }));
                        break;
                    }
                }
            }
        }

        internal void LogicLoop()
        {
            lock (Critters)
            {
                Critters.ForEach(c =>
                {
                    c.LifeSpan++;
                    c.Execute();
                });
            }
        }
        private void dispatcherTimer_Tick(object? sender, EventArgs e)
        {
            LogicLoop();
            simulationTime++;
            if (simulationTime >= LifeSpan)
            {
                NextGeneration();
                return;
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
            var dnas = SelectionCriteria();

            Reset();

            if (dnas.Count > 0)
                Critters.AddRange(Enumerable.Range(0, NumberCritter).Select(_ => Critter.FromDNA(dnas[Random.Next(0, dnas.Count)])));
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
            Generation++;
        }

        private List<string> SelectionCriteria()
        {
            Critters.RemoveAll(row => row.X < 70);
            SurvivalRate = (double)Critters.Count / (double)NumberCritter;
            var dnas = Critters.Select(row => row.DNA).ToList();
            return dnas;
        }

        internal void Start()
        {
            simulationTime = 0;
            /*simulationRunner = new Thread(SimulationThread);
            simulationRunner.IsBackground = true;
            simulationRunner.Start();*/
            dispatcherTimer.Start();
        }
    }
}

