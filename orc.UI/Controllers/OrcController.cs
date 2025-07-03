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
using Tesseract;
using static System.Net.Mime.MediaTypeNames;
using SixLabors.ImageSharp.Formats.Png;
using Image = SixLabors.ImageSharp.Image; // أو .Jpeg حسب التنسيق
using UglyToad.PdfPig;


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
                if (file.ContentType.ToLower().Contains("pdf")) {
                    string pdfPath = "D:\\mazen\\word\\tasks/1.pdf"; // أو مسار آخر
                    string pagedata = $" ";
                    using (PdfDocument document = PdfDocument.Open(pdfPath))
                    {
                        foreach (var page in document.GetPages())
                        {
                            string text = page.Text;
                            Console.OutputEncoding = System.Text.Encoding.UTF8; // لدعم العربية

                            Console.WriteLine($"P{page.Number}:\n{text}\n");
                            pagedata = pagedata + $"P{page.Number}:\n{text}\n";

                        }
                    }
                    if (lang == "ar")
                    {
                        string Arab_True = await _orc.FixArabicText(pagedata);
                        ViewBag.Message = Arab_True;
                    }
                    else
                    {
                        ViewBag.Message = pagedata;
                    }
                    return View();
                }
                var uploadsFolder = Path.Combine("wwwroot", "uploads");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = Path.GetFileNameWithoutExtension(file.FileName);
                var fileExt = Path.GetExtension(file.FileName);
                var grayFileName = $"{fileName}_gray{fileExt}";
                var savePath = Path.Combine(uploadsFolder, grayFileName);

                using var image = await Image.LoadAsync(file.OpenReadStream());


                // الحصول على الأبعاد
                int width_ignore = Convert.ToInt16(image.Width*0.31);
                int width_Cut = Convert.ToInt16(image.Width * 0.69);
                int height_ignore = Convert.ToInt16(image.Height*0.17);
                int height_Cut = Convert.ToInt16(image.Height*0.73);//0.82

                // شرح والمثال في الاسفل 🔲 تحديد الجزء المطلوب قصه (مثال: من الإحداثي 250,80 بحجم 600-400)
                var cropRectangle = new Rectangle(x: width_ignore, y: height_ignore, width: width_Cut, height: height_Cut);
                // for example var cropRectangle = new Rectangle(x: 250, y: 80, width: 600, height: 400);

                // التحقق أن الأبعاد داخل حدود الصورة الأصلية
                cropRectangle.Intersect(new Rectangle(0, 0, image.Width, image.Height));

                // 🪚 قص الجزء المحدد + تحويل إلى رمادي
                image.Mutate(x => x
                    .Crop(cropRectangle)
                    .Grayscale()
                    .Contrast(1.2f)      // رفع التباين
                    .Brightness(1.1f)   // رفع الإضاءة قليلاً
                );

                await image.SaveAsync(savePath); // تلقائيًا يحدد التنسيق حسب الامتداد

                Console.WriteLine("النص المستخرج:");

                //مثال string imagePath = "D:\\mazen\\orc\\orc.UI\\wwwroot\\uploads\\ITFdiag1.png"; // مسار الصورة
                string imagePath = savePath;
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
                    nationalSingle.Name = firstName+ lastName;
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

        [HttpPost("resultSave")]
        public async Task<IActionResult> resultSave(NationalIdData nationalIdData)
         {
            if (!ModelState.IsValid)
            {
                nationalIdData.message = "The Number which you Entered is Wrong";
                return View("Result",nationalIdData);
            }
            else nationalIdData.message = "";

            bool isValid = false;
            bool isNumber = await _orc.CheckIfDataNumbers(nationalIdData.Id_NationalNumber);
            if (isNumber)
            {
                isValid = await _orc.CheckIfNationalNumber(nationalIdData.Id_NationalNumber);
            }

            if (isValid)
            {
                Console.WriteLine("OK it's a National Number");
                ViewBag.Message = "OK it's a National Number!";
                var allNationals = _nationalTable.GetAll();
                var lastSerial = allNationals?.LastOrDefault()?.Id ?? "0";
                National nationalSingle = new National();
                nationalSingle.Id = (int.Parse(lastSerial) + 1).ToString("00000");
                nationalSingle.Name = nationalIdData.Name;
                nationalSingle.Address = nationalIdData.Address;
                nationalSingle.Government = nationalIdData.Governorate;
                nationalSingle.NationalNumber = nationalIdData.Id_NationalNumber;
                _nationalTable.Add(nationalSingle);
                _nationalTable.Complete();
                return View("Orc");
            }
            else
            {
                nationalIdData.message = "The Number which you Entered is Wrong";
                return View("Result", nationalIdData);

            }

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
