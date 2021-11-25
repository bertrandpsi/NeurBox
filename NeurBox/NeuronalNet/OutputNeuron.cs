namespace NeurBox.NeuronalNet
{
    internal abstract class OutputNeuron : Neuron
    {
        public void Execute()
        {
            if (GetValue(0) * WorldGrid.Random.NextDouble() > ActivationLimit)
                Action();
        }

        public abstract void Action();
    }
}
