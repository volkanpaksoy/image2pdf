using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Image2Pdf.Core.InputFileHanding
{
    public class BackupInputFilesStrategy : IInputFileHandlingStrategy
    {
        public void Process(List<string> sourceFileList, string outputFilePath)
        {
            string backupFolderName = $"images-of-{Path.GetFileNameWithoutExtension(outputFilePath)}";
            string backupFolderFullPath = Path.Combine(Path.GetDirectoryName(outputFilePath), backupFolderName);
            DirectoryInfo backupFolder = Directory.CreateDirectory(backupFolderFullPath);
            sourceFileList.ForEach(f => File.Move(f, Path.Combine(backupFolderFullPath, Path.GetFileName(f))));
        }
    }
}
