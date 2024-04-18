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
            gsrReport = gsrReport == null ? new List<GSR>() : gsrReport;
            return View(gsrReport);
        }




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

        private List<GSR> GetGsrReportList(string fileName)
        {
            List<GSR> gsrReport = new List<GSR>();

            var config = CsvConfiguration.FromAttributes<GSR>();
            #region Read CSV
            var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files");
            var filePath = Path.Combine(directoryPath, fileName);

            //var path = $"{Directory.GetCurrentDirectory()}{@"\wwwroot\files"}" + "\\" + fileName;
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, config))
            {
                
                csv.Read();
                csv.ReadHeader();
                while (csv.Read())
                {
                    var gsr = csv.GetRecord<GSR>();
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
