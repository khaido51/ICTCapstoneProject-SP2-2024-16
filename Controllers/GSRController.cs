using CsvHelper;
using CsvHelper.Configuration;
using ICTCapstoneProject.Models;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Numerics;
using static System.Net.Mime.MediaTypeNames;

namespace ICTCapstoneProject.Controllers
{
    public class GSRController : Controller
    {
   
        [HttpGet]
        public IActionResult Index(List<GSR> gsrReport)
        {
            if (gsrReport == null)
            {
                gsrReport = new List<GSR>();
            }
            else
            {
                gsrReport.ToList();
            }
           
            return View(gsrReport);
        }


        [HttpPost]
        public IActionResult Index(IFormFile file)
        {
            string permittedExtension = ".csv";
            var extension = Path.GetExtension(file.FileName);
            //Get the file name
            var fileName = Path.GetFileName(file.FileName);
            //Get the Path and store the fileName under files folder 
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\files", fileName);
            
           
            if(extension != permittedExtension)
            {
                TempData["Message"] = "Please Upload Only .csv file permitted";
                return RedirectToAction("Index");
            }
            
            else
            {
                using (var stream = System.IO.File.Create(filePath))
                {
                    file.CopyTo(stream);
                    
                }
                
                //validate csv structure
                var error = this.validateFile(fileName);
                if (!string.IsNullOrEmpty(error))
                {
                    TempData["Message"] = error;
                    System.IO.File.Delete(filePath);
                    return RedirectToAction("Index");
                }
                
                var gsrReport = this.GetGsrReportList(fileName);
                return Index(gsrReport);
            }
           
        }

        private string validateFile(string fileName)
        {
            string error = "";
       

            //var config = CsvConfiguration.FromAttributes<GSR>();
            #region Read CSV
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\files", fileName);
            var config = new CsvConfiguration(CultureInfo.CurrentCulture) { Delimiter = "\t" };
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, config))
            {
                csv.Read();
                csv.Read();
                csv.Read();       
                csv.ReadHeader();
                string header = csv.HeaderRecord[0];
                string msModel = nameof(GSR.ms);
                if (header != msModel) 
                {
                    error = "Header is not matched, Please upload correct CSV file";
                }
            }
            #endregion
         
            
            return error;
        }

        private List<GSR> GetGsrReportList(string fileName)
        {
            List<GSR> gsrReport = new List<GSR>();
            double countSample = 32130;
            double value = 0;
            //var config = CsvConfiguration.FromAttributes<GSR>();
            #region Read CSV
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\files", fileName);
            var config = new CsvConfiguration(CultureInfo.CurrentCulture) { Delimiter = "\t" };
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, config))
            {
                csv.Read();
                csv.Read();
                csv.Read();
                csv.ReadHeader();
                string test = csv.HeaderRecord[0];
               
                while (csv.Read())
                {
                   
                    var gsr = csv.GetRecord<GSR>();
                    //string header = csv.HeaderRecord.ToString();
                    gsr.test = double.Parse(gsr.ms);
                    gsr.tSpan = TimeSpan.FromMilliseconds(gsr.test);
                    //gsr.xValue = gsr.xValue + (1 / 51.2);
                    gsr.xValue = value;
                    gsr.xValueTspan = TimeSpan.FromSeconds(gsr.xValue);                  
                    gsr.totalSeconds = gsr.tSpan.TotalMilliseconds;
                    gsr.millisecs = gsr.totalSeconds.ToString();          
                    gsrReport.Add(gsr);
                    value += (1 / 51.2);
                }
            }
            #endregion
            return gsrReport;
        }
       
    }
}
