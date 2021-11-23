using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeurBox.NeuronalNet.PreMadeOutputs
{
    internal class WalkNorth : OutputNeuron
    {
        public override void Action() => Critter.MoveNorth();
    }
}
