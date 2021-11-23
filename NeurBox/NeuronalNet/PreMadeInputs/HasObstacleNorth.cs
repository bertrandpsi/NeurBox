using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeurBox.NeuronalNet.PreMadeInputs
{
    internal class HasObstacleNorth : InputNeuron
    {
        public override double Input()
        {
            for (var i = Critter.Y - 1; i >= 0 && i < Critter.Y - 20; i++)
                if (Critter.World.Grid[Critter.X, i] != 0)
                    return 1.0 - (Critter.Y - i) / 20.0;
            return 0.0;
        }
    }
}
