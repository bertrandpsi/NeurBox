using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeurBox.NeuronalNet.PreMadeInputs
{
    internal class NearSouth : InputNeuron
    {
        public override double Input()
        {
            return (double)Critter.Y / (double)Critter.World.GridSize;
        }
    }
}
