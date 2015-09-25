using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Image2Pdf.Core.InputFileHanding
{
    public class DeleteInputFilesStrategy : IInputFileHandlingStrategy
    {
        public void Process(List<string> sourceFileList, string outputFilePath)
        {
            sourceFileList.ForEach(f => File.Delete(f));
        }
    }
}
