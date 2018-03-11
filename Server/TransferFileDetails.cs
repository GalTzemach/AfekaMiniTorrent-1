using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    [Serializable]
    public class TransferFileDetails
    {
        private string fileName;
        private long fileSize;
        private List<Pear> pears;

        public string FileName { get { return fileName; } set { fileName = value; } }
        public long FileSize { get { return fileSize; } set { fileSize = value; } }
        public List<Pear> Pears { get { return pears; } set { pears = value; } }

        public TransferFileDetails(string fileName, long fileSize)
        {
            this.fileName = fileName;
            this.fileSize = fileSize;
            pears = new List<Pear>();
        }
    }

    [Serializable]
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
