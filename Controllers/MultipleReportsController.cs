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
            List<SelfReport> averageList = new List<SelfReport>();
            List<string> listOfFileNames = new List<string>();
            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\files", fileName); 

                using (var stream = System.IO.File.Create(filePath))
                {
                    file.CopyTo(stream);    
                }

           
                
                var error = this.validateFiles(fileName);
                if (!string.IsNullOrEmpty(error))
                {
                    TempData["Message"] = error;
                    System.IO.File.Delete(filePath);
                    return RedirectToAction("Index");
                }
                
                listOfFileNames.Add(fileName);
            }
          

            selfReportsDic = this.GetListOfSelfReport(listOfFileNames);
            averageList = this.CalculateAverageValue(selfReportsDic);
            //assign selfReportsDictionary and averageSelfReport with value calculated above
            return Index(new MultipleReports() { selfReportsDictionary = selfReportsDic, averageSelfReport = averageList }) ;
        }
        
        private string validateFiles(string file)
        {
            string error = "";

            //var config = CsvConfiguration.FromAttributes<GSR>();
            #region Read CSV
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\files", file);

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {

                csv.Read();
                csv.ReadHeader();
                string header = csv.HeaderRecord[2].ToLowerInvariant();
                header = header.Replace("-", "");
                string selfReportModel = nameof(SelfReport.selfReport).ToLowerInvariant();


                if (header != selfReportModel)
                {
                    error = "Header is not matched, Please upload correct CSV file";
                }
            }
            #endregion


            return error;
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


        private List<SelfReport> CalculateAverageValue(Dictionary<int, List<SelfReport>> multipleSelfReportDictionary)
        {
            List<SelfReport> newList = new List<SelfReport>();
            List<List<SelfReport>> listOfList = new List<List<SelfReport>>();


            //Loop through the map to add List<SelfReport> to the listOfList
            foreach (KeyValuePair<int, List<SelfReport>> kvp in multipleSelfReportDictionary)
            {
                listOfList.Add(kvp.Value);
            }

            //Initalize the size of every List<SelfReport> to have the same
            //length with the first list in listOfList
            int listSize = listOfList[0].Count;

            //Calculate the average of corresponding elements in the list 
            for (int i = 0; i < listSize; i++)
            {
                DateTime averageTimeStamp = DateTime.Today;
                int totalSegment = 0;
                double totalSelfReport = 0;

                //loop each list in the listOfList to get the corresponding
                //row index to add to total value
                foreach (var list in listOfList)
                {
                    totalSegment += list[i].segment;
                    totalSelfReport += list[i].selfReport;
                }

                //Calculate the average
                int averageSegment = totalSegment / listOfList.Count;
                double averageSelfReport = totalSelfReport / listOfList.Count;

                //Create SelfReport objects to add to the list
                SelfReport average = new SelfReport
                {
                    timeStamp = averageTimeStamp,
                    segment = averageSegment,
                    selfReport = averageSelfReport

                };
                newList.Add(average);
            }
           

            //return it to index page
            return newList;
        }
    }
}
