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
            //Returns the initial view for displaying a list of camera position data.
            [HttpGet]
            public IActionResult Index(List<CameraPosition> cameraPositions = null)
            {
                cameraPositions = cameraPositions == null ? new List<CameraPosition>() : cameraPositions;
                return View(cameraPositions);
            }

            //Receives the uploaded CSV file and stores it on the server.
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

                #endregion

                return Index(cameraPositions);
            }

            //Reads a CSV file, converts each row into a CameraPosition and returns a list
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
        }
    }
}
