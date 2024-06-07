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
   
        /// <summary>
        /// Receive the list of gsrReport that contains data created from CSV files
        /// </summary>
        /// <param name="gsrReport"></param>
        /// <returns>
        /// Return the list of GSR to the view, so it can use to generate graph or display in table
        /// </returns>
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


        /// <summary>
        /// Receive a list of file passing from the view which ready to add and read
        /// </summary>
        /// <param name="file"></param>
        /// <returns>
        /// Return the list of GSR object to index page.
        /// </returns>
        //Receive the file in Index function parameter passing from the view

        [HttpPost]
        public IActionResult Index(IFormFile file)
        {
           
            string permittedExtension = ".csv";
            var extension = Path.GetExtension(file.FileName);
            //Get the file name
            var fileName = Path.GetFileName(file.FileName);
            //Get the Path and store the fileName under files folder 
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\files", fileName);

            //Check for file extension format 
            if (extension != permittedExtension)
            {
                TempData["Message"] = "Please Upload Only .csv file permitted";
                return RedirectToAction("Index");
            }
            
            else
            {
                //Create or overwrites a file with a specified path
                using (var stream = System.IO.File.Create(filePath))
                {
                    file.CopyTo(stream);
                    
                }
                
                //validate csv structure
                var error = this.ValidateFile(fileName);
                if (!string.IsNullOrEmpty(error))
                {
                    //Get the error message
                    TempData["Message"] = error;
                    //Delete the file within the filePath
                    System.IO.File.Delete(filePath);
                    //return to Index with the Message ready to display
                    return RedirectToAction("Index");
                }

                //Get the value to store to gsrReport list by accessing the function of GetGsrReportList.
                List<GSR> gsrReport = this.GetGsrReportList(fileName);

                return Index(gsrReport);
            }
           
        }


        /// <summary>
        /// Receive a file, then check the file structure is matched with the model or not
        /// </summary>
        /// <param name="file"></param>
        /// <returns>
        /// Return a string error when the file structure is incorrect.
        /// </returns>
        //Validate files from user input
        private string ValidateFile(string fileName)
        {
            string error = "";
       

            //var config = CsvConfiguration.FromAttributes<GSR>();
            #region Read CSV
            //Specified the file path
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\files", fileName);
            //Configure the CsvConfiguration to read by TAB delimiter not comma delimiter
            var config = new CsvConfiguration(CultureInfo.CurrentCulture) { Delimiter = "\t" };

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, config))
            {
                //csv.Read() function doesnt read, it steps down to the first record in the file
                csv.Read();
                csv.Read();
                csv.Read();       
                csv.ReadHeader();
                //after reader the header, get the value from HeaderRecord[0] index to store in a string 
                string header = csv.HeaderRecord[0];
                //Get the name of the variable in GSR model
                string msModel = nameof(GSR.ms);

                //Compare the header column (ms) with the ms variable in GSR model
                if (header != msModel) 
                {
                    error = "Header is not matched, Please upload correct CSV file";
                }
            }
            #endregion
         
            
            return error;
        }


        /// <summary>
        /// Receive a fileName at function parameter
        /// Read the csv file and convert each row into object
        /// Add the object to the list with GSR data type
        /// Calculate the time that will be used to display on xAxis
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>
        /// Return the GSR list that contains all required value.
        /// </returns>
        //Generate the list of GSR by reading csv file
        private List<GSR> GetGsrReportList(string fileName)
        {
            //Empty list
            List<GSR> gsrReport = new List<GSR>();
            //double countSample = 32130;
            double value = 0;
            //var config = CsvConfiguration.FromAttributes<GSR>();

            #region Read CSV
            //locate the filePath
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\files", fileName);
            //Reconfigure to read the file since GSR csv structure is seperated by tab
            var config = new CsvConfiguration(CultureInfo.CurrentCulture) { Delimiter = "\t" };

            //StreamReader from StreamReader class that use to read text from a file
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, config))
            {
                //doesn't read, steps down to the first record
                csv.Read();
                csv.Read();
                csv.Read();
                //Read the row into CsvHelper as the header values
                csv.ReadHeader();
                //string test = csv.HeaderRecord[0];

                //the sampling rate (Fs) is 51.2, data length (L) is  32130.
                //To create the time label, 
                //t = [0:1 / Fs:(L - 1) / Fs]

                //Read each row of csv file
                while (csv.Read())
                { 
                    //Iterate through each record
                    var gsr = csv.GetRecord<GSR>();
                    //gsr.xValue = gsr.xValue + (1 / 51.2);
                    //assign xValue = value (the value will be incremented by 1/51.2 each rows based on the formula provided)
                    gsr.xValue = value;
                    //The xValueTspan value will be used to display on xAxis.
                    //It will return a TimeSpan that represents the specified number of seconds
                    gsr.xValueTspan = TimeSpan.FromSeconds(gsr.xValue);                  
                    //add the gsr object to gsrReport list    
                    gsrReport.Add(gsr);

                    //add the new value to the previous value
                    value += (1 / 51.2);
                }
            }
            #endregion
            return gsrReport;
        }
       
    }
}
