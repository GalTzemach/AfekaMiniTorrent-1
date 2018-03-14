namespace MiniTorrent
{
    public class ClientSearchReq
    {
        public string FileName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public ClientSearchReq(string fileName, string userName, string password)
        {
            this.FileName = fileName;
            this.UserName = userName;
            this.Password = password;
        }
    }
}
