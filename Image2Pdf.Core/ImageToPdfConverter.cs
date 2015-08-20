using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Image2Pdf.Core
{
    public class ImageToPdfConverter
    {
        public string ConvertImagesToPdf(List<string> sourceFileList, string outputFilePath, IProgress<TaskProgress> progress)
        {
            using (var outputStream = new MemoryStream())
            {
                int pageCount = 0;

                using (Document document = new Document())
                {
                    document.SetMargins(0, 0, 0, 0);

                    PdfWriter.GetInstance(document, outputStream).SetFullCompression();
                    document.Open();

                    foreach (string sourceFilePath in sourceFileList)
                    {
                        iTextSharp.text.Rectangle pageSize = null;

                        using (var sourceImage = new Bitmap(sourceFilePath))
                        {
                            pageSize = new iTextSharp.text.Rectangle(0, 0, sourceImage.Width, sourceImage.Height);
                        }

                        document.SetPageSize(pageSize);
                        document.NewPage();

                        using (var ms = new MemoryStream())
                        {
                            var image = iTextSharp.text.Image.GetInstance(sourceFilePath);
                            document.Add(image);
                            ++pageCount;
                            progress.Report(new TaskProgress()
                            {
                                ProcessedInputCount = pageCount,
                                StatusMessage = $"{pageCount} of {sourceFileList.Count} images have been added",
                                CompletedPercentage = (pageCount / sourceFileList.Count) * 100
                            });
                        }
                    }
                }

                progress.Report(new TaskProgress() { ProcessedInputCount = pageCount, StatusMessage = $"Saving files...", CompletedPercentage = 100 });

                File.WriteAllBytes(outputFilePath, outputStream.ToArray());

                progress.Report(new TaskProgress() { ProcessedInputCount = pageCount, StatusMessage = $"PDF creation completed.", CompletedPercentage = 100 });

                return outputFilePath;
            }
        }

    }
}
