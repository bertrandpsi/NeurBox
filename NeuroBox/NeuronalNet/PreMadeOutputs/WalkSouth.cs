namespace NeuroBox.NeuronalNet.PreMadeOutputs
{
    internal class WalkSouth : OutputNeuron
    {
        public override void Action() => Critter.MoveSouth();
    }
}
