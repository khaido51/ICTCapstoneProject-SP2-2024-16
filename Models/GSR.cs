using CsvHelper.Configuration.Attributes;
using System.Numerics;

namespace ICTCapstoneProject.Models
{
    //[Delimiter("\t")]
    //[CultureInfo("en-us")]
    public class GSR
    {
        [Index(0)]
        public string ms { get; set; }
        [Index(1)]
        public double noUnits { get; set; }
        [Index(2)]
        public float uS { get; set; }
        [Index(3)]
        public float kOhms { get; set; }
        [Index(4)]
        public float mV { get; set; }

        [Ignore]
        public double test { get; set; }

        [Ignore]
        public TimeSpan tSpan { get; set; }

        [Ignore]
        public double totalSeconds { get; set; }

        [Ignore]
        public string millisecs { get; set; }
    }


}
