namespace NeurBox.NeuronalNet.PreMadeInputs
{
    internal class NearNorth : InputNeuron
    {
        public override double Input()
        {
            return (double)Critter.Y / (double)Critter.World.GridSize;
        }
    }
}
