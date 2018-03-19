using System;

namespace Server
{
    public class FileDetails : IEquatable<FileDetails>
    {
        public string FileName { get; set; }
        public long FileSize { get; set; }

        public double FileSizeMB { get { return Math.Round((double)(FileSize) / 1024 / 1024, 4); } }

        public FileDetails(string fileName, long fileSize)
        {
            this.FileName = fileName;
            this.FileSize = fileSize;
        }

        public override int GetHashCode()
        {
            return FileName == null ? 0 : FileName.GetHashCode();
        }

        public bool Equals(FileDetails other)
        {
            return this.FileName == other.FileName && this.FileSize == other.FileSize;
        }
    }
}
