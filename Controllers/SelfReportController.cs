using CsvHelper;
using ICTCapstoneProject.Models;
using Microsoft.AspNetCore.Mvc;
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

            var selfReports = this.GetSelfReportList(fileName);
            return Index(selfReports);
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
