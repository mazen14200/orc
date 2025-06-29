using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using System;
using Tesseract;


namespace orc.UI.Controllers
{
    [Route("[controller]")]
    public class OrcController : Controller
    {
        [HttpGet("Orc")]
        public IActionResult Orc()
        {
            return View();
        }
        [HttpPost("Orc")]
        public async Task<IActionResult> Orc(string firstName, string lastName, IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                var filePath = Path.Combine("wwwroot/uploads", Path.GetFileName(file.FileName));
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                Console.WriteLine("النص المستخرج:");

                //string imagePath = "D:\\mazen\\orc\\orc.UI\\wwwroot\\uploads\\ITFdiag1.png"; // مسار الصورة
                string imagePath = filePath;
                string tessDataPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");

                string extractedText = ExtractTextFromImage(imagePath, tessDataPath);
                Console.WriteLine(extractedText);

                ViewBag.Message = "File uploaded successfully!";
            }
            else
            {
                ViewBag.Message = "No file uploaded!";
            }

            //return View("UploadResult");
            return View();
        }

        static string ExtractTextFromImage(string imagePath, string tessDataPath)
        {
            using var engine = new TesseractEngine(tessDataPath, "eng", EngineMode.Default);
            using var img = Pix.LoadFromFile(imagePath);
            using var page = engine.Process(img);
            return page.GetText();
        }
    }
}
