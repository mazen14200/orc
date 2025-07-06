using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace orc.core.Interfaces
{
    public interface IOrcService
    {
        public Task<string> ExtractTextFromImage(string imagePath, string tessDataPath);
        public Task<string> ExtractTextFromImage_AsEn(string imagePath, string tessDataPath);
        public Task<string> ExtractTextFromImage_AsAr(string imagePath, string tessDataPath);
        public Task<bool> CheckIfDataNumbers(string text_withoutSpaces);
        public Task<bool> CheckIfNationalNumber(string text_withoutSpaces);
        public Task<string> FixArabicText(string input);
        public Task<List<Bitmap>> ConvertPdfToImages(string pdfPath);
        public Task<string> ExtractTextFromImageBitMap_AsEn(Bitmap image, string tessDataPath);
        public Task<string> ExtractTextFromImageBitMap_AsArabic(Bitmap image, string tessDataPath);
        public Task<string> SavePdfToWwwRoot(IFormFile sourcefile, string wwwRootPath);

    }
}
