namespace NeuroBox.NeuronalNet.PreMadeInputs
{
    internal class Cyclic10 : InputNeuron
    {
        public override double Input()
        {
            return Math.Sin(Critter.LifeSpan / 10.0);
        }
    }
}
