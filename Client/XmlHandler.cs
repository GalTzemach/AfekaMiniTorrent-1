using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;

namespace MiniTorrent
{
    class XmlHandler
    {
        private static string fName = "MyConfig.xml";
        private XmlNodeType type;
        XmlTextReader reader;

        public Users[] readXmlFileAndCreateUser()
        {
            string userName = "";
            string password = "";
            string upload = "";
            string download = "";
            string ip = "";
            int upPort = 0;
            int downPort = 0;
            string fileName = "";
            long fileSize = 0;
            Users[] users = new Users[2];
            
            reader = new XmlTextReader(fName);

            while (reader.Read())
            {
                type = reader.NodeType;
                if (type == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "username":
                            reader.Read();
                            userName = reader.Value;
                            if (userName.Trim().Equals(""))
                            {
                                reader.Close();
                                return null;
                            }
                            break;

                        case "password":
                            reader.Read();
                            password = reader.Value;
                            if (password.Trim().Equals(""))
                            {
                                reader.Close();
                                return null;
                            }
                            break;

                        case "upload":
                            reader.Read();
                            upload = reader.Value;
                            if (upload.Trim().Equals("") || !Directory.Exists(upload))
                            {
                                reader.Close();
                                return null;
                            }
                            break;

                        case "download":
                            reader.Read();
                            download = reader.Value;
                            if (download.Trim().Equals("") || !Directory.Exists(download))
                            {
                                reader.Close();
                                return null;
                            }
                            break;

                        case "ip":
                            reader.Read();
                            ip = reader.Value;
                            if (ip.Trim().Equals(""))
                            {
                                reader.Close();
                                return null;
                            }
                            break;

                        case "upPort":
                            reader.Read();
                            upPort = Convert.ToInt32(reader.Value);
                            if (upPort == 0)
                            {
                                reader.Close();
                                return null;
                            }
                            break;

                        case "downPort":
                            reader.Read();
                            downPort = Convert.ToInt32(reader.Value);
                            if (downPort == 0)
                            {
                                reader.Close();
                                return null;
                            }
                            users[0] = new Users(userName, password, upload, download, ip, upPort, downPort);
                            users[1] = new Users(userName, password, ip, upPort, downPort);
                            break;

                        case "FileName":
                            reader.Read();
                            fileName = reader.Value;
                            break;

                        case "FileSize":
                            reader.Read();
                            if(File.Exists(upload + "\\" + fileName))
                            {
                                fileSize = Convert.ToInt64(reader.Value);
                                users[0].FileList.Add(new FileDetails(fileName, fileSize));
                                users[1].FileList.Add(new FileDetails(fileName, fileSize));
                            }                            
                            break;
                    }
                }
            }
            reader.Close();
            return users;
        }

        public void rebuildXml(Users currentUser, Dictionary<string, long> dic)
        {
            string userName = currentUser.UserName;
            string password = currentUser.Password;
            string upload = currentUser.UpLoc;
            string download = currentUser.DownLoc;

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "\t";
            XmlWriter writer = XmlWriter.Create(fName, settings);

            writer.WriteStartDocument();
            writer.WriteStartElement("User");

            writer.WriteStartElement("username");
            writer.WriteString(userName);
            writer.WriteEndElement(); //username

            writer.WriteStartElement("password");
            writer.WriteString(password);
            writer.WriteEndElement(); //password

            writer.WriteStartElement("upload");
            writer.WriteString(upload);
            writer.WriteEndElement(); //upload

            writer.WriteStartElement("download");
            writer.WriteString(download);
            writer.WriteEndElement(); //download

            writer.WriteStartElement("ip");
            writer.WriteString(currentUser.Ip);
            writer.WriteEndElement(); //ip

            writer.WriteStartElement("upPort");
            writer.WriteString(currentUser.UpPort.ToString());
            writer.WriteEndElement(); //upPort

            writer.WriteStartElement("downPort");
            writer.WriteString(currentUser.DownPort.ToString());
            writer.WriteEndElement(); //downPort

            addFileListToXml(writer, dic);
            writer.WriteEndElement(); //User
            writer.WriteEndDocument();
            writer.Close();
        }

        public void addFileListToXml(XmlWriter writer, Dictionary<string, long> files)
        {
            foreach (string key in files.Keys)
            {
                writer.WriteStartElement("File");
                writer.WriteStartElement("FileName");
                writer.WriteString(key);
                writer.WriteEndElement(); //FileName

                writer.WriteStartElement("FileSize");
                writer.WriteString(files[key].ToString());
                writer.WriteEndElement(); //FileSize
                writer.WriteEndElement(); //File
            }
        }
    }
}
