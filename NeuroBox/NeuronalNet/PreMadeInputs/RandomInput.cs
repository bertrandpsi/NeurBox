namespace NeuroBox.NeuronalNet.PreMadeInputs
{
    internal class RandomInput : InputNeuron
    {
        public override double Input()
        {
            return WorldGrid.Random.NextDouble() * 2.0 - 1;
        }
    }
}
