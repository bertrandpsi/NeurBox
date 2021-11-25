namespace NeuroBox.NeuronalNet.PreMadeInputs
{
    internal class NearWest : InputNeuron
    {
        public override double Input()
        {
            return (double)Critter.X / (double)Critter.World.GridSize;
        }
    }
}
