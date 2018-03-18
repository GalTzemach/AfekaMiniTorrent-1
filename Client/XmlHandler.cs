using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace MiniTorrent
{
    public class XmlHandler
    {
        private string fileName;
        private XmlNodeType xmlType;
        private XmlTextReader xmlReader;

        public XmlHandler(string fileName)
        {
            this.fileName = fileName;
        }

        public User ReadUserFromXml()
        {
            string userName = "";
            string password = "";
            string uploadPath = "";
            string downloadPath = "";
            string ip = "";
            int upPort = 0;
            int downPort = 0;
            string fileName = "";
            long fileSize = 0;
            User user = null;

            this.xmlReader = new XmlTextReader(this.fileName);

            while (xmlReader.Read())
            {
                xmlType = xmlReader.NodeType;

                if (xmlType == XmlNodeType.Element)
                {
                    switch (xmlReader.Name)
                    {
                        case "username":
                            xmlReader.Read();
                            userName = xmlReader.Value;
                            if (string.IsNullOrEmpty(userName.Trim()))
                            {
                                xmlReader.Close();
                                return null;
                            }
                            break;

                        case "password":
                            xmlReader.Read();
                            password = xmlReader.Value;
                            if (string.IsNullOrEmpty(password.Trim()))
                            {
                                xmlReader.Close();
                                return null;
                            }
                            break;

                        case "uploadPath":
                            xmlReader.Read();
                            uploadPath = xmlReader.Value;
                            if (string.IsNullOrEmpty(uploadPath.Trim()) || !Directory.Exists(uploadPath))
                            {
                                xmlReader.Close();
                                return null;
                            }
                            break;

                        case "downloadPath":
                            xmlReader.Read();
                            downloadPath = xmlReader.Value;
                            if (string.IsNullOrEmpty(downloadPath.Trim()) || !Directory.Exists(downloadPath))
                            {
                                xmlReader.Close();
                                return null;
                            }
                            break;

                        case "ip":
                            xmlReader.Read();
                            ip = xmlReader.Value;
                            if (string.IsNullOrEmpty(ip.Trim()))
                            {
                                xmlReader.Close();
                                return null;
                            }
                            break;

                        case "upPort":
                            xmlReader.Read();
                            upPort = Convert.ToInt32(xmlReader.Value);
                            if (upPort == 0)
                            {
                                xmlReader.Close();
                                return null;
                            }
                            break;

                        case "downPort":
                            xmlReader.Read();
                            downPort = Convert.ToInt32(xmlReader.Value);
                            if (downPort == 0)
                            {
                                xmlReader.Close();
                                return null;
                            }
                            user = new User(userName, password, uploadPath, downloadPath, ip, upPort, downPort);
                            //user[1] = new User(userName, password, ip, upPort, downPort);
                            break;

                        case "FileName":
                            xmlReader.Read();
                            fileName = xmlReader.Value;
                            break;

                        case "FileSize":
                            xmlReader.Read();
                            if (File.Exists(uploadPath + "\\" + fileName))
                            {
                                fileSize = Convert.ToInt64(xmlReader.Value);
                                user.FileList.Add(new FileDetails(fileName, fileSize));
                                //user[1].FileList.Add(new FileDetails(fileName, fileSize));
                            }
                            break;
                    }
                }
            }
            xmlReader.Close();
            return user;
        }

        public void WriteUserToXml(User currentUser, Dictionary<string, long> files)
        {
            XmlWriterSettings xmlSettings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "\t"
            };

            XmlWriter xmlWriter = XmlWriter.Create(fileName, xmlSettings);

            xmlWriter.WriteStartDocument();

            xmlWriter.WriteStartElement("User");

            xmlWriter.WriteStartElement("username");
            xmlWriter.WriteString(currentUser.UserName);
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("password");
            xmlWriter.WriteString(currentUser.Password);
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("uploadPath");
            xmlWriter.WriteString(currentUser.UploadPath);
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("downloadPath");
            xmlWriter.WriteString(currentUser.DownloadPath);
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("ip");
            xmlWriter.WriteString(currentUser.Ip);
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("upPort");
            xmlWriter.WriteString(currentUser.UpPort.ToString());
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("downPort");
            xmlWriter.WriteString(currentUser.DownPort.ToString());
            xmlWriter.WriteEndElement();

            AddUserFilesToXml(xmlWriter, files);

            xmlWriter.WriteEndElement(); //User

            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
        }

        public void AddUserFilesToXml(XmlWriter writer, Dictionary<string, long> files)
        {
            foreach (string fileName in files.Keys)
            {
                writer.WriteStartElement("File");

                writer.WriteStartElement("FileName");
                writer.WriteString(fileName);
                writer.WriteEndElement();

                writer.WriteStartElement("FileSize");
                writer.WriteString(files[fileName].ToString());
                writer.WriteEndElement();

                writer.WriteEndElement(); //File
            }
        }
    }
}
