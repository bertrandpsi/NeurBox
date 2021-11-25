namespace NeurBox.NeuronalNet.PreMadeInputs
{
    internal class HasObstacleSouth : InputNeuron
    {
        public override double Input()
        {
            for (var i = Critter.Y + 1; i < Critter.World.GridSize && i < Critter.Y + 20; i++)
                if (Critter.World.Grid[Critter.X, i] != 0)
                    return 1.0 - (i - Critter.Y) / 20.0;
            return 0.0;
        }
    }
}
