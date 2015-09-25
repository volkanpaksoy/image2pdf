using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Image2Pdf.Core.InputFileHanding
{
    public class RenameInputFilesStrategy : IInputFileHandlingStrategy
    {
        public void Process(List<string> sourceFileList, string outputFilePath)
        {
            // Renamed file format: [original filename]-Page-[n]-of-[output pdf filename].ext
            sourceFileList.ForEach(f =>
            {
                string originalFilename = Path.GetFileNameWithoutExtension(f);
                string outputFilename = Path.GetFileNameWithoutExtension(outputFilePath);
                string renamedFilename = $"{originalFilename}-Page-[{sourceFileList.IndexOf(f) + 1}]-of-{outputFilename}{Path.GetExtension(f)}";
                string renamedFullPath = Path.Combine(Path.GetDirectoryName(outputFilePath), renamedFilename);
                File.Move(f, renamedFullPath);
            });
        }


    }
}
