using CsvHelper;
using ICTCapstoneProject.Models;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Numerics;

namespace ICTCapstoneProject.Controllers
{
    public class GSRController : Controller
    {
   
        [HttpGet]
        public IActionResult Index(List<GSR> gsrReport = null )
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
                    var gsr = csv.GetRecord<GSR>();
                    //gsr.ms = Convert.ToDecimal(csv.GetField("ms"), CultureInfo.InvariantCulture);
                    gsrReport.Add(gsr);
                }
            }
            #endregion
            return gsrReport;
        }
       
    }
}
