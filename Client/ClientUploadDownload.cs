using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniTorrent
{
    public class ClientUploadDownload
    {
        private string fileName;
        private long fromByte;
        private long toByte;

        public string FileName { get { return fileName; } set { fileName = value; } }
        public long FromByte { get { return fromByte; } set { fromByte = value; } }
        public long ToByte { get { return toByte; } set { toByte = value; } }

        public ClientUploadDownload(string fileName, long fromByte, long toByte)
        {
            this.fileName = fileName;
            this.fromByte = fromByte;
            this.toByte = toByte;
        }
    }
}
