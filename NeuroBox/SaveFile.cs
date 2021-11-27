using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace NeuroBox
{
    public class SimultationParameters
    {
        [XmlAttribute]
        public int LifeSpan { get; set; }
        [XmlAttribute]
        public int InternalNeurons { get; set; }
        [XmlAttribute]
        public int NetworkConnections { get; set; }
        [XmlAttribute]
        public int GridSize { get; set; }
        [XmlAttribute]
        public int NumberCritter { get; set; }
        [XmlAttribute]
        public double MutationRate { get; set; }
        [XmlAttribute]
        public double MinReproductionFactor { get; set; }
        [XmlAttribute]
        public bool DnaMixing { get; set; }
    }

    [XmlRoot("Simulation")]
    public class SaveFile
    {
        [XmlAnyElement("FileInformation")]
        public XmlComment VersionComment { get { return new XmlDocument().CreateComment("This file contains the simultaion settings for NeuroBox."); } set { } }

        public SimultationParameters Parameters { get; set; } = new SimultationParameters();

        public string SelectionCondition { get; set; }
        public string SpawnCoordinate { get; set; }
        public string WorldBlocking { get; set; }
    }
}
