namespace NeurBox.NeuronalNet.PreMadeOutputs
{
    internal class WalkEast : OutputNeuron
    {
        public override void Action() => Critter.MoveEast();
    }
}
