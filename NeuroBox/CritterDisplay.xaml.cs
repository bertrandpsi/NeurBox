using NeuroBox.NeuronalNet;
using System.Windows.Controls;
using System.Windows.Media;

namespace NeuroBox
{
    /// <summary>
    /// Interaction logic for Critter.xaml
    /// </summary>
    public partial class CritterDisplay : UserControl
    {
        public CritterDisplay()
        {
            InitializeComponent();
        }

        public int X => Critter.X;
        public int Y => Critter.Y;

        public Critter Critter { get; set; }

        Color ColorFromString(string src)
        {
            var hash = src.Substring(0, 6).GetHashCode();
            var r = Math.Min(255, (hash % 250) * 2.2);
            var g = Math.Min(255, ((hash / 250) % 250) * 2.2);
            var b = Math.Min(255, ((hash / (250 * 250)) % 250) * 2.2);
            return Color.FromRgb((byte)r, (byte)g, (byte)b);
        }



        public void CalculateColor()
        {
            var colors = Critter.DNA.Split(' ').Skip(1).Select(d => ColorFromString(d));
            dot.Fill = new SolidColorBrush(Color.FromRgb((byte)colors.Average(c => c.R), (byte)colors.Average(c => c.G), (byte)colors.Average(c => c.B)));
        }
    }
}
