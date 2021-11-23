using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeurBox.NeuronalNet
{
    internal abstract class Neuron
    {
        public const int MaxDepth = 10;

        public Critter Critter { get; set; }
        public List<NeuronalConnection> Connections { get; set; } = new List<NeuronalConnection>();
        public List<NeuronalConnection> ChildConnections { get; set; } = new List<NeuronalConnection>();
        public bool HasConnections => Connections.Count > 0;
        public double ActivationLimit { get; set; } = 0.8;
        public bool IsConnected => ChildConnections.Count > 0;

        public virtual double GetValue(int depth = 0)
        {
            if (depth >= MaxDepth)
                return 0;
            return InRange(Connections.Sum(row => row.From.GetValue(depth + 1) * row.Intensity));
        }

        private double InRange(double source)
        {
            return Math.Atan(source * 2);
        }

        internal void Connect(Neuron neuron, double intensity)
        {
            var conn = new NeuronalConnection { From = neuron, To = this, Intensity = intensity };
            Connections.Add(conn);
            neuron.ChildConnections.Add(conn);
        }

        internal bool AlreadyConnectedWith(Neuron neuron)
        {
            return Connections.Any(row => row.From == neuron) || ChildConnections.Any(row => row.To == neuron);
        }

        public int Position => Critter.Neurons.IndexOf(this);
    }
}
