using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MiniTorrent
{
    public class FilesAndStatus : ObservableCollection<FileDetails>
    {
        private string fileName;
        private long fileSize;
        private string status;
        private int pbValue;
        private double bitRate;
        private string totalTransferTime;

        public string FileName { get { return fileName; }
            set
            {
                if (fileName == value)
                    return;

                fileName = value;
                
            }
        }
        public long FileSize { get { return fileSize; } set { fileSize = value; } }
        public string Status { get { return status; } set { status = value; } }
        public int PbValue { get { return pbValue; } set { pbValue = value; } }
        public double BitRate { get { return bitRate; } set { bitRate = value; } }
        public string TotalTransferTime { get { return totalTransferTime; } set { totalTransferTime = value; } }

        public FilesAndStatus(string fileName, long fileSize, string status)
        {
            this.fileName = fileName;
            this.fileSize = fileSize;
            this.status = status;
            this.pbValue = 0;
        }
    }
}
