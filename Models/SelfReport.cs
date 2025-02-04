﻿using CsvHelper.Configuration.Attributes;

namespace ICTCapstoneProject.Models
{
    public class SelfReport
    {
        [Index(0)]
        public DateTime timeStamp { get; set; }   
        
        [Index(1)]
        public int segment { get; set; }

        [Index(2)]
        public double selfReport {get; set;}
    }
}
