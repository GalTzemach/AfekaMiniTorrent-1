using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace MiniTorrent
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //error massegeges 
        private string emptyFieldsError = "All the fields have to be filled";
        private string noUserError = "Username or password are incorrect";
        private string wrongPathError = "the upload/download path is incorrect";
        private string userAlredySigned = "The username is alredy signed in";
        private string userBlocked = "The username is blocked";
        private string confInCorrect = "The ConfigFile is incorrect or not exist";
        //error massegeges 

        private string fName = "MyConfig.xml";  //config file name
        private string serverIp = "192.168.1.156";//server Ip
        private int upPort = 8005;
        private int serverPort = 8006;
        private List<FilesAndStatus> uploadFiles;
        private XmlHandler xmlHandler = new XmlHandler();
        private Users currentUser;
        private bool startOver = false;

        public MainWindow()
        {
            InitializeComponent();
            Hide();
            if (File.Exists(fName)) //if there is a config file and its ok, no need for re-login
                buildJsonFileAndSend();
            else
            {
                startOver = true;
                Show();
            }
                
        }

        private void signUp_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://localhost:63525/HomePage.aspx"); //signup web
        }

        private void signIn_Click(object sender, RoutedEventArgs e)
        {
            if(checkedFields())
            {
                buildXmlFile();
                buildJsonFileAndSend();
            }            
        }

        //check sign in fields
        public bool checkedFields() 
        {
            //field are filled
            if(user.Text.Trim() == "" || pass.Text.Trim() == "" || upLoc.Text.Trim() == "" 
                || downLoc.Text.Trim() == "")
            {
                errorLabel.Content = emptyFieldsError; 
                errorLabel.Visibility = Visibility.Visible;
                return false;
            }

            else if(!Directory.Exists(upLoc.Text.Trim()) || !Directory.Exists(downLoc.Text.Trim()))
            {
                errorLabel.Content = wrongPathError;
                errorLabel.Visibility = Visibility.Visible;
                return false;
            }

            else
            {
                errorLabel.Visibility = Visibility.Hidden;
                return true;
            }
        }

        public void buildXmlFile()
        {            
            string userName = user.Text.Trim();
            string password = pass.Text.Trim();
            string upload = upLoc.Text.Trim();
            string download = downLoc.Text.Trim();

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
            writer.WriteString(GetLocalIPAddress());
            writer.WriteEndElement(); //ip

            writer.WriteStartElement("upPort");
            writer.WriteString(upPort.ToString());
            writer.WriteEndElement(); //upPort

            writer.WriteStartElement("downPort");
            writer.WriteString(serverPort.ToString());
            writer.WriteEndElement(); //downPort

            addFileListToXml(writer);
            writer.WriteEndElement(); //User
            writer.WriteEndDocument();
            writer.Close();
        }

        public async void buildJsonFileAndSend() //json file for user login
        {
            try
            {
                Users[] users = new Users[2];
                users = xmlHandler.readXmlFileAndCreateUser();

                if(users != null)
                {
                    if (!startOver)
                        updateFileList(users);

                    currentUser = users[0];

                    TcpClient client = new TcpClient();

                    await client.ConnectAsync(serverIp, serverPort);
                    NetworkStream ns = client.GetStream();

                    string jasonStriing = JsonConvert.SerializeObject(users[1]);

                    byte[] jsonFile = ASCIIEncoding.ASCII.GetBytes(jasonStriing);
                    byte[] jsonFileLength = BitConverter.GetBytes(jsonFile.Length);

                    await ns.WriteAsync(jsonFileLength, 0, jsonFileLength.Length);
                    await ns.WriteAsync(jsonFile, 0, jsonFile.Length);

                    serverResponse(ns, currentUser);
                }
                else
                {
                    errorLabel.Content = confInCorrect;
                    errorLabel.Visibility = Visibility.Visible;
                    startOver = true;
                    this.Show();
                }
                                
            }
            catch (Exception e)
            {
                MessageBoxResult result = MessageBox.Show("Server Offline");
                this.Close();
            }
        }

        //if its login with config file, check for file changes in locations
        public void updateFileList(Users [] users) 
        {
            Dictionary<string, long> dic = getAllFiles(users[0].UpLoc);
            users[0].FileList.Clear();
            users[1].FileList.Clear();

            foreach (String key in dic.Keys)
            {
                FileDetails tempFile = new FileDetails(key, dic[key]);
                users[0].FileList.Add(tempFile);
                users[1].FileList.Add(tempFile);
            }

            xmlHandler.rebuildXml(users[0], dic);
        }

        //pc can have multiply ip addresses
        public string GetLocalIPAddress() 
        {
            List<string> ipAdress = new List<string>();
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    ipAdress.Add(ip.ToString());                
            }

            if (ipAdress.Count == 1)
                return ipAdress[0];
            else
            {
                IpMsgBox msg = new IpMsgBox(ipAdress);
                msg.ShowDialog();
                return msg.chosenIp;
            }            
        }

        //server response to login request
        public async void serverResponse(NetworkStream ns, Users currentUser) 
        {
            byte[] answer = new byte[1];
            await ns.ReadAsync(answer, 0, 1);            

            if (answer[0] == 0) //wrong username or password
            {
                errorLabel.Content = noUserError;
                errorLabel.Visibility = Visibility.Visible;
                startOver = true;
                this.Show();
            }

            else if (answer[0] == 1)// all ok
            {
                //open app
                if (uploadFiles == null)
                    getAllFiles(currentUser.UpLoc.Trim());
                UserControlPanel cp = new UserControlPanel(ns, uploadFiles, currentUser);
                cp.Show();
                this.Close();
                errorLabel.Visibility = Visibility.Hidden;
            }

            else if (answer[0] == 2)// all ok
            {
                errorLabel.Content = userAlredySigned;
                errorLabel.Visibility = Visibility.Visible;
                startOver = true;
                this.Show();
            }
            else
            {
                errorLabel.Content = userBlocked;
                errorLabel.Visibility = Visibility.Visible;
                startOver = true;
                this.Show();
            }
        }

        public Dictionary<string, long> getAllFiles(string path)
        {
            Dictionary<string, long> dic = new Dictionary<string, long>();
            uploadFiles = new List<FilesAndStatus>();

            foreach (string file in Directory.GetFiles(path))
            {
                string fileName = System.IO.Path.GetFileName(file);
                FileInfo f = new FileInfo(file);
                long fileSize = f.Length;
                dic[fileName] = fileSize;
                uploadFiles.Add(new FilesAndStatus(fileName, fileSize, "Standby"));
            }

            foreach (string dir in Directory.GetDirectories(path))
            {                
                foreach(string file in Directory.GetFiles(dir))
                {
                    string fileName = System.IO.Path.GetFileName(file);
                    FileInfo f = new FileInfo(file);
                    long fileSize = f.Length;
                    dic[fileName] = fileSize;
                    uploadFiles.Add(new FilesAndStatus(fileName, fileSize, "Standby"));
                }                
            }
            return dic;
        }

        public void addFileListToXml(XmlWriter writer)
        {
            Dictionary<string, long> files = getAllFiles(upLoc.Text.Trim());

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

        private void upLocBut_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.ShowDialog();
                upLoc.Text = dialog.SelectedPath;
            }
        }

        private void dwLocBut_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.ShowDialog();
                downLoc.Text = dialog.SelectedPath;
            }
        }
    }
}
