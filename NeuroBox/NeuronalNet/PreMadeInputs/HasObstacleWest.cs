namespace NeuroBox.NeuronalNet.PreMadeInputs
{
    internal class HasObstacleWest : InputNeuron
    {
        public override double Input()
        {
            for (var i = Critter.X - 1; i >= 0 && i > Critter.X - 20; i--)
                if (Critter.World.Grid[i, Critter.Y] != 0)
                    return 1.0 - (Critter.X - i) / 20.0;
            return 0.0;
        }
    }
}
