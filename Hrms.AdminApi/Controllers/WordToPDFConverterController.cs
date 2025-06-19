using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WordToPDFConverter.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class WordToPDFConverterController : ControllerBase
    {
        [HttpPost("split-pdf")]
        [RequestSizeLimit(200 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 200 * 1024 * 1024)]
        public async Task<IActionResult> SplitPdf(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            string tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempFolder);
            string zipPath = Path.Combine(tempFolder, "SalarySlip.zip");

            string tempPdfPath = Path.Combine(tempFolder, "temp.pdf");
            using (var fileStream = new FileStream(tempPdfPath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            try
            {
                Console.WriteLine($"Processing PDF: {tempPdfPath}");

                using (var reader = new iText.Kernel.Pdf.PdfReader(tempPdfPath))
                using (var pdfDoc = new iText.Kernel.Pdf.PdfDocument(reader))
                {
                    int totalPages = pdfDoc.GetNumberOfPages();

                    using (var inputPdf = PdfSharp.Pdf.IO.PdfReader.Open(tempPdfPath, PdfDocumentOpenMode.Import))
                    {
                        for (int i = 0; i < totalPages; i++)
                        {
                            Console.WriteLine($"Processing Page {i + 1}...");
                            ProcessPage(inputPdf, pdfDoc, tempFolder, i);
                        }
                    }
                }

                Console.WriteLine("Creating ZIP...");
                using (FileStream zipToCreate = new FileStream(zipPath, FileMode.Create))
                {
                    using (ZipArchive archive = new ZipArchive(zipToCreate, ZipArchiveMode.Create, leaveOpen: false))
                    {
                        foreach (var filePath in Directory.GetFiles(tempFolder, "*.pdf"))
                        {
                            archive.CreateEntryFromFile(filePath, Path.GetFileName(filePath));
                        }
                    }
                }

                byte[] zipBytes = await System.IO.File.ReadAllBytesAsync(zipPath);
                Console.WriteLine("ZIP ready, sending response...");
                return File(zipBytes, "application/zip", "SalarySlip.zip");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, $"Error processing PDF: {ex.Message}");
            }
            finally
            {
                if (Directory.Exists(tempFolder))
                    Directory.Delete(tempFolder, true);
            }
        }

        private void ProcessPage(PdfSharp.Pdf.PdfDocument inputPdf, iText.Kernel.Pdf.PdfDocument pdfDoc, string tempFolder, int pageNumber)
        {
            try
            {
                Console.WriteLine($"Extracting Emp Code for Page {pageNumber + 1}...");
                string empCode = ExtractEmpCode(pdfDoc, pageNumber);
                string fileName = !string.IsNullOrEmpty(empCode) ? $"{empCode}.pdf" : $"Page_{pageNumber + 1}.pdf";

                using (var singlePagePdf = new PdfSharp.Pdf.PdfDocument())
                {
                    singlePagePdf.AddPage(inputPdf.Pages[pageNumber]);
                    string outputPath = Path.Combine(tempFolder, fileName);
                    singlePagePdf.Save(outputPath);
                    Console.WriteLine($"Saved {outputPath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing page {pageNumber + 1}: {ex.Message}");
            }
        }

        private string ExtractEmpCode(iText.Kernel.Pdf.PdfDocument pdfDoc, int pageNumber)
        {
            try
            {
                if (pageNumber >= pdfDoc.GetNumberOfPages())
                    return null;

                string pageText = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(pageNumber + 1));
                Match match = Regex.Match(pageText, @"Emp\. Code:\s*(\S+)");

                return match.Success ? match.Groups[1].Value : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting Emp Code from page {pageNumber + 1}: {ex.Message}");
                return null;
            }
        }
    }
}
