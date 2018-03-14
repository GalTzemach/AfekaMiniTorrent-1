namespace MiniTorrent
{
    public class ClientUploadDownload
    {
        public string FileName { get; set; }
        public long FromByte { get; set; }
        public long ToByte { get; set; }

        public ClientUploadDownload(string fileName, long fromByte, long toByte)
        {
            this.FileName = fileName;
            this.FromByte = fromByte;
            this.ToByte = toByte;
        }
    }
}
