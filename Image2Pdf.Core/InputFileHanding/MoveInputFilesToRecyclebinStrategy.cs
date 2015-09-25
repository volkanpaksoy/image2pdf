using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Image2Pdf.Core.InputFileHanding
{
    public class MoveInputFilesToRecyclebinStrategy : IInputFileHandlingStrategy
    {
        public void Process(List<string> sourceFileList, string outputFilePath)
        {
            sourceFileList.ForEach(f => Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(f, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin));
        }
    }
}
