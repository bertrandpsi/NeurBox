using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuroBox.NeuronalNet.PreMadeInputs
{
    class CrittersNearby : InputNeuron
    {
        public override double Input()
        {
            var nb = 0;
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    var a = Critter.X - 5 + i;
                    var b = Critter.Y - 5 + j;
                    if (a < 0 || b < 0 || a >= Critter.World.GridSize || b >= Critter.World.GridSize)
                        continue;
                    if (Critter.World.Grid[a, b] >= 0)
                        nb++;
                }
            }
            return nb / 10.0;
        }
    }
}
