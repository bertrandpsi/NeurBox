using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeurBox.NeuronalNet
{
    internal class NeuronalConnection
    {
        public double Intensity { get; set; }
        public Neuron From { get; set; }
        public Neuron To { get; set; }

        public double Calculate()
        {
            throw new NotImplementedException();
        }
    }
}
