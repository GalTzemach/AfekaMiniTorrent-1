using System.Collections.Generic;

namespace MiniTorrent
{
    public class TransferFileDetails
    {
        private string fileName;
        private long fileSize;
        private List<Pear> pears;

        public string FileName { get { return fileName; } set { fileName = value; } }
        public long FileSize { get { return fileSize; } set { fileSize = value; } }
        public List<Pear> Pears { get { return pears; } set { pears = value; } }
        public int PearsCount { get { return pears.Count; } }

        public TransferFileDetails(string fileName, long fileSize)
        {
            this.fileName = fileName;
            this.fileSize = fileSize;
            pears = new List<Pear>();
        }
    }

    public class Pear
    {
        private string ip;
        private int port;

        public string Ip { get { return ip; } set { ip = value; } }
        public int Port { get { return port; } set { port = value; } }

        public Pear(string ip, int port)
        {
            this.ip = ip;
            this.port = port;
        }
    }
}
