using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using ICTCapstoneProject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ICTCapstoneProject.Controllers
{
    public class SelfReportController : Controller
    {
        [HttpGet]
        public IActionResult Index(List<SingleSelfReport>? selfReports = null)
        {
            if (selfReports == null)
            {
                selfReports = new List<SingleSelfReport>();
            }
            return View(selfReports);
        }

        /// <summary>
        /// Recevied the file from view 
        /// now process 
        /// </summary>
        /// <param name="baselineFiles"></param>
        /// <param name="passiveFiles"></param>
        /// <param name="activeFiles"></param>
        /// <returns>
        /// Return the list of selfReport to index page in view 
        /// </returns>
        [HttpPost]
        public IActionResult Index(List<IFormFile> baselineFiles, List<IFormFile> passiveFiles, List<IFormFile> activeFiles)
        {
            List<SingleSelfReport> selfReports = new List<SingleSelfReport>();

            if (baselineFiles != null && baselineFiles.Count > 0)
            {
                selfReports.AddRange(ProcessFiles(baselineFiles, "Baseline"));
            }

            if (passiveFiles != null && passiveFiles.Count > 0)
            {
                selfReports.AddRange(ProcessFiles(passiveFiles, "Passive"));
            }

            if (activeFiles != null && activeFiles.Count > 0)
            {
                selfReports.AddRange(ProcessFiles(activeFiles, "Active"));
            }

            return Index(selfReports);
        }

       
        private List<SingleSelfReport> ProcessFiles(List<IFormFile> files, string sceneType)
        {
            List<SingleSelfReport> selfReports = new List<SingleSelfReport>();
            foreach (var file in files)
            {
                string permittedExtension = ".csv";
                var extension = Path.GetExtension(file.FileName);
                var fileName = Path.GetFileName(file.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files", fileName);

                if (extension != permittedExtension)
                {
                    TempData["Message"] = "Please Upload Only .csv file permitted";
                    continue;
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                var error = ValidateFile(filePath);
                if (!string.IsNullOrEmpty(error))
                {
                    TempData["Message"] = error;
                    System.IO.File.Delete(filePath);
                    continue;
                }

                var fileSelfReports = GetSelfReportList(filePath, sceneType);
                selfReports.AddRange(fileSelfReports);
            }
            return selfReports;
        }

        private string ValidateFile(string filePath)
        {
            string error = "";
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                MissingFieldFound = null
            }))
            {
                csv.Read();
                csv.ReadHeader();
                string header = csv.HeaderRecord[2].ToLowerInvariant().Replace("-", "");
                string timeStampModel = nameof(SingleSelfReport.selfReport).ToLowerInvariant();

                if (header != timeStampModel)
                {
                    error = "Header is not matched, Please upload correct CSV file";
                }
            }
            return error;
        }

        private List<SingleSelfReport> GetSelfReportList(string filePath, string sceneType)
        {
            List<SingleSelfReport> selfReports = new List<SingleSelfReport>();
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                MissingFieldFound = null
            }))
            {
                csv.Read();
                csv.ReadHeader();
                while (csv.Read())
                {
                    var report = csv.GetRecord<SingleSelfReport>();
                    report.sceneType = sceneType;
                    selfReports.Add(report);
                }
            }
            return selfReports;
        }
    }
}
