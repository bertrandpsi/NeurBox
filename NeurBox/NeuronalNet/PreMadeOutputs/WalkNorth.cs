namespace NeurBox.NeuronalNet.PreMadeOutputs
{
    internal class WalkNorth : OutputNeuron
    {
        public override void Action() => Critter.MoveNorth();
    }
}
