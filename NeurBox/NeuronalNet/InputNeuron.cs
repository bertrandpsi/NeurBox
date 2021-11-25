﻿namespace NeurBox.NeuronalNet
{
    internal abstract class InputNeuron : Neuron
    {
        public abstract double Input();
        public void StoreCache()
        {
            cachedValue = Input();
        }

        double cachedValue = 0;
        public override double GetValue(int depth) => cachedValue;
    }
}
