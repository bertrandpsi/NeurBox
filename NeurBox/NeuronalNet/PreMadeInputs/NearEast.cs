﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeurBox.NeuronalNet.PreMadeInputs
{
    internal class NearEast : InputNeuron
    {
        public override double Input()
        {
            return 1.0 - (double)Critter.X / (double)Critter.World.GridSize;
        }
    }

}
