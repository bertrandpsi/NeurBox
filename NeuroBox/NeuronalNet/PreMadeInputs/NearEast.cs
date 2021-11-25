namespace NeuroBox.NeuronalNet.PreMadeInputs
{
    internal class NearEast : InputNeuron
    {
        public override double Input()
        {
            return 1.0 - (double)Critter.X / (double)Critter.World.GridSize;
        }
    }

}
