﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeurBox.NeuronalNet
{
    internal abstract class OutputNeuron : Neuron
    {
        public abstract void Execute();
    }
}
