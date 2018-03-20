using System;
using System.Collections.ObjectModel;

namespace Client
{
    public class FileStatus : ObservableCollection<FileDetails>
    {
        public double FileSizeMB { get { return Math.Round(((double)FileSize / 1024 / 1024), 4); } }
        public double BitRateMbps { get { return Math.Round(BitRate, 4); } }

        public string FileName { get; set; }
        public long FileSize { get; set; }
        public string Status { get; set; }
        public int PercentCompleted { get; set; }
        public double BitRate { get; set; }
        public string TotaTime { get; set; }

        public FileStatus(string fileName, long fileSize, string status)
        {
            this.FileName = fileName;
            this.FileSize = fileSize;
            this.Status = status;
            this.PercentCompleted = 0;
        }
    }
}
