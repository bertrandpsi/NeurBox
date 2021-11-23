using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeurBox.NeuronalNet.PreMadeInputs
{
    internal class RandomInput : InputNeuron
    {
        public override double Input()
        {
            return WorldGrid.Random.NextDouble() * 2.0 - 1;
        }
    }
}
