using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    [Serializable]
    public class Users : IEquatable<Users>
    {
        private string userName;
        private string password;
        private string ip;
        private int upPort;
        private int downPort;
        List<FileDetails> fileList;

        public string UserName { get { return userName; } set { userName = value; } }
        public string Password { get { return password; } set { password = value; } }
        public string Ip { get { return ip; } set { ip = value; } }
        public int UpPort { get { return upPort; } set { upPort = value; } }
        public int DownPort { get { return downPort; } set { downPort = value; } }
        public List<FileDetails> FileList { get { return fileList; } set { fileList = value; } }

        public Users(string userName, string password, string ip, int upPort, int downPort)
        {
            this.userName = userName;
            this.password = password;
            this.ip = ip;
            this.upPort = upPort;
            this.downPort = downPort;
            fileList = new List<FileDetails>();
        }

        public override int GetHashCode()
        {
            if (userName == null) return 0;
            return userName.GetHashCode();
        }

        public bool Equals(Users other)
        {
            return this.userName == other.userName;
        }
    }
}
