using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using orc.core.Interfaces;
using orc.core.Models;
using orc.UI.DTO;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
//using System.Drawing;
using Tesseract;
using static System.Net.Mime.MediaTypeNames;


namespace orc.UI.Controllers
{

    [Route("[controller]")]
    public class OrcController : Controller
    {
        private readonly IBaseRepository<National> _nationalTable;
        private readonly IOrcService _orc;

        public OrcController(IBaseRepository<National> nationalTable, IOrcService orc)
        {
            _nationalTable = nationalTable;
            _orc = orc;
        }
        [HttpGet("Orc")]
        public IActionResult Orc()
        {
            return View();
        }
        [HttpPost("Orc")]
        public async Task<IActionResult> Orc(string firstName, string lastName,string lang, IFormFile file)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.TextValue_View = "من فضلك اكمل البيانات";
                return View();
            }
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
                string extractedText = "";
                if(lang == "ar")
                {
                    // هنا بنحدد الجزء اللي عايزين نقطعه من الصورة:
                    Rectangle cropRect_Name = new Rectangle(x: 50, y: 100, width: 300, height: 100);
                    Rectangle cropRect_Address = new Rectangle(x: 50, y: 100, width: 300, height: 100);
                    Rectangle cropRect_NationalNumber = new Rectangle(x: 50, y: 100, width: 300, height: 100);
                    extractedText = await _orc.ExtractTextFromImage_AsAr(imagePath, tessDataPath);
                }
                else
                {
                    extractedText = await _orc.ExtractTextFromImage_AsEn(imagePath, tessDataPath);
                }
                Console.WriteLine(extractedText);
                var structuredData = ParseOcrText(extractedText);

                return View("Result", structuredData);
                //string text_withoutSpaces = extractedText.Replace(" ", "").Replace("\n", "").Replace("\t", "");
                //string text_withoutSpaces = Regex.Replace(extractedText, @"[^\d٠-٩]", "");
                string text_withoutSpaces = extractedText;

                bool isValid = false;
                bool isNumber = await _orc.CheckIfDataNumbers(text_withoutSpaces);
                if (isNumber)
                {
                    isValid = await _orc.CheckIfNationalNumber(text_withoutSpaces);
                }

                ViewBag.TextValue_View = extractedText;

                if (isValid) {
                    Console.WriteLine("OK it's a National Number");
                    ViewBag.Message = "OK it's a National Number!";
                    var allNationals = _nationalTable.GetAll();
                    var lastSerial = allNationals?.LastOrDefault()?.Id??"0";
                    National nationalSingle = new National();
                    nationalSingle.Id = (int.Parse(lastSerial) + 1).ToString("00000");
                    nationalSingle.FirstName = firstName;
                    nationalSingle.LastName = lastName;
                    nationalSingle.NationalNumber = text_withoutSpaces;
                    _nationalTable.Add(nationalSingle);
                    _nationalTable.Complete();

                }
                else
                {
                    Console.WriteLine("No, it's Not a National Number");
                    ViewBag.Message = "No, it's Not a National Number!";

                }

            }
            else
            {
                ViewBag.Message = "No file uploaded!";
            }

            //return View("UploadResult");
            return View();
        }

        public NationalIdData ParseOcrText(string ocrText)
        {
            var lines = ocrText
                .Split('\n')
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList();

            return new NationalIdData
            {
                Name = lines.Count > 1 ? $"{lines[0]} {lines[1]}" : lines.FirstOrDefault(),
                Address = lines.ElementAtOrDefault(2),
                Governorate = lines.ElementAtOrDefault(3),
                Extra = string.Join(" | ", lines.Skip(4)) // Combine remaining info
            };
        }
    }
}
