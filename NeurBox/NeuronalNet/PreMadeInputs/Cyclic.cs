using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeurBox.NeuronalNet.PreMadeInputs
{
    internal class Cyclic : InputNeuron
    {
        public override double Input()
        {
            return Math.Sin(Critter.LifeSpan / 10.0);
        }
    }
}
