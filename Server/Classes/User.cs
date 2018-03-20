using System;
using System.Collections.Generic;

namespace Server
{
    [Serializable]
    public class User : IEquatable<User>
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Ip { get; set; }
        public int UpPort { get; set; }
        public int DownPort { get; set; }
        public List<FileDetails> FileList { get; set; }

        public User(string userName, string password, string ip, int upPort, int downPort)
        {
            this.UserName = userName;
            this.Password = password;
            this.Ip = ip;
            this.UpPort = upPort;
            this.DownPort = downPort;
            this.FileList = new List<FileDetails>();
        }

        public override int GetHashCode()
        {
            return UserName == null ? 0 : UserName.GetHashCode();
        }

        public bool Equals(User other)
        {
            return this.UserName == other.UserName;
        }
    }
}
