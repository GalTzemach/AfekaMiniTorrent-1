namespace MiniTorrent
{
    public class FileRequest
    {
        public string FileName { get; set; }
        public long FromByte { get; set; }
        public long ToByte { get; set; }

        public FileRequest(string fileName, long fromByte, long toByte)
        {
            this.FileName = fileName;
            this.FromByte = fromByte;
            this.ToByte = toByte;
        }
    }
}
