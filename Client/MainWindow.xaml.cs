using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Xml;

namespace MiniTorrent
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Error massegeges.
        private string emptyFields = "All the fields have to be filled";
        private string userNotExist = "Username or password are incorrect";
        private string incorrectPath = "the upload/download path is incorrect";
        private string userAlreadyConnected = "The username is alredy signed in";
        private string userDisable = "The username is blocked";
        private string IncorrectConfigFile = "The ConfigFile is incorrect or not exist";

        private const string SERVER_IP = "192.168.1.156";
        private const string CONFIG_FILE_NAME = "MyConfig.xml";
        private const int UP_PORT = 8005;
        private const int SERVER_PORT = 8006;

        private NetworkStream stream;
        private List<FileStatus> uploadFiles;
        private XmlHandler xmlHandler;
        private User currentUser;
        private bool startOver;

        public MainWindow()
        {
            xmlHandler = new XmlHandler(CONFIG_FILE_NAME);
            startOver = false;

            InitializeComponent();
            Hide();

            if (File.Exists(CONFIG_FILE_NAME))
            {
                // Config file exist, no re-login required.
                SendUserAsJsonToServer();
            }

            else
            {
                startOver = true;
                Show();
            }

        }

        private void SignUp_Click(object sender, RoutedEventArgs e)
        {
            // Opens a new user registration page on the web portal.
            Process.Start("http://localhost:62053/CreateNewUser.aspx");
        }

        private void SignIn_Click(object sender, RoutedEventArgs e)
        {
            if (CheckFields())
            {
                BuildXmlFile();
                SendUserAsJsonToServer();
            }
        }

        public bool CheckFields()
        {
            // Ckeck empty fields.
            if (string.IsNullOrEmpty(userName.Text.Trim()) ||
                string.IsNullOrEmpty(password.Text.Trim()) ||
                string.IsNullOrEmpty(uploadPath.Text.Trim()) ||
                string.IsNullOrEmpty(downloadPath.Text.Trim()))
            {
                errorLabel.Content = emptyFields;
                errorLabel.Visibility = Visibility.Visible;
                return false;
            }

            // Check if directories are exist.
            if (!Directory.Exists(uploadPath.Text.Trim()) ||
                     !Directory.Exists(downloadPath.Text.Trim()))
            {
                errorLabel.Content = incorrectPath;
                errorLabel.Visibility = Visibility.Visible;
                return false;
            }

            // Everything is fine
            errorLabel.Visibility = Visibility.Hidden;
            return true;
        }

        public void BuildXmlFile()
        {
            string userName = this.userName.Text.Trim();
            string password = this.password.Text.Trim();
            string upload = uploadPath.Text.Trim();
            string download = downloadPath.Text.Trim();

            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "\t"
            };

            XmlWriter writer = XmlWriter.Create(CONFIG_FILE_NAME, settings);

            writer.WriteStartDocument();
            writer.WriteStartElement("User");

            writer.WriteStartElement("username");
            writer.WriteString(userName);
            writer.WriteEndElement(); //username

            writer.WriteStartElement("password");
            writer.WriteString(password);
            writer.WriteEndElement(); //password

            writer.WriteStartElement("uploadPath");
            writer.WriteString(upload);
            writer.WriteEndElement(); //upload

            writer.WriteStartElement("downloadPath");
            writer.WriteString(download);
            writer.WriteEndElement(); //download

            writer.WriteStartElement("ip");
            writer.WriteString(GetCorrectIPAddress());
            writer.WriteEndElement(); //ip

            writer.WriteStartElement("upPort");
            writer.WriteString(UP_PORT.ToString());
            writer.WriteEndElement(); //upPort

            writer.WriteStartElement("downPort");
            writer.WriteString(SERVER_PORT.ToString());
            writer.WriteEndElement(); //downPort

            AddUserFilesToXml(writer);
            writer.WriteEndElement(); //User
            writer.WriteEndDocument();
            writer.Close();
        }

        public void AddUserFilesToXml(XmlWriter writer)
        {
            Dictionary<string, long> files = GetAllFiles(uploadPath.Text.Trim());

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

        private Dictionary<string, long> GetAllFiles(string path)
        {
            Dictionary<string, long> files = new Dictionary<string, long>();
            uploadFiles = new List<FileStatus>();

            foreach (string file in Directory.GetFiles(path))
            {
                AddFileToUploadFiles(file, files);
            }

            foreach (string dir in Directory.GetDirectories(path))
            {
                foreach (string file in Directory.GetFiles(dir))
                {
                    AddFileToUploadFiles(file, files);
                }
            }

            return files;
        }

        private void AddFileToUploadFiles(string file, Dictionary<string, long> files)
        {
            string fileName;
            long fileSize;
            FileInfo fileInfo;

            fileName = Path.GetFileName(file);
            fileInfo = new FileInfo(file);
            fileSize = fileInfo.Length; // In bytes.
            files[fileName] = fileSize;
            uploadFiles.Add(new FileStatus(fileName, fileSize, "Standby"));
        }

        // Send for user Login.
        public async void SendUserAsJsonToServer()
        {
            try
            {
                User users = xmlHandler.ReadUserFromXml();

                if (users != null)
                {
                    if (!startOver)
                    {
                        UpdateFileList(users);
                    }

                    currentUser = users;

                    TcpClient client = new TcpClient();

                    // Connecting to server.
                    await client.ConnectAsync(SERVER_IP, SERVER_PORT);
                    stream = client.GetStream();

                    // Convert user object to json before send.
                    string jsonString = JsonConvert.SerializeObject(users);

                    byte[] jsonByte = ASCIIEncoding.ASCII.GetBytes(jsonString);
                    byte[] jsonSize = BitConverter.GetBytes(jsonByte.Length);

                    // Write size.
                    await stream.WriteAsync(jsonSize, 0, jsonSize.Length);

                    // Write user As json.
                    await stream.WriteAsync(jsonByte, 0, jsonByte.Length);

                    ServerResponse(currentUser);
                }
                else
                {
                    ShowErrorLabel(IncorrectConfigFile);
                }
            }
            catch (Exception e)
            {
                MessageBoxResult result = MessageBox.Show("Unable to connect to server\n" + e);
                this.Close();
            }
        }

        // When Login with config file, check for file changes in specific path.
        private void UpdateFileList(User user)
        {
            Dictionary<string, long> files = GetAllFiles(user.UploadPath);
            user.FileList.Clear();

            foreach (String file in files.Keys)
            {
                FileDetails tempFile = new FileDetails(file, files[file]);
                user.FileList.Add(tempFile);
            }

            xmlHandler.WriteUserToXml(user, files);
        }

        // Handle server response to Login request.
        public async void ServerResponse(User currentUser)
        {
            byte[] answer = new byte[1];

            // Read answer from server.
            await stream.ReadAsync(answer, 0, 1);
            // 0 = User not exist.
            // 1 = Connecting the user.
            // 2 = User alredy connected.
            // 3 = User Disable.

            switch (answer[0])
            {
                case 0:
                    ShowErrorLabel(userNotExist);
                    break;
                case 1:
                    if (uploadFiles == null)
                        GetAllFiles(currentUser.UploadPath.Trim());

                    UserControlPanel userControlPanel = new UserControlPanel(stream, uploadFiles, currentUser);
                    userControlPanel.Show();

                    errorLabel.Visibility = Visibility.Hidden;
                    this.Close();
                    break;
                case 2:
                    ShowErrorLabel(userAlreadyConnected);
                    break;
                case 3:
                    ShowErrorLabel(userDisable);
                    break;
            }
        }

        private void ShowErrorLabel(string theError)
        {
            errorLabel.Content = theError;
            errorLabel.Visibility = Visibility.Visible;
            startOver = true;
            this.Show();
        }

        // Because pc can have multiply ip addresses.
        public string GetCorrectIPAddress()
        {
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                return endPoint.Address.ToString();
            }
        }

        private void UploadPath_Click(object sender, RoutedEventArgs e)
        {
            using (var uploadFolderDialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                uploadFolderDialog.ShowDialog();
                uploadPath.Text = uploadFolderDialog.SelectedPath;
            }
        }

        private void DownloadPath_Click(object sender, RoutedEventArgs e)
        {
            using (var downloadFolderDialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                downloadFolderDialog.ShowDialog();
                downloadPath.Text = downloadFolderDialog.SelectedPath;
            }
        }
    }
}
