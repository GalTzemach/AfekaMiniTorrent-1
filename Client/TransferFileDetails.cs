using System.Collections.Generic;

namespace MiniTorrent
{
    public class TransferFileDetails
    {
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public int NumOfPeers { get; set; }
        public List<Peer> Peers { get; set; }

        public TransferFileDetails(string fileName, long fileSize)
        {
            this.FileName = fileName;
            this.FileSize = fileSize;
            this.Peers = new List<Peer>();
        }
    }

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
