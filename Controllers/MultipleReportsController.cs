using CsvHelper;
using ICTCapstoneProject.Models;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace ICTCapstoneProject.Controllers
{
    public class MultipleReportsController : Controller
    {
        /// <summary>
        /// Receive the model with all required data
        /// </summary>
        /// <param name="model"></param>
        /// <returns>
        /// Return the model to the view, so it can used the data to display to the user
        /// </returns>
        public IActionResult Index(MultipleReports model)
        {
            //return update value from model to view
            return View(model);
        }

        /// <summary>
        /// Receive a list of file passing from the view which ready to add and read
        /// </summary>
        /// <param name="files"></param>
        /// <returns>
        /// Returns a model that contains required data which use to display on the view.
        /// </returns>
        [HttpPost]
        //This function received a list of files in the parameter passing from the view.
        public IActionResult Index(List<IFormFile> files)
        {

            //Dictionary contains a key as int to count number of files and value is the list of SelfReport
            Dictionary<int, List<SelfReport>> selfReportsDic= new Dictionary<int, List<SelfReport>>();

            //Empty averageValue List
            List<SelfReport> averageList = new List<SelfReport>();

            //List of string contains fileName 
            List<string> listOfFileNames = new List<string>();

            //Loop through each file
            foreach (var file in files)
            {
                //Check for file format
                string permittedExtension = ".csv";
                var extension = Path.GetExtension(file.FileName);

                //Create filePath
                var fileName = Path.GetFileName(file.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\files", fileName); 

                //Check for file exstension
                if(extension != permittedExtension)
                {
                    TempData["Message"] = "Please Upload Only .csv file permitted";
                    return RedirectToAction("Index");
                }

                //Create or overwrites a file with a specified path
                using (var stream = System.IO.File.Create(filePath))
                {
                    file.CopyTo(stream);    
                }

                //Check for csv file structure
                var error = this.ValidateFiles(fileName);

                //Return error message if error has a value
                if (!string.IsNullOrEmpty(error))
                {
                    //Get the error message
                    TempData["Message"] = error;
                    //Delete the file within the filePath
                    System.IO.File.Delete(filePath);
                    //return to Index with the Message ready to display
                    return RedirectToAction("Index");
                }
                
                //Else add the fileName to the listOfFileNames list.
                listOfFileNames.Add(fileName);
            }
          
            //Get the value to store in selfReportsDic Dictionary
            selfReportsDic = this.GetListOfSelfReport(listOfFileNames);
            //Get the value to store in averageList List
            averageList = this.CalculateAverageValue(selfReportsDic);
            //assign selfReportsDictionary and averageSelfReport with value calculated above
            return Index(new MultipleReports() { selfReportsDictionary = selfReportsDic, averageSelfReport = averageList }) ;
        }
        
       /// <summary>
       /// Receive a file, then check the file structure is matched with the model or not
       /// </summary>
       /// <param name="file"></param>
       /// <returns>
       /// Return a string error when the file structure is incorrect.
       /// </returns>
        //Validate files from user input
        private string ValidateFiles(string file)
        {
            string error = "";

            //var config = CsvConfiguration.FromAttributes<GSR>();
            #region Read CSV
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\files", file);

            //Read the file
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {

                csv.Read();
                csv.ReadHeader();
                //Reader the header and assign Header Record at column index 2 to string variable
                string header = csv.HeaderRecord[2].ToLowerInvariant();
                //As the headerRecord value is self-report => selfreport
                header = header.Replace("-", "");
                //get the name of variable in SelfReport model and lower case to selfreport
                string selfReportModel = nameof(SelfReport.selfReport).ToLowerInvariant();

                //check the header that match with the variable in model to display error
                if (header != selfReportModel)
                {
                    error = "Header is not matched, Please upload correct CSV file";
                }
            }
            #endregion


            return error;
        }


        /// <summary>
        /// Receive a listOfFiles, then read it and convert it to object store it to the Dictionary
        /// </summary>
        /// <param name="listOfFileNames">
        /// Receive a list of file names
        /// </param>
        /// <returns>
        /// Return a dictionary contain int as key and value List<SelfReport>
        /// </returns>
        private Dictionary<int, List<SelfReport>> GetListOfSelfReport(List<string> listOfFileNames)
        {
            int count = 0;
            //Empty selfReports List
            List<SelfReport> selfReports = new List<SelfReport>();
            //Empty Dictionary
            Dictionary<int, List<SelfReport>> selfReportsDictionary = new Dictionary<int, List<SelfReport>>();  
            foreach(var file in listOfFileNames)
            {
               
                #region readCSV
                //read each file from the list of file
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\files", file);

                using(var reader = new StreamReader(filePath))
                using(var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                  
                    selfReports = new List<SelfReport>();
                    
                    csv.Read(); 
                    csv.ReadHeader();
                    while (csv.Read())
                    {
                        //Iterate each record with SelfReport type to create an object
                        var report = csv.GetRecord<SelfReport>();
                        
                        //add the object to the list
                        selfReports.Add(report);
                        
                    }
                    //after finish to read 1 file, implement to 1
                    count++;
                    //store the count value and the list of selfReports to the Dictionary
                    selfReportsDictionary.Add(count, selfReports);
                    //Continue the same process for the next file.
                }
                #endregion

            }
            return selfReportsDictionary;
        }


        /// <summary>
        /// This function receives the dictionary that contains multiple list of self reports
        /// it will loop through the value of the dictionary and add each selfReport list into the List type List<SelfReport>
        /// Loop through the list and get each row with identical index 
        /// The sum of each row will be calculated 
        /// The total value will then divide by the number of files 
        /// With Segment Total value and Self Report total value, it now will be used to create a selfReport object and add to the list
        /// </summary>
        /// <param name="multipleSelfReportDictionary"></param>
        /// <returns>
        /// Return the list of average data after calculation based on multiple CSV files.
        /// </returns>
        //Calculate the Average data from multiple csv files store in the Dictionary
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
                //get current date time
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
