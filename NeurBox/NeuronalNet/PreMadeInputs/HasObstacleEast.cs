using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeurBox.NeuronalNet.PreMadeInputs
{
    internal class HasObstacleEast : InputNeuron
    {
        public override double Input()
        {
            for (var i = Critter.X + 1; i < Critter.World.GridSize && i < Critter.X + 20; i++)
                if (Critter.World.Grid[i, Critter.Y] != 0)
                    return 1.0 - (i - Critter.X) / 20.0;
            return 0.0;
        }
    }
}
