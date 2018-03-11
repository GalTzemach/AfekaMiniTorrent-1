using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DAL;

namespace Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcpListener serverListener = null;
        private static BackgroundWorker bw = new BackgroundWorker();
        private delegate void myDelegate(string s);
        //private static ServerInformation serverInfo;
        private string serverIp = "192.168.1.156"; //server ip address
        private static OperationsDB dba = new OperationsDB();

        public MainWindow()
        {
            dba.logOffAllUsers();
            dba.clearFileTable();
            InitializeComponent();

            //serverInfo = new ServerInformation();
            //serverInfo.Show();
            bw.DoWork += bw_DoWork;
            serverStart();
        }

        //add strings to server log
        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            myDelegate deli = new myDelegate(appendToTextBox);
            serverLog.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, deli, e.Argument.ToString());
        }

        private void appendToTextBox(string s)
        {
            serverLog.AppendText(s);
            serverLog.ScrollToEnd();
        }

        //start listening to clients request
        public async void serverStart()
        {
            try
            {
                serverListener = new TcpListener(IPAddress.Parse(serverIp), 8006);
                int counter = 0;
                serverListener.Start();
                serverLog.AppendText("Server Started\n");

                counter = 0;
                while (true)
                {
                    serverLog.AppendText("wait for connections\n");
                    TcpClient clientSocket = await serverListener.AcceptTcpClientAsync();
                    counter += 1;
                    serverLog.AppendText("Client No:" + Convert.ToString(counter) + " started!\n");
                    handleClinet client = new handleClinet();
                    client.startClient(clientSocket, Convert.ToString(counter));
                }
            }
            catch (Exception e)
            {
                serverLog.AppendText(e.ToString());
            }
            finally
            {
                serverLog.AppendText("exit");
                serverListener.Stop();
            }
        }

        //Class to handle each client request separatly
        public class handleClinet
        {
            private TcpClient clientSocket;
            private string clNo;
            private Users currentUser;

            public void startClient(TcpClient inClientSocket, string clineNo)
            {
                this.clientSocket = inClientSocket;
                this.clNo = clineNo;
                Thread t = new Thread(newClient);
                t.Start();
            }
            private void newClient()
            {
                NetworkStream ns = clientSocket.GetStream();
                receiveJsonFromClient(ns);
            }

            private async void receiveJsonFromClient(NetworkStream ns)
            {
                string jsonFile;
                byte[] jsonBytes;
                byte[] jsonLengthBytes = new byte[4]; //int32

                await ns.ReadAsync(jsonLengthBytes, 0, 4); // int32

                while (bw.IsBusy) ;
                bw.RunWorkerAsync("Json length received\n");

                jsonBytes = new byte[BitConverter.ToInt32(jsonLengthBytes, 0)];
                await ns.ReadAsync(jsonBytes, 0, jsonBytes.Length);

                while (bw.IsBusy) ;
                bw.RunWorkerAsync("Json received\n");

                jsonFile = ASCIIEncoding.ASCII.GetString(jsonBytes);

                currentUser = JsonConvert.DeserializeObject<Users>(jsonFile);

                addNewUser(ns, currentUser);
            }

            private async void addNewUser(NetworkStream ns, Users currentUser)
            {
                try
                {
                    byte[] answer = new byte[1];
                    int state = 0;

                    if (currentUser != null)
                    {
                        state = dba.getUser(currentUser.UserName, currentUser.Password);
                        if (state == 1)
                        {
                            if (!serverInfo.OnLineUsers.Contains(currentUser))
                            {
                                serverInfo.addUserFiles(currentUser, dba);
                                answer[0] = 1;
                                await ns.WriteAsync(answer, 0, 1);
                                clientRequestHandler(ns);
                            }
                        }
                        else
                        {
                            answer[0] = (byte)state;
                            await ns.WriteAsync(answer, 0, 1);
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBoxResult result = MessageBox.Show(e.ToString(), "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                }
            }

            private async void clientRequestHandler(NetworkStream ns)
            {
                while (true)
                {
                    string jsonFile;
                    byte[] jsonBytes;
                    byte[] jsonLengthBytes = new byte[4]; //int32
                    List<TransferFileDetails> tfList = new List<TransferFileDetails>();
                    try
                    {
                        await ns.ReadAsync(jsonLengthBytes, 0, 4); // int32

                        jsonBytes = new byte[BitConverter.ToInt32(jsonLengthBytes, 0)];
                        await ns.ReadAsync(jsonBytes, 0, jsonBytes.Length);

                        while (bw.IsBusy) ;
                        bw.RunWorkerAsync("Search request received\n");

                        jsonFile = ASCIIEncoding.ASCII.GetString(jsonBytes);

                        ClientSearchReq csr = JsonConvert.DeserializeObject<ClientSearchReq>(jsonFile);

                        if (csr.FileName.Equals("exit"))
                        {
                            ns.Close();
                            serverInfo.removeUserFiles(currentUser);
                            dba.logOffUser(currentUser.UserName);
                            foreach (FileDetails f in currentUser.FileList)
                                dba.removeFile(f.FileName, f.FileSize);
                            while (bw.IsBusy) ;
                            bw.RunWorkerAsync(currentUser.UserName + " exit\n");
                            break;
                        }

                        bool fileFoundOnServer = false;
                        if (serverInfo.isUserActive(csr.UserName, csr.Password))
                        {
                            foreach (FileDetails sf in serverInfo.ServerFiles.Keys)
                            {
                                if (sf.FileName.Contains(csr.FileName))
                                {
                                    fileFoundOnServer = true;
                                    tfList.Add(filesDetailsbuilder(sf));
                                }
                            }
                            if (fileFoundOnServer)
                                createAndSendJson(ns, tfList);
                            else
                                ns.WriteByte(0);
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBoxResult result = MessageBox.Show(e.ToString(), "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    }
                }
            }

            private TransferFileDetails filesDetailsbuilder(FileDetails file)
            {
                TransferFileDetails tf = new TransferFileDetails(file.FileName, file.FileSize);

                foreach (Users u in serverInfo.ServerFiles[file])
                    tf.Pears.Add(new Pear(u.Ip, u.UpPort));

                return tf;
            }

            private async void createAndSendJson(NetworkStream ns, List<TransferFileDetails> tfList)
            {
                ns.WriteByte(1);
                string jasonStriing = JsonConvert.SerializeObject(tfList);

                List<TransferFileDetails> tfd = JsonConvert.DeserializeObject<List<TransferFileDetails>>(jasonStriing);

                byte[] jsonFile = ASCIIEncoding.ASCII.GetBytes(jasonStriing);
                byte[] jsonFileLength = BitConverter.GetBytes(jsonFile.Length);

                await ns.WriteAsync(jsonFileLength, 0, jsonFileLength.Length);
                await ns.WriteAsync(jsonFile, 0, jsonFile.Length);
            }
        }

        private void serverClose(object sender, CancelEventArgs e)
        {
            dba.logOffAllUsers();
            dba.clearFileTable();
            serverInfo.Close();
        }




    }
}
