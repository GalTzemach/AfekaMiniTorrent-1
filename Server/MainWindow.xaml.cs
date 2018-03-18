using DAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;

namespace Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string SERVER_IP = "192.168.1.156";
        private const int SERVER_LISTENER_PORT = 8006;

        private delegate void delegate1(string s);
        private TcpListener serverListener;
        private static BackgroundWorker bw;
        private static ServerInformation serverInfo;
        private static OperationsDB DB = new OperationsDB();

        public MainWindow()
        {
            DB.LogOffAllUsers();
            DB.DeleteAllFiles();

            InitializeComponent();

            serverInfo = new ServerInformation();
            serverInfo.Show();

            bw = new BackgroundWorker();
            bw.DoWork += Bw_DoWork;

            ServerStart();
        }

        // Writing a message to Log in the background
        private void Bw_DoWork(object sender, DoWorkEventArgs e)
        {
            delegate1 delegate1 = new delegate1(WriteToLog);
            TextBox_serverLog.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, delegate1, e.Argument.ToString());
        }

        private void WriteToLog(string message)
        {
            TextBox_serverLog.AppendText(message);
            TextBox_serverLog.ScrollToEnd();
        }

        // Server start listening to clients request.
        public async void ServerStart()
        {
            try
            {
                serverListener = new TcpListener(IPAddress.Parse(SERVER_IP), SERVER_LISTENER_PORT);
                serverListener.Start();
                TextBox_serverLog.AppendText("Server start Listening for new client request.\n");

                while (true)
                {
                    TcpClient clientSocket = await serverListener.AcceptTcpClientAsync();
                    HandleAClinet client = new HandleAClinet(clientSocket);
                }
            }

            catch (Exception e)
            {
                TextBox_serverLog.AppendText(e.ToString());
            }

            finally
            {
                TextBox_serverLog.AppendText("Server stop.");
                serverListener.Stop();
            }
        }

        // This class handle each client.
        public class HandleAClinet
        {
            private TcpClient clientSocket;
            private User currentUser;
            private NetworkStream stream;

            public HandleAClinet(TcpClient clientSocket)
            {
                this.clientSocket = clientSocket;
                this.stream = this.clientSocket.GetStream();

                Thread thread = new Thread(ReceiveUserInfo);
                thread.Start();
            }

            private async void ReceiveUserInfo()
            {
                string jsonString; // As string.
                byte[] jsonBytes; // As json.
                byte[] jsonSize = new byte[4]; // The Size (int32).

                // Read size.
                await stream.ReadAsync(jsonSize, 0, 4);
                jsonBytes = new byte[BitConverter.ToInt32(jsonSize, 0)];

                // Read User object as Json. 
                await stream.ReadAsync(jsonBytes, 0, jsonBytes.Length);

                // Convert to user object from Json.
                jsonString = ASCIIEncoding.ASCII.GetString(jsonBytes);
                currentUser = JsonConvert.DeserializeObject<User>(jsonString);

                while (bw.IsBusy) ;
                bw.RunWorkerAsync(currentUser.UserName + " connected to server.\n");

                // Update users list.
                AddNewUser(stream, currentUser);
            }

            private async void AddNewUser(NetworkStream stream, User newUser)
            {
                try
                {
                    byte[] answer = new byte[1];
                    int status = 0;
                    // 0 = User not exist.
                    // 1 = Connecting the user.
                    // 2 = User alredy connected.
                    // 3 = User Disable.

                    if (newUser != null)
                    {
                        status = DB.GetUserStatus(newUser.UserName, newUser.Password);
                        answer[0] = (byte)status;

                        if (status == 1)
                        {
                            if (!serverInfo.ActiveUsers.Contains(newUser))
                            {
                                // Add user files to server.
                                serverInfo.AddUserFiles(newUser, DB);

                                await stream.WriteAsync(answer, 0, 1);

                                FileRequestHandler();
                            }
                        }

                        else
                        {
                            await stream.WriteAsync(answer, 0, 1);
                        }
                    }
                }

                catch (Exception e)
                {
                    MessageBoxResult result = MessageBox.Show(e.ToString(), "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                }
            }

            private async void FileRequestHandler()
            {
                while (true)
                {
                    string jsonFile; // As string.
                    byte[] jsonBytes;// As json.
                    byte[] jsonSize = new byte[4]; // The Size (int32).
                    List<TransferFileDetails> filesSearchResult = new List<TransferFileDetails>();

                    try
                    {
                        // Read size.
                        await stream.ReadAsync(jsonSize, 0, 4);
                        jsonBytes = new byte[BitConverter.ToInt32(jsonSize, 0)];

                        // Read SearchRequest object As json.
                        await stream.ReadAsync(jsonBytes, 0, jsonBytes.Length);

                        // Convert to SearchRequest object from Json.
                        jsonFile = ASCIIEncoding.ASCII.GetString(jsonBytes);
                        ClientSearchReq searchRequst = JsonConvert.DeserializeObject<ClientSearchReq>(jsonFile);

                        // Execute when user press X on his window.
                        if (searchRequst.FileName.Equals("exit"))
                        {
                            stream.Close();
                            serverInfo.DeleteUserFiles(currentUser);
                            DB.LogOffUser(currentUser.UserName);

                            foreach (FileDetails file in currentUser.FileList)
                                DB.DeletePeerFromFile(file.FileName, file.FileSize);

                            while (bw.IsBusy) ;
                            bw.RunWorkerAsync(currentUser.UserName + " is Loged off now.\n");

                            break;
                        }

                        while (bw.IsBusy) ;
                        bw.RunWorkerAsync(currentUser.UserName + " send file request.\n");

                        bool fileExistInServer = false;

                        // Check if user is active.
                        if (serverInfo.IsActiveUser(searchRequst.UserName, searchRequst.Password))
                        {
                            foreach (FileDetails file in serverInfo.ServerFileList.Keys)
                            {
                                // Looking for some or all of the fileName.
                                if (file.FileName.Contains(searchRequst.FileName) || searchRequst.FileName == "*")
                                {
                                    fileExistInServer = true;
                                    filesSearchResult.Add(CreateTransferFileDetails(file));
                                }
                            }

                            if (fileExistInServer)
                                SendFileListAsJson(filesSearchResult);

                            else
                                // File not found in Server.
                                stream.WriteByte(0);
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBoxResult result = MessageBox.Show(e.ToString(), "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    }
                }
            }

            private TransferFileDetails CreateTransferFileDetails(FileDetails file)
            {
                TransferFileDetails transferFile = new TransferFileDetails(file.FileName, file.FileSize);

                foreach (User user in serverInfo.ServerFileList[file])
                    transferFile.PeersList.Add(new Peer(user.Ip, user.UpPort));

                return transferFile;
            }

            private async void SendFileListAsJson(List<TransferFileDetails> transferFileList)
            {
                // File/s found in Server.
                stream.WriteByte(1);

                // Convert transferFileList (List<TransferFileDetails>) to json before send.
                string jsonString = JsonConvert.SerializeObject(transferFileList);
                byte[] jsonFile = ASCIIEncoding.ASCII.GetBytes(jsonString);
                byte[] jsonSize = BitConverter.GetBytes(jsonFile.Length);

                // Write size.
                await stream.WriteAsync(jsonSize, 0, jsonSize.Length);

                // Write as json.
                await stream.WriteAsync(jsonFile, 0, jsonFile.Length);
            }
        }

        private void ServerClose(object sender, CancelEventArgs e)
        {
            DB.LogOffAllUsers();
            DB.DeleteAllFiles();

            serverListener.Stop();
            serverInfo.Close();
        }
    }
}
