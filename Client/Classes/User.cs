using System;
using System.Collections.Generic;

namespace MiniTorrent
{
    [Serializable]
    public class User : IEquatable<User>
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string UploadPath { get; set; }
        public string DownloadPath { get; set; }
        public string Ip { get; set; }
        public int UpPort { get; set; }
        public int DownPort { get; set; }
        public List<FileDetails> FileList { get; set; }

        public User(string userName, string password, string uploadPath, string downloadPath, string ip, int upPort, int downPort)
        {
            this.UserName = userName;
            this.Password = password;
            this.UploadPath = uploadPath;
            this.DownloadPath = downloadPath;
            this.Ip = ip;
            this.UpPort = upPort;
            this.DownPort = downPort;
            FileList = new List<FileDetails>();
        }

        public bool Equals(User other)
        {
            return this.UserName == other.UserName;
        }
    }
}
