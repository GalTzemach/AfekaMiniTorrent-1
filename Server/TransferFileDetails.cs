using System;
using System.Collections.Generic;

namespace Server
{
    [Serializable]
    public class TransferFileDetails
    {
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public List<Peer> PeersList { get; set; }

        public TransferFileDetails(string fileName, long fileSize)
        {
            this.FileName = fileName;
            this.FileSize = fileSize;
            this.PeersList = new List<Peer>();
        }
    }

    [Serializable]
    public class Peer
    {
        public string Ip { get; set; }
        public int Port { get; set; }

        public Peer(string ip, int port)
        {
            this.Ip = ip;
            this.Port = port;
        }
    }
}
