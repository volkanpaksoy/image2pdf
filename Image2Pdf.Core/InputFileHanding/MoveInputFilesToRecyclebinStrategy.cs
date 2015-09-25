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
        public void Process(List<string> sourceFileList, string outputFilePath, IProgress<TaskProgress> progress)
        {
            for (int i = 0; i < sourceFileList.Count; i++)
            {
                Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(
                    sourceFileList[i],
                    Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
                    Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);

                progress.Report(new TaskProgress() { ProcessedInputCount = i + 1, StatusMessage = $"Moving file #{i+1} to recycle bin" });
            }

            // sourceFileList.ForEach(f => Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(f, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin));
        }
    }
}
