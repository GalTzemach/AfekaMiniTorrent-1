using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MiniTorrent
{
    public class TransferFileDetails
    {
        private string fileName;
        private long fileSize;
        private List<Peer> peerList;

        public string FileName { get { return fileName; } set { fileName = value; } }
        public long FileSize { get { return fileSize; } set { fileSize = value; } }
        public List<Peer> PeersList { get { return peerList; } set { peerList = value; } }
        public int NumOfPeers { get { return peerList.Count; } }

        public TransferFileDetails(string fileName, long fileSize)
        {
            this.fileName = fileName;
            this.fileSize = fileSize;
            this.peerList = new List<Peer>();
        }
    }

    public class Peer
    {
        private string ip;
        private int port;

        public string Ip { get { return ip; } set { ip = value; } }
        public int Port { get { return port; } set { port = value; } }

        public Peer(string ip, int port)
        {
            this.ip = ip;
            this.port = port;
        }
    }
}
