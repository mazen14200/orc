using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace orc.core.Interfaces
{
    public interface IOrcService
    {
        public Task<string> ExtractTextFromImage(string imagePath, string tessDataPath);
        public Task<bool> CheckIfDataNumbers(string text_withoutSpaces);
        public Task<bool> CheckIfNationalNumber(string text_withoutSpaces);
        
    }
}
