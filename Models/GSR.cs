using CsvHelper.Configuration.Attributes;
using System.Numerics;

namespace ICTCapstoneProject.Models
{
    public class GSR
    {
        [Index(0)]
        public float ms { get; set; }
        [Index(1)]
        public int noUnits { get; set; }
        [Index(2)]
        public float uS { get; set; }
        [Index(3)]
        public float kOhms { get; set; }
        [Index(4)]
        public float mV { get; set; }
    }
}
