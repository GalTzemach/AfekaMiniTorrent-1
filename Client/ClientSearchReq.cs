using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniTorrent
{
    public class ClientSearchReq
    {
        private string fileName;
        private string userName;
        private string password;

        public string FileName { get { return fileName; } set { fileName = value; } }
        public string UserName { get { return userName; } set { userName = value; } }
        public string Password { get { return password; } set { password = value; } }

        public ClientSearchReq(string fileName, string userName, string password)
        {
            this.fileName = fileName;
            this.userName = userName;
            this.password = password;
        }
    }
}
