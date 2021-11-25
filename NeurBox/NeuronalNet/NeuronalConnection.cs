namespace NeurBox.NeuronalNet
{
    internal class NeuronalConnection
    {
        public double Intensity { get; set; }
        public Neuron From { get; set; }
        public Neuron To { get; set; }
        public string DNA => From.Position.ToString("X03") + To.Position.ToString("X03") + ((int)(Intensity * 4000) + 4000).ToString("X04");

        public double Calculate()
        {
            throw new NotImplementedException();
        }
    }
}
