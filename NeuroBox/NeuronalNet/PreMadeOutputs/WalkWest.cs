namespace NeuroBox.NeuronalNet.PreMadeOutputs
{
    internal class WalkWest : OutputNeuron
    {
        public override void Action() => Critter.MoveWest();
    }
}
