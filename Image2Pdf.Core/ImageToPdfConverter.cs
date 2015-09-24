using Image2Pdf.Core.InputFileHanding;
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
        private IInputFileHandlingStrategy _inputFileHandlingStrategy;
        private List<string> _sourceFileList;
        private string _outputFilePath;

        public ImageToPdfConverter(List<string> sourceFileList, 
            string outputFilePath,
            IInputFileHandlingStrategy inputFileHandlingStrategy)
        {
            _sourceFileList = sourceFileList;
            _outputFilePath = outputFilePath;
            _inputFileHandlingStrategy = inputFileHandlingStrategy;
        }

        public void ConvertImagesToPdf(IProgress<TaskProgress> progress)
        {
            if (_sourceFileList == null || _sourceFileList.Count == 0) { throw new ArgumentException("At least 1 source file must be specified"); }
            if (string.IsNullOrWhiteSpace(_outputFilePath)) { throw new ArgumentException("Invalid output file name"); }

            using (var outputStream = new MemoryStream())
            {
                int pageCount = 0;

                using (Document document = new Document())
                {
                    document.SetMargins(0, 0, 0, 0);

                    PdfWriter.GetInstance(document, outputStream).SetFullCompression();
                    document.Open();

                    foreach (string sourceFilePath in _sourceFileList)
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
                                StatusMessage = $"{pageCount} of {_sourceFileList.Count} images have been added",
                                CompletedPercentage = (pageCount / _sourceFileList.Count) * 100
                            });
                        }
                    }
                }

                progress.Report(new TaskProgress() { ProcessedInputCount = pageCount, StatusMessage = $"Saving files...", CompletedPercentage = 100 });

                File.WriteAllBytes(_outputFilePath, outputStream.ToArray());

                progress.Report(new TaskProgress() { ProcessedInputCount = pageCount, StatusMessage = $"PDF creation completed.", CompletedPercentage = 100 });


                HandleInputFiles();

                HandleOutputFile();
            }
        }

        private void HandleInputFiles()
        {
            if (_inputFileHandlingStrategy != null)
            {
                _inputFileHandlingStrategy.Process(_sourceFileList);
            }
        }

        private void HandleOutputFile()
        {
        }



    }
}
