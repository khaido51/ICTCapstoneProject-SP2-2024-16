using CsvHelper;
using ICTCapstoneProject.Models;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace ICTCapstoneProject.Controllers
{
    public class MultipleReportsController : Controller
    {
        public IActionResult Index(MultipleReports model)
        {
            return View(model);
        }

        [HttpPost]
        public IActionResult Index(List<IFormFile> files)
        {
            //List<SelfReport> selfReports = new List<SelfReport>();
            Dictionary<int, List<SelfReport>> selfReportsDic= new Dictionary<int, List<SelfReport>>();
            List<string> listOfFileNames = new List<string>();
            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\files", fileName); 

                using (var stream = System.IO.File.Create(filePath))
                {
                    file.CopyTo(stream);    
                }
                
               
                listOfFileNames.Add(fileName);
               
                

            }
            selfReportsDic = this.GetListOfSelfReport(listOfFileNames);
            return Index(new MultipleReports() { selfReportsDictionary = selfReportsDic });
        }

       

        private  Dictionary<int, List<SelfReport>> GetListOfSelfReport(List<string> listOfFileNames)
        {
            int count = 0;
            List<SelfReport> selfReports = new List<SelfReport>();
            Dictionary<int, List<SelfReport>> selfReportsDictionary = new Dictionary<int, List<SelfReport>>();  
            foreach(var file in listOfFileNames)
            {
               
                #region readCSV
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\files", file);

                using(var reader = new StreamReader(filePath))
                using(var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    selfReports = new List<SelfReport>();
                    
                    csv.Read(); 
                    csv.ReadHeader();
                    while (csv.Read())
                    {
                       
                        var report = csv.GetRecord<SelfReport>();
                        selfReports.Add(report);
                        
                    }
                    count++;
                    selfReportsDictionary.Add(count, selfReports);
                }


                #endregion

            }
            return selfReportsDictionary;
        }

     
    }
}
