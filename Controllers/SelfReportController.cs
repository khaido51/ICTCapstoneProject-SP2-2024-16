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
        /// <summary>
        /// Handle the GET request for SelfReport Index
        /// Initialize an empty list of SingleSelfReport
        /// </summary>
        /// <param name="selfReports">
        /// List of SingleSelfReport ojects
        /// if null, an empty list
        /// </param>
        /// <returns>
        /// Return Index view with the list of selfReports
        /// </returns>
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
        /// Handle POST request for uploading self-report CSV files for different scenes
        /// Categorize them by scene type, and return the updated list of selfReports
        /// </summary>
        /// <param name="baselineFiles">Baseline list of IFormFile object</param>
        /// <param name="passiveFiles">Passive list of IFormFile object</param>
        /// <param name="activeFiles">Active list of IFormFile object</param>
        /// <returns>
        /// Return the Index view with the list of selfReports
        /// </returns>
        [HttpPost]
        public IActionResult Index(List<IFormFile> baselineFiles, List<IFormFile> passiveFiles, List<IFormFile> activeFiles)
        {
            List<SingleSelfReport> selfReports = new List<SingleSelfReport>();

            if (baselineFiles != null && baselineFiles.Count > 0)
            {
                // Add Baseline scene type
                selfReports.AddRange(ProcessFiles(baselineFiles, "Baseline"));
            }

            if (passiveFiles != null && passiveFiles.Count > 0)
            {
                // Add Passive scene type
                selfReports.AddRange(ProcessFiles(passiveFiles, "Passive"));
            }

            if (activeFiles != null && activeFiles.Count > 0)
            {
                // Add Active scene type
                selfReports.AddRange(ProcessFiles(activeFiles, "Active"));
            }

            return Index(selfReports);
        }

       /// <summary>
       /// Process a list of CSV files for specific scene type
       /// Each file is validated, read, and converted into list of SingleSelfReport
       /// </summary>
       /// <param name="files">List of IForm object</param>
       /// <param name="sceneType">Type of Scene(Baseline, Passive, Active)</param>
       /// <returns>
       /// Return a list of SingleSelfReport objects
       /// </returns>
        private List<SingleSelfReport> ProcessFiles(List<IFormFile> files, string sceneType)
        {
            List<SingleSelfReport> selfReports = new List<SingleSelfReport>();

            // Process over each file
            foreach (var file in files)
            {
                // Define permitted file type
                string permittedExtension = ".csv";

                // Get the file extension and name
                var extension = Path.GetExtension(file.FileName);
                var fileName = Path.GetFileName(file.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files", fileName);

                // Validate the file extension
                if (extension != permittedExtension)
                {
                    TempData["Message"] = "Please Upload Only .csv file permitted";
                    continue;
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                // Validate the file content
                var error = ValidateFile(filePath);
                if (!string.IsNullOrEmpty(error))
                {
                    TempData["Message"] = error;
                    System.IO.File.Delete(filePath);
                    continue;
                }

                // Process the file to generate self-reports
                var fileSelfReports = GetSelfReportList(filePath, sceneType);
                selfReports.AddRange(fileSelfReports);
            }
            return selfReports;
        }

        /// <summary>
        /// Validates the structure of a CSV file 
        /// to ensure it matches the expected format
        /// by checking the header of CSV file
        /// </summary>
        /// <param name="filePath">File path</param>
        /// <returns>
        /// Return error message if the validation fails
        /// Otherwise return empty sting
        /// </returns>
        private string ValidateFile(string filePath)
        {

            string error = "";
            
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                MissingFieldFound = null // Handle missing fields
            }))
            {
                csv.Read();
                csv.ReadHeader();

                // Extract the header from the third column, convert to lowercase and remove dashes
                string header = csv.HeaderRecord[2].ToLowerInvariant().Replace("-", "");
                // Get the expected header name based on the SingleSelfReport model
                string timeStampModel = nameof(SingleSelfReport.selfReport).ToLowerInvariant();

                if (header != timeStampModel)
                {
                    error = "Header is not matched, Please upload correct CSV file";
                }
            }
            return error;
        }

        /// <summary>
        /// Parse CSV file to extract a list of SingleSelfReport object
        /// Associate each report with a specified scene type
        /// </summary>
        /// <param name="filePath">File path of CSV file to be read</param>
        /// <param name="sceneType">Scene type</param>
        /// <returns>
        /// Return List of SingleSelfReport object
        /// </returns>
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

                // Read each row of CSV file
                while (csv.Read())
                {
                    // Parse the row into a SingleSelfReport object
                    var report = csv.GetRecord<SingleSelfReport>();
                    // Set the scene type
                    report.sceneType = sceneType;
                    // Add the report to the list
                    selfReports.Add(report);
                }
            }
            return selfReports;
        }
    }
}
