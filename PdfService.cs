using System;
using System.Text;
using GenericProject.ViewModel;
using Microsoft.Extensions.Configuration;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;

namespace GenericProject.Service
{
    public class PdfService : IPdfService
    {
        private readonly IConfiguration Configuration;

        public PdfService(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public byte[] ConvertHtmlTextToPDF(string htmlText)
        {
            if (string.IsNullOrEmpty(htmlText))
            {
                return null;
            }

            using (MemoryStream outputStream = new MemoryStream())
            {
                byte[] data = Encoding.UTF8.GetBytes(htmlText);
                using (MemoryStream msInput = new MemoryStream(data))
                {
                    Document doc = new Document();
                    PdfWriter writer = PdfWriter.GetInstance(doc, outputStream);

                    PdfDestination pdfDest = new PdfDestination(PdfDestination.XYZ, 0, doc.PageSize.Height, 1f);

                    doc.Open();

                    XMLWorkerHelper.GetInstance().ParseXHtml(writer, doc, msInput, null, Encoding.UTF8, new UnicodeFontFactory());

                    PdfAction action = PdfAction.GotoLocalPage(1, pdfDest, writer);
                    writer.SetOpenAction(action);

                    doc.Close();
                    return outputStream.ToArray();
                }
            }
        }

        public JSONResultViewModel ProcessCreatePdf(byte[] file, string savePath, string documentId)
        {
            var result = new JSONResultViewModel
            {
                Success = false,
                ErrorMessage = "Export failed.",
                ExtraResult = ""
            };

            var targetFolder = Path.Combine(savePath, "GeneratedPdfs");
            var fileName = $"{documentId}.pdf";
            var filePath = Path.Combine(targetFolder, fileName);
            var relativePath = $"/GenedatedPdfs/{fileName}";

            try
            {
                if (!Directory.Exists(targetFolder))
                {
                    Directory.CreateDirectory(targetFolder);
                }

                File.WriteAllBytes(filePath, file);

                result.Success = true;
                result.ErrorMessage = "";
                result.ExtraResult = relativePath;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"Error: {ex.Message}";
            }

            return result;
        }

        public JSONResultViewModel ViewPdf(string savePath, string documentId)
        {
            var result = new JSONResultViewModel
            {
                Success = false,
                ErrorMessage = "View failed.",
                ExtraResult = ""
            };

            var targetFolder = Path.Combine(savePath, "GeneratedPdfs");
            var filePath = Path.Combine(targetFolder, $"{documentId}.pdf");
            var relativePath = $"/GeneratedPdfs/{documentId}.pdf";

            try
            {
                if (!File.Exists(filePath))
                {
                    result.ErrorMessage = "File not found.";
                }
                else
                {
                    result.Success = true;
                    result.ErrorMessage = "";
                    result.ExtraResult = relativePath;
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"Error: {ex.Message}";
            }

            return result;
        }
    }
}
