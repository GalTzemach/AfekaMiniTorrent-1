using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class FileDetails : IEquatable<FileDetails>
    {
        private string fileName;
        private long fileSize;

        public string FileName { get { return fileName; } set { fileName = value; } }
        public long FileSize { get { return fileSize; } set { fileSize = value; } }

        public FileDetails(string fileName, long fileSize)
        {
            this.fileName = fileName;
            this.fileSize = fileSize;
        }

        public override int GetHashCode()
        {
            if (fileName == null) return 0;
            return fileName.GetHashCode();
        }

        public bool Equals(FileDetails other)
        {
            return this.FileName == other.FileName && this.FileSize == other.FileSize;
        }
    }
}
