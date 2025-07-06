using orc.core.Interfaces;
using orc.core.Models;
using PdfiumViewer; // تحويل ال pdf to images
using System;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using Tesseract;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace orc.Services.Services
{
    public class OrcService : IOrcService
    {
        private readonly IBaseRepository<National> _nationalTable;

        public OrcService(IBaseRepository<National> nationalTable)
        {
            _nationalTable = nationalTable;
        }

        public async Task<string> ExtractTextFromImage(string imagePath, string tessDataPath)
        {
            using var engine = new TesseractEngine(tessDataPath, "ara", EngineMode.Default);
            engine.DefaultPageSegMode = PageSegMode.Auto;

            using var img = Pix.LoadFromFile(imagePath);
            using var page = engine.Process(img);

            string fullText = page.GetText();
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("Full OCR Output:");
            Console.WriteLine(fullText);

            return fullText;
        }

        public async Task<string> ExtractTextFromImage_AsEn(string imagePath, string tessDataPath)
        {
            using var engine = new TesseractEngine(tessDataPath, "eng", EngineMode.Default);
            using var img = Pix.LoadFromFile(imagePath);
            using var page = engine.Process(img);
            return page.GetText();
        }
        public async Task<string> ExtractTextFromImage_AsAr(string imagePath, string tessDataPath)
        {
            using var engine = new TesseractEngine(tessDataPath, "ara", EngineMode.Default);
            using var img = Pix.LoadFromFile(imagePath);
            using var page = engine.Process(img);
            return page.GetText();
        }

        public async Task<string> ExtractDataExceptNationalId(string imagePath, string tessDataPath)
        {
            string text = await ExtractTextFromImage(imagePath, tessDataPath);

            // Remove national ID (usually a 14-digit Arabic numeral)
            string arabicDigitPattern = @"[\u0660-\u0669]{14}";
            string latinDigitPattern = @"\d{14}";

            // Replace Arabic and Latin 14-digit numbers with empty string
            string cleanText = Regex.Replace(text, arabicDigitPattern, "");
            cleanText = Regex.Replace(cleanText, latinDigitPattern, "");

            // Optionally, clean remaining junk characters
            cleanText = cleanText.Trim();

            Console.WriteLine("\nFiltered OCR Output (excluding national number):");
            Console.WriteLine(cleanText);

            return cleanText;
        }

        public async Task<bool> CheckIfDataNumbers(string text_withoutSpaces)
        {
            bool success = long.TryParse(text_withoutSpaces, out _);
            return success;
        }

        public async Task<bool> CheckIfNationalNumber(string text_withoutSpaces)
        {
            if (text_withoutSpaces.Length != 14) return false;

            try
            {
                int century = int.Parse(text_withoutSpaces.Substring(0, 1));
                int year = int.Parse(text_withoutSpaces.Substring(1, 2));
                int month = int.Parse(text_withoutSpaces.Substring(3, 2));
                int day = int.Parse(text_withoutSpaces.Substring(5, 2));
                int city = int.Parse(text_withoutSpaces.Substring(7, 2));

                if (century > 3 || century == 0) return false;
                if (month == 0 || month > 12) return false;
                if (day == 0 || day > 31) return false;

                if (month == 2 && day > 29) return false;
                if (new[] { 4, 6, 9, 11 }.Contains(month) && day > 30) return false;

                if ((city > 4 && city < 11) || (city > 35 && city < 88)) return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> FixArabicText(string input)
        {
            // عكس السطر
            var lines = input.Split('\n');
            var fixedLines = lines.Select(line => new string(line.Reverse().ToArray()));

            return string.Join("\n", fixedLines);
        }



        public async Task<List<Bitmap>> ConvertPdfToImages(string pdfPath)
        {
            var images = new List<Bitmap>();
            using (var document = PdfDocument.Load(pdfPath))
            {
                for (int page = 0; page < document.PageCount; page++)
                {
                    var image = document.Render(page, 300, 300, true); // دقة عالية
                    images.Add((Bitmap)image);
                }
            }
            return images;
        }

        public async Task<string> ExtractTextFromImageBitMap_AsEn(Bitmap image, string tessDataPath)
        {
            using (var engine = new TesseractEngine(tessDataPath, "eng", EngineMode.Default))
            {
                using (var pix = PixConverter.ToPix(image))
                {
                    using (var page = engine.Process(pix))
                    {
                        return page.GetText();
                    }
                }
            }
        }
        public async Task<string> ExtractTextFromImageBitMap_AsArabic(Bitmap image, string tessDataPath)
        {
            using (var engine = new TesseractEngine(tessDataPath, "ara", EngineMode.Default))
            {
                using (var pix = PixConverter.ToPix(image))
                {
                    using (var page = engine.Process(pix))
                    {
                        return page.GetText();
                    }
                }
            }
        }

        public async Task<string> SavePdfToWwwRoot(IFormFile sourceFile, string wwwRootPath)
        {
            // تأكد إن الملف موجود
            if (sourceFile==null || sourceFile.Length<1)
                throw new FileNotFoundException("Source PDF not found.");
            string folderName = "pdfs";
            // إنشاء فولدر pdfs لو مش موجود داخل wwwroot
            string targetFolder = Path.Combine(wwwRootPath, folderName);
            if (!Directory.Exists(targetFolder))
                Directory.CreateDirectory(targetFolder);

            //// عمل اسم جديد للملف (مثلا باستخدام Guid لتجنب التكرار)
            //string newFileName = $"{Guid.NewGuid()}.pdf";
            string newFileName = sourceFile.FileName;

            // تحديد المسار الكامل للنسخة الجديدة
            string targetPath = Path.Combine(targetFolder, newFileName);

            // حفظ الملف
            using (var stream = new FileStream(targetPath, FileMode.Create))
            {
                await sourceFile.CopyToAsync(stream);
            }

            // إنشاء path relative عشان تقدر تستخدمه فى الروابط
            string relativePath = Path.Combine("/", folderName, newFileName).Replace("\\", "/");

            return targetPath;
        }


}
}
