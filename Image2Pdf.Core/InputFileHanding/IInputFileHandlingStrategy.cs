using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Image2Pdf.Core.InputFileHanding
{
    public interface IInputFileHandlingStrategy
    {
        void Process(List<string> sourceFileList, string outputFilePath);
    }
}
