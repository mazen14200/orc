using orc.core.Interfaces;
using orc.core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;
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
            using var engine = new TesseractEngine(tessDataPath, "eng", EngineMode.Default);
            using var img = Pix.LoadFromFile(imagePath);
            using var page = engine.Process(img);
            return page.GetText();
        }

        public async Task<bool> CheckIfDataNumbers(string text_withoutSpaces) {
            Int64 ConvertedNumber;
            bool success = Int64.TryParse(text_withoutSpaces, out ConvertedNumber);
            if (success)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> CheckIfNationalNumber(string text_withoutSpaces)
        {
            if (text_withoutSpaces.Length != 14)
            {
                return false;
            }

            int century = int.Parse(text_withoutSpaces.Substring(0, 1));
            int year = int.Parse(text_withoutSpaces.Substring(1, 2));
            int month = int.Parse(text_withoutSpaces.Substring(3, 2));
            int day = int.Parse(text_withoutSpaces.Substring(5, 2));
            int city = int.Parse(text_withoutSpaces.Substring(7, 2));
            int serial = int.Parse(text_withoutSpaces.Substring(9, 4));
            //int gender = int.Parse(text_withoutSpaces.Substring(12, 1));
            int validation = int.Parse(text_withoutSpaces.Substring(13, 1));
            
            if (century > 3 || century == 0)
            {
                return false;
            }
            if (month > 12 || month==0)
            {
                return false;
            }
            if (day == 0)
            {
                return false;
            }

            if (month ==2)
            {
                if (day > 29)
                {
                    return false;
                }
            }
            else if (month == 1 || month == 3 || month == 5 || month == 7 || month == 8 || month == 10 || month == 12)
            {
                if (day > 31)
                {
                    return false;
                }
            }
            else
            {
                if (day > 30)
                {
                    return false;
                }
            }
            if (city > 4 && city <11)
            {
                return false;
            }
            if (city > 35 && city < 88)
            {
                return false;
            }
            return true;
        }
        
    }
}
