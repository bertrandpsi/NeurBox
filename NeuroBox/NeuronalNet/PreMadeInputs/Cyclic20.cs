namespace NeuroBox.NeuronalNet.PreMadeInputs
{
    internal class Cyclic20 : InputNeuron
    {
        public override double Input()
        {
            return Math.Sin(Critter.LifeSpan / 20.0);
        }
    }
}
