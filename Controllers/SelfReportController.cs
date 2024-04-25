using CsvHelper;
using CsvHelper.Configuration;
using ICTCapstoneProject.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace ICTCapstoneProject.Controllers
{
    public class SelfReportController : Controller
    {

        [HttpGet]
        public IActionResult Index(List<SelfReport> selfReports)
        {
        
            if (selfReports == null)
            {
                selfReports = new List<SelfReport>();
            }
            else
            {
                selfReports = selfReports.ToList();
            }

            return View(selfReports);
        }
       
        /*
        [HttpPost]
        public IActionResult Index(List<IFormFile> file)
        {
            List<SelfReport> selfReport = new List<SelfReport>();
            int count = 0;
            foreach (var fileItem in file)
            {
                string permittedExtension = ".csv";
                var extension = Path.GetExtension(fileItem.FileName);
                //Get the file name
                var fileName = Path.GetFileNameWithoutExtension(fileItem.FileName);
                //Get the Path and store the fileName under files folder 
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\files", fileName);
                if (extension != permittedExtension)
                {
                    TempData["Message"] = "Please Upload Only .csv file permitted";
                    return RedirectToAction("Index");
                }

                else
                {   
                    if(fileName == fileItem.FileName)
                    {
                        count++;
                        fileName = fileName + count + extension;
                        using (var stream = System.IO.File.Create(filePath))
                        {
                            fileItem.CopyTo(stream);

                        }

                        //selfReport = this.GetListOfSelfReport(file);
                    }
                   
                   
                }
            }
            return View(selfReport);
        }
        */
        /*
        private List<SelfReport> GetListOfSelfReport(List<IFormFile> file)
        {

            //Dictionary<int, List<SelfReport>> selfReportDictionary = new Dictionary<int, List<SelfReport>>();

            throw new NotImplementedException();
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


            if (extension != permittedExtension)
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

                var selfReports = this.GetSelfReportList(fileName);
                return Index(selfReports);
            }

        }
        
        private string validateFile(string fileName)
        {
            string error = "";

            //var config = CsvConfiguration.FromAttributes<GSR>();
            #region Read CSV
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\files", fileName);

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {

                csv.Read();
                csv.ReadHeader();
                string header = csv.HeaderRecord[2].ToLowerInvariant();
                header = header.Replace("-", "");
                string timeStampModel = nameof(SelfReport.selfReport).ToLowerInvariant();
                

                if (header != timeStampModel)
                {
                    error = "Header is not matched, Please upload correct CSV file";
                }
            }
            #endregion


            return error;
        }

        private List<SelfReport> GetSelfReportList(string fileName)
        {
            List<SelfReport> selfReports = new List<SelfReport>();

            #region Read CSV
            var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files");
            var filePath = Path.Combine(directoryPath, fileName);

            //var path = $"{Directory.GetCurrentDirectory()}{@"\wwwroot\files"}" + "\\" + fileName;
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
             
                    csv.Read();
                    csv.ReadHeader();
                    while (csv.Read())
                    {
                        var report = csv.GetRecord<SelfReport>();
                        selfReports.Add(report);
                    }
                
               
            }
            #endregion
            return selfReports;
        }
    }
}
