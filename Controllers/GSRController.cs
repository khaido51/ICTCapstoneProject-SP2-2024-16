﻿using CsvHelper;
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



        /*
        [HttpPost]
        public IActionResult Index(IFormFile file, [FromServices] IWebHostEnvironment hostingEnvironment)
        {
            #region Upload CSV
            string fileName = $"{hostingEnvironment.WebRootPath}\\files\\{file.FileName}";
            using (FileStream fileStream = System.IO.File.Create(fileName))
            {
                file.CopyTo(fileStream);
                fileStream.Flush();
            }
            #endregion

            var gsrReport = this.GetGsrReportList(fileName);


            return Index(gsrReport);
        }
        */


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
            List<GSR> gsrReport = new List<GSR>();

            //var config = CsvConfiguration.FromAttributes<GSR>();
            #region Read CSV
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\files", fileName);
            var config = new CsvConfiguration(CultureInfo.CurrentCulture) { Delimiter = "\t" };
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, config))
            {

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

            //var config = CsvConfiguration.FromAttributes<GSR>();
            #region Read CSV
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\files", fileName);
            var config = new CsvConfiguration(CultureInfo.CurrentCulture) { Delimiter = "\t" };
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, config))
            {
                
                csv.Read();
                csv.ReadHeader();
                string test = csv.HeaderRecord[0];
                while (csv.Read())
                {
                   
                    var gsr = csv.GetRecord<GSR>();
                    //string header = csv.HeaderRecord.ToString();
                    gsr.test = double.Parse(gsr.ms);
                    gsr.tSpan = TimeSpan.FromMilliseconds(gsr.test);
                    gsr.totalSeconds = gsr.tSpan.TotalMilliseconds;
                    gsr.millisecs = gsr.totalSeconds.ToString();
                    //gsr.totalSeconds = gsr.tSpan.TotalMinutes;
                    
                    //double.Parse("1.50E-15", CultureInfo.InvariantCulture)
                    //gsr.ms = Convert.ToDecimal(csv.GetField("ms"), CultureInfo.InvariantCulture);
                    gsrReport.Add(gsr);
                }
            }
            #endregion
            return gsrReport;
        }
       
    }
}
