using ICTCapstoneProject.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using CsvHelper;
using System.Web;
using System.Reflection.PortableExecutable;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using System.IO;
using System;
using System.Globalization;
using Microsoft.IdentityModel.Tokens;

namespace ICTCapstoneProject.Controllers
{
    public class CameraPositionController : Controller
    {
        /// <summary>
        /// Handle CSV data to convert double values "NULL" as null 
        /// and to prevent TypeConversionException by returning null
        /// if conversion fails.
        /// Override the DefaultTypeConverter class of CsvHelper
        /// </summary>
        public class NullableDoubleConverter : DefaultTypeConverter
        {
            /// <summary>
            /// Convert a string from CSV data to a nullable double. 
            /// Treat "NULL" as a null
            /// Attempt to parse strings to double
            /// Preventing TypeConversionException
            /// </summary>
            /// <param name="text">String to convert</param>
            /// <param name="row"></param>
            /// <param name="memberMapData"></param>
            /// <returns>
            /// Return a nullable double or null if the string is "NULL"
            /// </returns>
            public override object? ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
            {
                // Check if the text is "NULL" and return null in that case.
                if (string.Equals(text, "NULL", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                // Attempt to parse the text as a double for non-"NULL" values.
                if (double.TryParse(text, out double result))
                {
                    return result;
                }

                // If parsing fails, return null (or we can throw an exception)
                return null;
            }
        }

        /// <summary>
        /// Read timestamps and parse into DateTime object
        /// Calculate a total time range in Minutes
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>
        /// Return the total number of minutes between the first and last timestamp.
        /// </returns>
        private int GetTotalMinutesRange(string filePath)
        {
            List<string> timestamps = new List<string>();
            using (var reader = new StreamReader(filePath))
            // CultureInfo.InvariantCulture parse strings to DateTime format
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true }))
            {
                csv.Read();
                csv.ReadHeader();
                while (csv.Read())
                {
                    // Add to the list from Timestamp field
                    var timestamp = csv.GetField("Timestamp");
                    timestamps.Add(timestamp);
                }
            }
            if (timestamps.Count > 0)
            {
                // Calculate the time range difference 
                // between first and last timestamp
                var firstTimestamp = DateTime.Parse(timestamps.First());
                var lastTimestamp = DateTime.Parse(timestamps.Last());
                return (int)(lastTimestamp - firstTimestamp).TotalMinutes;
            }
            return 0;
        }

        /// <summary>
        /// Display the index view with CameraPosition object list
        /// if no list, initialize an empty list
        /// </summary>
        /// <param name="cameraPositions">List of CameraPosition object</param>
        /// <returns>
        /// Return a View that display a list of CameraPosition
        /// </returns>
        [HttpGet]
        public IActionResult Index(List<CameraPosition>? cameraPositions = null)
        {
            cameraPositions = cameraPositions == null ? new List<CameraPosition>() : cameraPositions;
            return View(cameraPositions);
        }

        /// <summary>
        /// Post request to upload CSV file
        /// Set ViewBag.MaxMinutes manipulated by GetTotalMinutesRange
        /// </summary>
        /// <param name="file">
        /// CSV file uploaded by user.
        /// </param>
        /// <returns>
        /// Redirect to the same View
        /// </returns>
        [HttpPost]
        public IActionResult Index(IFormFile file)
        {

            #region Upload CameraPosition CSV file

            // Get the File name
            var fileName = Path.GetFileName(file.FileName);
            // Configure file path
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\files", fileName);
            
            // Save the file
            using (var stream = System.IO.File.Create(filePath))
            {
                file.CopyTo(stream);
                
            }

            //Valid the file
            var error = this.validatesFile(fileName);
            if (!string.IsNullOrEmpty(error))
            {
                TempData["Message"] = error;
                System.IO.File.Delete(filePath);
                return RedirectToAction("Index");
            }

            // Create Camera Position Object List from CSV data
            var cameraPositions = GetCameraPositionsFromCSV(filePath);

            // Manipulate the time range Max Minutes
            int maxMinutes = GetTotalMinutesRange(filePath);
            ViewBag.MaxMinutes = maxMinutes;

            #endregion

            return Index(cameraPositions);
        }

        /// <summary>
        /// Receive a file, then check the file structure is matched with the model or not
        /// </summary>
        /// <param name="file"></param>
        /// <returns>
        /// Return a string error when the file structure is incorrect.
        /// </returns>
        private string validatesFile(string fileName)
        {
            string error = "";

            //Read CSV file
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\files", fileName);
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {

                csv.Read();
                csv.ReadHeader();
                string header = csv.HeaderRecord[2];
                string cameraPositionModel = nameof(CameraPosition.CameraActive);


                if (header != cameraPositionModel)
                {
                    error = "Header is not matched, Please upload correct CSV file";
                }
            }



            return error;
        }

        /// <summary>
        /// Receive a file path as function parameter
        /// Read the csv file and convert each row into CameraPosition object
        /// Return the list of CameraPosition Object
        /// </summary>
        /// <param name="filePath">
        /// Path to the CSV file to be processed
        /// </param>
        /// <returns>
        /// List of CameraPosition objects populated from the CSV data
        /// </returns>
        private List<CameraPosition> GetCameraPositionsFromCSV(string filePath)
        {

            #region Read CameraPosition CSV data
            
            // Configure the CSV File Reader
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                // To lower case for Headers
                PrepareHeaderForMatch = args => args.Header.ToLower(),
                // Handle invalid data entry 
                BadDataFound = context => { },
            };

            List<CameraPosition> cameraPositions = new List<CameraPosition>();
            // Open CSV file
            using (var reader = new StreamReader(filePath))
            // Read the data one row by row
            using (var csv = new CsvReader(reader, config))
            {
                // Custom Converter for double data type of CSV Reader context
                csv.Context.TypeConverterCache.AddConverter<double?>(new NullableDoubleConverter());
                // Convert to Object from reading data, and convert to List
                cameraPositions = csv.GetRecords<CameraPosition>().ToList();
            }

            #endregion

            return cameraPositions;
        }
    }
}