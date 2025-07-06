# ORC Project
*we extracted text data from (img and pdf) as english and arabic text using lipraries free in .Net*
## First Moduel 
*extracted text data from (img) as english and arabic text using lipraries* ```SixLabors.ImageSharp -- Tesseract```
### SixLabors.ImageSharp 
*used for sharpen and discrepancy and convert img to gray and then Crop data section*
### Tesseract
*used for get data from images as arabic and english*
**first download language files as ara.traineddata -- eng.traineddata  **
- *link 1 for little files more faster but low quality https://github.com/tesseract-ocr/tessdata   *
- *link 2 for big files more slower but high quality https://github.com/tesseract-ocr/tessdata_best  *
**put downloaded files ara,eng and put them in folder with name ```tessdata``` and put this folder in project which has this package installed**


## Second Moduel
*extracted text data from (pdf_is_text) as english and arabic text using lipraries * ```UglyToad.PdfPig```
### UglyToad.PdfPig
*used for extracting text from pdf_is_text*


## third Moduel
*extracted text data from (pdf_is_imges) as english and arabic text using lipraries* ```PdfiumViewer -- PdfiumViewer.Native.x86_64.v -- Tesseract```
### PdfiumViewer +AND+ PdfiumViewer.Native.x86_64.v
*used for convert (pdf_is_img) file to (images files)*
#### could you get this following Error 
*System.DllNotFoundException: 'Unable to load DLL 'pdfium.dll' or one of its dependencies: The specified module could not be found. (0x8007007E)'*
**solving by downloading this package also after the other ```PdfiumViewer.Native.x86_64.v``` **

### Tesseract
*used for get data from images as arabic and english*
**first download language files as ara.traineddata -- eng.traineddata  **
- *link 1 for little files more faster but low quality https://github.com/tesseract-ocr/tessdata   *
- *link 2 for big files more slower but high quality https://github.com/tesseract-ocr/tessdata_best  *
**put downloaded files ara,eng and put them in folder with name ```tessdata``` and put this folder in project which has this package installed**

