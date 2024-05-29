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

namespace ICTCapstoneProject.Controllers
{
    public class CameraPositionController : Controller
    {
        public class NullableDoubleConverter : DefaultTypeConverter
        {
            public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
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

                // If parsing fails, you can either return null or throw an exception,
                // depending on how you wish to handle unexpected formats.
                return null;
            }
        }

        private int GetTotalMinutesRange(string filePath)
        {
            List<string> timestamps = new List<string>();
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true }))
            {
                csv.Read();
                csv.ReadHeader();
                while (csv.Read())
                {
                    var timestamp = csv.GetField("Timestamp");
                    timestamps.Add(timestamp);
                }
            }
            if (timestamps.Count > 0)
            {
                var firstTimestamp = DateTime.Parse(timestamps.First());
                var lastTimestamp = DateTime.Parse(timestamps.Last());
                return (int)(lastTimestamp - firstTimestamp).TotalMinutes;
            }
            return 0;
        }

        [HttpGet]
        public IActionResult Index(List<CameraPosition> cameraPositions = null)
        {
            cameraPositions = cameraPositions == null ? new List<CameraPosition>() : cameraPositions;
            return View(cameraPositions);
        }

        [HttpPost]
        public IActionResult Index(IFormFile file, [FromServices] IWebHostEnvironment hostingEnvironment)
        {

            #region Upload CameraPosition CSV file
            string fileName = $"{hostingEnvironment.WebRootPath}\\files\\{file.FileName}";
            using (FileStream fileStream = System.IO.File.Create(fileName))
            {
                file.CopyTo(fileStream);
                fileStream.Flush();
            }

            var cameraPositions = GetCameraPositionsFromCSV(fileName);

            int maxMinutes = GetTotalMinutesRange(fileName);
            ViewBag.MaxMinutes = maxMinutes;
            #endregion
            return Index(cameraPositions);
        }

        private List<CameraPosition> GetCameraPositionsFromCSV(string filePath)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                PrepareHeaderForMatch = args => args.Header.ToLower(),
                BadDataFound = context => { },
            };

            List<CameraPosition> cameraPositions = new List<CameraPosition>();
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, config))
            {
                csv.Context.TypeConverterCache.AddConverter<double?>(new NullableDoubleConverter());
                cameraPositions = csv.GetRecords<CameraPosition>().ToList();
            }
            return cameraPositions;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}