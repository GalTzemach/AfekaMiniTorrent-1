using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniTorrent
{
    [Serializable]
    public class Users : IEquatable<Users>
    {
        private string userName;
        private string password;
        private string upLoc;
        private string downLoc;
        private string ip;
        private int upPort;
        private int downPort;
        List<FileDetails> fileList;

        public string UserName { get { return userName; } set { userName = value; } }
        public string Password { get { return password; } set { password = value; } }
        public string UpLoc { get { return upLoc; } set { upLoc = value; } }
        public string DownLoc { get { return downLoc; } set { downLoc = value; } }
        public string Ip { get { return ip; } set { ip = value; } }
        public int UpPort { get { return upPort; } set { upPort = value; } }
        public int DownPort { get { return downPort; } set { downPort = value; } }
        public List<FileDetails> FileList { get { return fileList; } set { fileList = value; } }

        public Users(string userName, string password, string upLoc, string downLoc, string ip, int upPort, int downPort)
        {
            this.userName = userName;
            this.password = password;
            this.upLoc = upLoc;
            this.downLoc = downLoc;
            this.ip = ip;
            this.upPort = upPort;
            this.downPort = downPort;
            fileList = new List<FileDetails>();
        }

        public Users(string userName, string password, string ip, int upPort, int downPort)
        {
            this.userName = userName;
            this.password = password;
            this.ip = ip;
            this.upPort = upPort;
            this.downPort = downPort;
            fileList = new List<FileDetails>();
        }

        public bool Equals(Users other)
        {
            return this.userName == other.userName;
        }
    }
}
