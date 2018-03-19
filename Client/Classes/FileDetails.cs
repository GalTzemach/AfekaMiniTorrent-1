using System;

namespace MiniTorrent
{
    public class FileDetails : IEquatable<FileDetails>
    {
        public string FileName { get; set; }
        public long FileSize { get; set; }

        public FileDetails(string fileName, long fileSize)
        {
            this.FileName = fileName;
            this.FileSize = fileSize;
        }

        public bool Equals(FileDetails other)
        {
            return this.FileName == other.FileName && this.FileSize == other.FileSize;
        }
    }
}
