namespace NeuroBox.NeuronalNet.PreMadeInputs
{
    internal class Cyclic : InputNeuron
    {
        public override double Input()
        {
            return Math.Sin(Critter.LifeSpan / 10.0);
        }
    }
}
