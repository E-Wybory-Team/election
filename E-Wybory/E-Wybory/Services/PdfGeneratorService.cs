using System.IO;
using E_Wybory.Application.Wrappers;
using E_Wybory.Domain.Entities;
using E_Wybory.Services.Interfaces;
using Google.Protobuf.WellKnownTypes;
using Syncfusion;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;


namespace E_Wybory.Services
{
    public class PdfGenerateService : IPdfGeneratorService
    {
        public async Task<string> GeneratePdfWithImage_Syncfusion(string title, string content, string fileName)
        {
            var currentPath = Directory.GetCurrentDirectory();
            var imagePath = currentPath + "\\wwwroot\\logo_ewybory.png";
            Console.WriteLine($"Image path: {imagePath}");
            if (!File.Exists(imagePath))
            {
                Console.WriteLine("Image file not found at the specified path.");
                throw new FileNotFoundException("Image file not found at the specified path.", imagePath);
            }

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (PdfDocument pdfDocument = new PdfDocument())
                {
                    PdfPage pdfPage = pdfDocument.Pages.Add();
                    PdfGraphics graphics = pdfPage.Graphics;

                    try
                    {
                        var titleFontPath = currentPath + "\\PdfElements\\arialbd.ttf";
                        var contentFontPath = currentPath + "\\PdfElements\\arial.ttf";
                        PdfFont titleFont = new PdfTrueTypeFont(titleFontPath, 32);
                        PdfFont contentFont = new PdfTrueTypeFont(contentFontPath, 20);
                        graphics.DrawString(title, titleFont, PdfBrushes.Black, new Syncfusion.Drawing.PointF(30, 160));
                        float pageWidth = pdfPage.GetClientSize().Width;
                        float pageHeight = pdfPage.GetClientSize().Height;
                        float rectWidth = 150; 
                        float rectHeight = 150; 
                        float x = (pageWidth - rectWidth) / 2;
                        float y = (pageHeight - rectHeight) / 2;

                        using (FileStream imageStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                        {
                            PdfBitmap image = new PdfBitmap(imageStream);
                            graphics.DrawImage(image, new Syncfusion.Drawing.RectangleF(x, 0, 150, 150));
                        }

                        graphics.DrawString(content, contentFont, PdfBrushes.Black, new Syncfusion.Drawing.RectangleF(0, 300, pdfPage.GetClientSize().Width, pdfPage.GetClientSize().Height - 300));
                        pdfDocument.Save(memoryStream);

                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine("Not found a font file.");
                        throw new FileNotFoundException("Not found a font file..");
                    }

                }

                var pdfPath = currentPath + $"\\PdfElements\\{fileName}";
                await File.WriteAllBytesAsync(pdfPath, memoryStream.ToArray());

                return pdfPath;
            }
        }
    }
}
