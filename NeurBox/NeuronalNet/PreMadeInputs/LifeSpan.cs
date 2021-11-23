using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeurBox.NeuronalNet.PreMadeInputs
{
    internal class LifeSpan : InputNeuron
    {
        public override double Input()
        {
            return (double)Critter.LifeSpan / (double)Critter.MaxLifeSpan;
        }
    }
}
