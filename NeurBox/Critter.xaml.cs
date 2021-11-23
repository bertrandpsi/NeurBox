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
            // Creates all the neurons
            Neurons.AddRange(inputs.Select(t => (Neuron)t.GetConstructor(BindingFlags.Public | BindingFlags.Instance, Array.Empty<Type>()).Invoke(Array.Empty<object>())));
            Neurons.AddRange(outputs.Select(t => (Neuron)t.GetConstructor(BindingFlags.Public | BindingFlags.Instance, Array.Empty<Type>()).Invoke(Array.Empty<object>())));
            Neurons.AddRange(Enumerable.Range(0, InternalNeurons).Select(_ => new InternalNeuron()));
            Neurons.ForEach(n => n.Critter = this);

            var nonInputs = Neurons.Skip(inputs.Count).ToList();
            var nonOuputs = Neurons.Where(n => !(n is OutputNeuron)).ToList();

            // Create all the connections
            for (var i = 0; i < NetworkConnections; )
            {
                var a = nonInputs[WorldGrid.Random.Next(nonInputs.Count)];
                var b = nonOuputs[WorldGrid.Random.Next(nonOuputs.Count)];
                if (a.AlreadyConnectedWith(b))
                    continue;
                a.Connect(b, WorldGrid.Random.NextDouble() * 0.8 + 0.2);
                i++;
            }
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
            if (X > 0 && World.Grid[X + 1, Y] == -1)
            {
                World.Grid[X, Y] = -1;
                X--;
                World.Grid[X, Y] = Id;
            }
        }

        internal void Execute()
        {
            foreach (var n in Neurons.Where(row => row is InputNeuron && row.IsConnected).Cast<InputNeuron>())
                n.StoreCache();

            foreach (var n in Neurons.Where(row => row is OutputNeuron && row.HasConnections).Cast<OutputNeuron>())
                n.Execute();
        }
    }
}
