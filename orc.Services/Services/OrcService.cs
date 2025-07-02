using orc.core.Interfaces;
using orc.core.Models;
using System.Text;
using System.Text.RegularExpressions;
using Tesseract;

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
            //using var engine = new TesseractEngine(tessDataPath, "ara+eng", EngineMode.Default);
            //engine.SetVariable("tessedit_char_whitelist", "0123456789٠١٢٣٤٥٦٧٨٩");

            using var engine = new TesseractEngine(tessDataPath, "ara", EngineMode.Default);
            //engine.SetVariable("tessedit_char_whitelist", "٠١٢٣٤٥٦٧٨٩");
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
    }
}
