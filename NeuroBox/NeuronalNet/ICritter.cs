namespace NeuroBox.NeuronalNet
{
    public interface ICritter
    {
        public int MaxLifeSpan { get; }
        public int LifeSpan { get; }
        public int InternalNeurons { get; }
        public int NetworkConnections { get; }
        public int X { get; }
        public int Y { get; }
        public int GridSize { get; }
        public int Id { get; }
    }
}