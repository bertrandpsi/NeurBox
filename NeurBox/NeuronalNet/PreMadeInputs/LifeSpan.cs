namespace NeurBox.NeuronalNet.PreMadeInputs
{
    internal class LifeSpan : InputNeuron
    {
        public override double Input()
        {
            return (double)Critter.LifeSpan / (double)Critter.MaxLifeSpan;
        }
    }
}
