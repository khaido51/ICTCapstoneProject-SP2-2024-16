﻿using Microsoft.AspNetCore.Mvc;

namespace ICTCapstoneProject.Models
{
    public class MultipleReports
    {
      public Dictionary<int, List<SelfReport>> selfReportsDictionary { get; set; }    
    }
}
