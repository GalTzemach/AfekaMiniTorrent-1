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

        //[JsonConstructor]
        //public TransferFileDetails(TransferFileDetails other)
        //{
        //    this.fileName = other.fileName;
        //    this.fileSize = other.fileSize;
        //    this.peerList = other.peerList;
        //}

        //public string FileName { get; set; }
        //public long FileSize { get; set; }
        //public List<Peer> PeerList { get; set; }
        //public int NumOfPeers { get { return PeerList.Count; } }

        //public TransferFileDetails(string fileName, long fileSize, List<Peer> peerList)
        //{
        //    this.FileName = fileName;
        //    this.FileSize = fileSize;
        //    this.PeerList = peerList;
        //}
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

        //public string Ip { get; set; }
        //public int Port { get; set; }

        //public Peer(string ip, int port)
        //{
        //    this.Ip = ip;
        //    this.Port = port;
        //}
    }
}
