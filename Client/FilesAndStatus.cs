using System.Collections.ObjectModel;

namespace MiniTorrent
{
    public class FileStatus : ObservableCollection<FileDetails>
    {
        //public string FileName
        //{
        //    get { return fileName; }
        //    set
        //    {
        //        if (fileName == value)
        //            return;

        //        fileName = value;
        //    }
        //}

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
