using NeurBox.NeuronalNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    /// Interaction logic for Critter.xaml
    /// </summary>
    public partial class Critter : UserControl
    {
        static SemaphoreSlim gridMovement = new SemaphoreSlim(1);

        public Critter()
        {
            InitializeComponent();
        }

        public int MaxLifeSpan { get; internal set; }
        public int LifeSpan { get; internal set; } = 0;
        public int InternalNeurons { get; internal set; }
        public int NetworkConnections { get; internal set; }
        public int X { get; internal set; }
        public int Y { get; internal set; }
        public int GridSize { get; internal set; }
        public int Id { get; internal set; }
        public WorldGrid World { get; set; }
        public string DNA => InternalNeurons.ToString("X03") + " " + string.Join(" ", Neurons.SelectMany(row => row.Connections.Select(c => c.DNA)));
        public int ConnectionsUsed => Neurons.Sum(n => n.Connections.Count);

        internal List<Neuron> Neurons = new List<Neuron>();

        static List<ConstructorInfo> inputs;
        static List<ConstructorInfo> outputs;

        static Critter()
        {
            inputs = Assembly.GetExecutingAssembly().GetTypes().Where(row => row.IsSubclassOf(typeof(InputNeuron))).Select(t=> t.GetConstructor(BindingFlags.Public | BindingFlags.Instance, Array.Empty<Type>())).ToList();
            outputs = Assembly.GetExecutingAssembly().GetTypes().Where(row => row.IsSubclassOf(typeof(OutputNeuron))).Select(t => t.GetConstructor(BindingFlags.Public | BindingFlags.Instance, Array.Empty<Type>())).ToList();
        }

        public void Build()
        {
            if (Neurons.Count == 0)
            {
                // Creates all the neurons
                Neurons.AddRange(inputs.Select(t => (Neuron)t.Invoke(Array.Empty<object>())));
                Neurons.AddRange(outputs.Select(t => (Neuron)t.Invoke(Array.Empty<object>())));
                Neurons.AddRange(Enumerable.Range(0, InternalNeurons).Select(_ => new InternalNeuron()));
                Neurons.ForEach(n => n.Critter = this);
            }

            if (ConnectionsUsed < NetworkConnections)
            {
                var nonInputs = Neurons.Skip(inputs.Count).ToList();
                var nonOuputs = Neurons.Take(inputs.Count + InternalNeurons).ToList();

                // Create all the connections
                for (var i = ConnectionsUsed; i < NetworkConnections;)
                {
                    var a = nonInputs[WorldGrid.Random.Next(nonInputs.Count)];
                    var b = nonOuputs[WorldGrid.Random.Next(nonOuputs.Count)];
                    if (a.AlreadyConnectedWith(b))
                        continue;
                    a.Connect(b, WorldGrid.Random.NextDouble() * 1.8 - 0.9);
                    i++;
                }
            }
        }

        Color ColorFromString(string src)
        {
            var hash = src.Substring(0, 6).GetHashCode();
            var r = Math.Min(255, (hash % 250) * 2.2);
            var g = Math.Min(255, ((hash / 250) % 250) * 2.2);
            var b = Math.Min(255, ((hash / (250 * 250)) % 250) * 2.2);
            return Color.FromRgb((byte)r, (byte)g, (byte)b);
        }

        public void CalculateColor()
        {
            var colors = DNA.Split(' ').Skip(1).Select(d => ColorFromString(d));
            dot.Fill = new SolidColorBrush(Color.FromRgb((byte)colors.Average(c => c.R), (byte)colors.Average(c => c.G), (byte)colors.Average(c => c.B)));
        }

        IEnumerable<(TTypeA, TTypeB)> Mix<TTypeA, TTypeB>(IEnumerable<TTypeA> a, IEnumerable<TTypeB> b)
        {
            var pos = 0;
            var enumerator = b.GetEnumerator();
            foreach (var av in a)
            {
                enumerator.MoveNext();
                yield return (av, enumerator.Current);
            }
        }

        public double CompareDNA(Critter other)
        {
            return Mix(DNA.Split(' ').Skip(1), other.DNA.Split(' ').Skip(1)).Average(genome => CompareGenome(genome.Item1, genome.Item2));
        }

        double CompareGenome(string a, string b)
        {
            if (a == b)
                return 1;
            if (a.Substring(0, 6) == b.Substring(0, 6))
            {
                var va = (int.Parse(a.Substring(6), System.Globalization.NumberStyles.HexNumber) - 4000.0) / 4000.0;
                var vb = (int.Parse(a.Substring(6), System.Globalization.NumberStyles.HexNumber) - 4000.0) / 4000.0;
                //return 0.75 + Math.Max(0, Math.Min(1, 1 - Math.Abs(va - vb))) / 4.0;
                return 0.75 + (Neuron.InRange(Math.Abs(va - vb)) + 1) / 8.0;
            }
            return 0;
        }

        internal void MoveEast()
        {
            gridMovement.Wait();
            if (X < World.GridSize - 2 && World.Grid[X + 1, Y] == -1)
            {
                World.Grid[X, Y] = -1;
                X++;
                World.Grid[X, Y] = Id;
            }
            gridMovement.Release();
        }

        internal void MoveNorth()
        {
            gridMovement.Wait();
            if (Y > 0 && World.Grid[X, Y - 1] == -1)
            {
                World.Grid[X, Y] = -1;
                Y--;
                World.Grid[X, Y] = Id;
            }
            gridMovement.Release();
        }

        internal void MoveSouth()
        {
            gridMovement.Wait();
            if (Y < World.GridSize - 2 && World.Grid[X, Y + 1] == -1)
            {
                World.Grid[X, Y] = -1;
                Y++;
                World.Grid[X, Y] = Id;
            }
            gridMovement.Release();
        }

        internal void MoveWest()
        {
            gridMovement.Wait();
            if (X > 0 && World.Grid[X - 1, Y] == -1)
            {
                World.Grid[X, Y] = -1;
                X--;
                World.Grid[X, Y] = Id;
            }
            gridMovement.Release();
        }

        internal static Critter FromDNA(string dna, double mutationRate)
        {
            var result = new Critter();
            var dnaConnections = dna.Split(' ').ToList();
            result.InternalNeurons = int.Parse(dnaConnections[0], System.Globalization.NumberStyles.HexNumber);

            result.Neurons.AddRange(inputs.Select(t => (Neuron)t.Invoke(Array.Empty<object>())));
            result.Neurons.AddRange(outputs.Select(t => (Neuron)t.Invoke(Array.Empty<object>())));
            result.Neurons.AddRange(Enumerable.Range(0, result.InternalNeurons).Select(_ => new InternalNeuron()));
            result.Neurons.ForEach(n => n.Critter = result);

            foreach (var d in dnaConnections.Skip(1))
            {
                if (WorldGrid.Random.NextDouble() < mutationRate) // We skip this connection (a random one will be created instead)
                    continue;
                var idFrom = int.Parse(d.Substring(0, 3), System.Globalization.NumberStyles.HexNumber);
                var idTo = int.Parse(d.Substring(3, 3), System.Globalization.NumberStyles.HexNumber);
                var intensity = (((double)int.Parse(d.Substring(6, 4), System.Globalization.NumberStyles.HexNumber)) - 4000) / 4000.0;
                var intensityChanger = WorldGrid.Random.NextDouble() * mutationRate;
                intensityChanger = 1 + (intensityChanger * 2 - intensityChanger);
                intensity *= intensityChanger;
                result.Neurons[idTo].Connect(result.Neurons[idFrom], intensity);
            }
            return result;
        }

        internal void Execute()
        {
            LifeSpan++;

            foreach (var n in Neurons.OfType<InputNeuron>().Where(row => row.IsConnected).Cast<InputNeuron>())
                n.StoreCache();

            foreach (var n in Neurons.OfType<OutputNeuron>().Where(row => row.HasConnections).Cast<OutputNeuron>())
                n.Execute();
        }
    }
}
