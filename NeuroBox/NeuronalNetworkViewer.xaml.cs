using NeuroBox.NeuronalNet;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NeuroBox
{
    /// <summary>
    /// Interaction logic for NeuronalNetworkViewer.xaml
    /// </summary>
    public partial class NeuronalNetworkViewer : UserControl
    {
        public NeuronalNetworkViewer()
        {
            InitializeComponent();
        }

        internal void Render(List<WorldGrid.TopUsage> topMostUsed)
        {
            networkPreview.Children.Clear();
            var pos = 0;
            foreach (var t in topMostUsed.ToList())
            {
                var title = new TextBlock { Text = "Position " + (pos + 1) + " - " + (t.Frequency * 100).ToString("F2") + "%", Width = double.NaN, TextAlignment = TextAlignment.Left, FontSize = 20, FontWeight = FontWeights.Bold, Margin = new Thickness(20) };
                networkPreview.Children.Add(title);
                networkPreview.Children.Add(DrawNeuronalNet(t.Specimen));
                pos++;
            }
        }

        private Canvas DrawNeuronalNet(Critter critter)
        {
            var canvas = new Canvas { Height = 300 };
            var pos = new int[] { 10, 10, 10 };
            var visitedNode = new List<Neuron>();
            var toVisit = new Queue<Neuron>();

            // All the output nodes
            var usedOutputs = critter.Neurons.OfType<OutputNeuron>().Where(row => row.HasConnections);
            toVisit = new Queue<Neuron>(usedOutputs);
            visitedNode.AddRange(usedOutputs);
            var connections = new List<NeuronalConnection>();
            var neuronLookup = new Dictionary<Neuron, Grid>();

            // Draw all the neurons
            while (toVisit.Count > 0)
            {
                var n = toVisit.Dequeue();

                var g = new Grid();
                neuronLookup.Add(n, g);
                var e = new Ellipse();
                g.Children.Add(e);
                var t = new TextBlock { Text = TypeTitle(n) };
                g.Children.Add(t);
                canvas.Children.Add(g);
                if (n is OutputNeuron)
                {
                    g.SetValue(Canvas.LeftProperty, (double)pos[2]);
                    g.SetValue(Canvas.TopProperty, (double)220);
                    e.Fill = new SolidColorBrush(Colors.LightCoral);
                    pos[2] += 110;
                }
                else if (n is InputNeuron)
                {
                    g.SetValue(Canvas.LeftProperty, (double)pos[0]);
                    g.SetValue(Canvas.TopProperty, (double)10);
                    e.Fill = new SolidColorBrush(Colors.SkyBlue);
                    pos[0] += 110;
                }
                else
                {
                    g.SetValue(Canvas.LeftProperty, (double)pos[1]);
                    g.SetValue(Canvas.TopProperty, (double)110);
                    e.Fill = new SolidColorBrush(Colors.LightSteelBlue);
                    pos[1] += 110;
                }
                foreach (var c in n.Connections)
                {
                    connections.Add(c);
                    if (visitedNode.Contains(c.From))
                        continue;
                    visitedNode.Add(c.From);
                    toVisit.Enqueue(c.From);
                }
            }

            // Draw all the connections
            foreach (var c in connections)
            {
                if (!neuronLookup.ContainsKey(c.From) || !neuronLookup.ContainsKey(c.To))
                    continue;
                if (c.From == c.To)
                {
                    var l = new Path();
                    var d = new PathGeometry();
                    l.Data = d;
                    var p = new PathFigure();
                    d.Figures.Add(p);
                    var s = new BezierSegment();
                    p.Segments.Add(s);
                    p.StartPoint = new Point((double)neuronLookup[c.From].GetValue(Canvas.LeftProperty) + 35.0, (double)neuronLookup[c.From].GetValue(Canvas.TopProperty) + 35.0);
                    s.Point1 = new Point((double)neuronLookup[c.From].GetValue(Canvas.LeftProperty) + 135.0, (double)neuronLookup[c.From].GetValue(Canvas.TopProperty) + 35.0);
                    s.Point2 = new Point((double)neuronLookup[c.To].GetValue(Canvas.LeftProperty) + 35.0, (double)neuronLookup[c.To].GetValue(Canvas.TopProperty) + 135.0);
                    s.Point3 = new Point((double)neuronLookup[c.To].GetValue(Canvas.LeftProperty) + 35.0, (double)neuronLookup[c.To].GetValue(Canvas.TopProperty) + 35.0);
                    l.StrokeThickness = Math.Max(0.5, Math.Abs(c.Intensity) * 3);
                    l.Stroke = c.Intensity < 0 ? Brushes.Red : Brushes.Green;
                    canvas.Children.Insert(0, l);
                }
                else
                {
                    var l = new Path();
                    var d = new PathGeometry();
                    l.Data = d;
                    var p = new PathFigure();
                    d.Figures.Add(p);
                    var s = new BezierSegment();
                    p.Segments.Add(s);
                    p.StartPoint = new Point((double)neuronLookup[c.From].GetValue(Canvas.LeftProperty) + 35.0, (double)neuronLookup[c.From].GetValue(Canvas.TopProperty) + 35.0);
                    s.Point1 = new Point((double)neuronLookup[c.From].GetValue(Canvas.LeftProperty) + 135.0, (double)neuronLookup[c.From].GetValue(Canvas.TopProperty) + 35.0);
                    s.Point2 = new Point((double)neuronLookup[c.To].GetValue(Canvas.LeftProperty) + 135.0, (double)neuronLookup[c.To].GetValue(Canvas.TopProperty) + 35.0);
                    s.Point3 = new Point((double)neuronLookup[c.To].GetValue(Canvas.LeftProperty) + 35.0, (double)neuronLookup[c.To].GetValue(Canvas.TopProperty) + 35.0);
                    l.StrokeThickness = Math.Max(0.5, Math.Abs(c.Intensity) * 3);
                    l.Stroke = c.Intensity < 0 ? Brushes.Red : Brushes.Green;
                    canvas.Children.Insert(0, l);
                }
            }
            canvas.Width = pos.Max() + 110;

            return canvas;
        }

        static Regex titleExp = new Regex("([a-z])([A-Z])");
        private string TypeTitle(object o) => titleExp.Replace(o.GetType().Name, "$1 $2");
    }
}
