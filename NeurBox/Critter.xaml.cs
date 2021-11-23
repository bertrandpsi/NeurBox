using NeurBox.NeuronalNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    /// Interaction logic for Critter.xaml
    /// </summary>
    public partial class Critter : UserControl
    {
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

        static List<Type> inputs;
        static List<Type> outputs;

        static Critter()
        {
            inputs = Assembly.GetExecutingAssembly().GetTypes().Where(row => row.IsSubclassOf(typeof(InputNeuron))).ToList();
            outputs = Assembly.GetExecutingAssembly().GetTypes().Where(row => row.IsSubclassOf(typeof(OutputNeuron))).ToList();
        }

        public void Build()
        {
            if (Neurons.Count == 0)
            {
                // Creates all the neurons
                Neurons.AddRange(inputs.Select(t => (Neuron)t.GetConstructor(BindingFlags.Public | BindingFlags.Instance, Array.Empty<Type>()).Invoke(Array.Empty<object>())));
                Neurons.AddRange(outputs.Select(t => (Neuron)t.GetConstructor(BindingFlags.Public | BindingFlags.Instance, Array.Empty<Type>()).Invoke(Array.Empty<object>())));
                Neurons.AddRange(Enumerable.Range(0, InternalNeurons).Select(_ => new InternalNeuron()));
                Neurons.ForEach(n => n.Critter = this);
            }

            if (ConnectionsUsed < NetworkConnections)
            {
                var nonInputs = Neurons.Skip(inputs.Count).ToList();
                var nonOuputs = Neurons.Where(n => !(n is OutputNeuron)).ToList();

                // Create all the connections
                for (var i = ConnectionsUsed; i < NetworkConnections;)
                {
                    var a = nonInputs[WorldGrid.Random.Next(nonInputs.Count)];
                    var b = nonOuputs[WorldGrid.Random.Next(nonOuputs.Count)];
                    if (a.AlreadyConnectedWith(b))
                        continue;
                    a.Connect(b, WorldGrid.Random.NextDouble() * 0.8 + 0.2);
                    i++;
                }
            }

            CalculateColor();
        }

        public void CalculateColor()
        {
            var hash = DNA.GetHashCode();
            var r = hash % 200 + 30;
            var g = (hash / 200) % 200 + 30;
            var b = (hash / (200 * 200)) % 200 + 30;
            dot.Fill = new SolidColorBrush(Color.FromRgb((byte)r, (byte)g, (byte)b));
        }

        internal void MoveEast()
        {
            if (X < World.GridSize - 1 && World.Grid[X + 1, Y] == -1)
            {
                World.Grid[X, Y] = -1;
                X++;
                World.Grid[X, Y] = Id;
            }
        }

        internal void MoveNorth()
        {
            if (Y > 0 && World.Grid[X, Y - 1] == -1)
            {
                World.Grid[X, Y] = -1;
                Y--;
                World.Grid[X, Y] = Id;
            }
        }

        internal void MoveSouth()
        {
            if (Y < World.GridSize - 1 && World.Grid[X, Y + 1] == -1)
            {
                World.Grid[X, Y] = -1;
                Y++;
                World.Grid[X, Y] = Id;
            }
        }

        internal void MoveWest()
        {
            if (X > 0 && World.Grid[X - 1, Y] == -1)
            {
                World.Grid[X, Y] = -1;
                X--;
                World.Grid[X, Y] = Id;
            }
        }

        internal static Critter FromDNA(string dna)
        {
            var result = new Critter();
            var dnaConnections = dna.Split(' ').ToList();
            result.InternalNeurons = int.Parse(dnaConnections[0], System.Globalization.NumberStyles.HexNumber);

            result.Neurons.AddRange(inputs.Select(t => (Neuron)t.GetConstructor(BindingFlags.Public | BindingFlags.Instance, Array.Empty<Type>()).Invoke(Array.Empty<object>())));
            result.Neurons.AddRange(outputs.Select(t => (Neuron)t.GetConstructor(BindingFlags.Public | BindingFlags.Instance, Array.Empty<Type>()).Invoke(Array.Empty<object>())));
            result.Neurons.AddRange(Enumerable.Range(0, result.InternalNeurons).Select(_ => new InternalNeuron()));
            result.Neurons.ForEach(n => n.Critter = result);

            foreach (var d in dnaConnections.Skip(1))
            {
                var idFrom = int.Parse(d.Substring(0, 3), System.Globalization.NumberStyles.HexNumber);
                var idTo = int.Parse(d.Substring(3, 3), System.Globalization.NumberStyles.HexNumber);
                var intensity = (((double)int.Parse(d.Substring(6, 4), System.Globalization.NumberStyles.HexNumber)) - 4000) / 4000.0;
                result.Neurons[idTo].Connect(result.Neurons[idFrom], intensity);
            }
            return result;
        }

        internal void Execute()
        {
            foreach (var n in Neurons.OfType<InputNeuron>().Where(row => row.IsConnected).Cast<InputNeuron>())
                n.StoreCache();

            foreach (var n in Neurons.OfType<OutputNeuron>().Where(row => row.HasConnections).Cast<OutputNeuron>())
                n.Execute();
        }
    }
}
