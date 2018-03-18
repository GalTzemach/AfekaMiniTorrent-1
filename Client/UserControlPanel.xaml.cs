using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace MiniTorrent
{
    /// <summary>
    /// Interaction logic for UserControlPanel.xaml
    /// </summary>
    public partial class UserControlPanel : Window
    {
        private const int BUFFER_SIZE = 50000;

        private NetworkStream stream;
        private static User currentUser;

        private static List<FileStatus> uploadFiles;
        private static List<FileStatus> downloadFiles;

        private delegate void delegate1();
        private static BackgroundWorker bwProgressBarUpdate;
        private static BackgroundWorker bwReflactionButton;

        private static object thisLock;
        private static bool isActiveUser;

        public UserControlPanel(NetworkStream stream, List<FileStatus> uploadFiles, User currentUser)
        {
            InitializeComponent();

            thisLock = new object();
            isActiveUser = true;
            this.stream = stream;

            UserControlPanel.downloadFiles = new List<FileStatus>();
            UserControlPanel.uploadFiles = new List<FileStatus>();
            UserControlPanel.uploadFiles = uploadFiles;
            UserControlPanel.currentUser = currentUser;
            upload_DataGrid.ItemsSource = uploadFiles;
            download_DataGrid.ItemsSource = downloadFiles;

            bwProgressBarUpdate = new BackgroundWorker();
            bwReflactionButton = new BackgroundWorker();
            bwProgressBarUpdate.DoWork += BwProgressBarUpdate_DoWork;
            bwReflactionButton.DoWork += BwReflactionButton_DoWork;

            CheckForReflactionFile();
            StartListeningForFileReq();
        }

        // Check if exist reflaction.dll file.
        public void CheckForReflactionFile()
        {
            if (File.Exists(currentUser.DownloadPath + "\\reflection.dll"))
            {
                while (bwReflactionButton.IsBusy) ;
                bwReflactionButton.RunWorkerAsync();
            }
        }

        // BackgroundWorker for progress bar update.
        private void BwProgressBarUpdate_DoWork(object sender, DoWorkEventArgs e)
        {
            delegate1 del = new delegate1(UpdateDataGrid);
            download_DataGrid.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, del);
        }

        // BackgroundWorker for show reflaction button.
        private void BwReflactionButton_DoWork(object sender, DoWorkEventArgs e)
        {
            delegate1 del = new delegate1(ShowReclactionButton);
            reflection.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, del);
        }

        private void UpdateDataGrid()
        {
            download_DataGrid.Items.Refresh();
            upload_DataGrid.Items.Refresh();
        }

        private void ShowReclactionButton()
        {
            reflection.Visibility = Visibility.Visible;
        }

        private void Btn_download_Click(object sender, RoutedEventArgs e)
        {
            SearchAndDownload searchAndDownload = new SearchAndDownload(stream, currentUser);
            searchAndDownload.ShowDialog();

            if (searchAndDownload.TransferFileDetails != null)
            {
                TransferFileDetails transferFileDetails = searchAndDownload.TransferFileDetails;
                downloadFiles.Add(new FileStatus(transferFileDetails.FileName, transferFileDetails.FileSize, "Downloading.."));

                UpdateDataGrid();

                DownloadFileHandler downloadFile = new DownloadFileHandler(transferFileDetails, download_DataGrid);
            }
        }

        // Client listening for other client file request.
        public async void StartListeningForFileReq()
        {
            TcpListener clientListener = null;

            try
            {
                clientListener = new TcpListener(IPAddress.Parse(currentUser.Ip), currentUser.UpPort);
                clientListener.Start();

                while (isActiveUser)
                {
                    TcpClient clientSocket = await clientListener.AcceptTcpClientAsync();
                    UploadFileHandler uploadFile = new UploadFileHandler(clientSocket);
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            finally
            {
                clientListener.Stop();
            }
        }

        // This class handle file uploads.
        public class UploadFileHandler
        {
            private TcpClient clientSocket;
            private NetworkStream stream;
            private FileRequest fileRequest;

            public UploadFileHandler(TcpClient clientSocket)
            {
                this.clientSocket = clientSocket;
                this.stream = this.clientSocket.GetStream();

                Thread thread = new Thread(GetFileRequest);
                thread.Start();
            }

            public async void GetFileRequest()
            {
                string jsonFile;
                byte[] jsonBytes;
                byte[] jsonSize = new byte[4]; // int 32.

                // Read size.
                await stream.ReadAsync(jsonSize, 0, 4);

                // Read FileRequest object as json.
                jsonBytes = new byte[BitConverter.ToInt32(jsonSize, 0)];
                await stream.ReadAsync(jsonBytes, 0, jsonBytes.Length);

                // Convert to FileRequest object.
                jsonFile = ASCIIEncoding.ASCII.GetString(jsonBytes);
                fileRequest = JsonConvert.DeserializeObject<FileRequest>(jsonFile);

                StartUploadFile();
            }

            private async void StartUploadFile()
            {
                FileStatus fileStatus = null;
                FileInfo fileInfo = null;
                FileStream fileStream = null;
                string filePath;

                foreach (FileStatus tempFileStatus in uploadFiles)
                {
                    if (tempFileStatus.FileName.Equals(fileRequest.FileName))
                    {
                        fileStatus = tempFileStatus;
                        //break;
                    }
                }

                try
                {
                    filePath = currentUser.UploadPath + "\\" + fileRequest.FileName;
                    fileInfo = new FileInfo(filePath);
                    fileStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read);

                    long totalSize = fileRequest.ToByte - fileRequest.FromByte;
                    long totalSend = 0;
                    long totalLeft = totalSize;

                    int AmountOfByteRead = 0;
                    byte[] buffer = new byte[BUFFER_SIZE];

                    // Looking for where you start sending
                    fileStream.Seek(fileRequest.FromByte, 0);

                    fileStatus.Status = "Uploading..";

                    while (bwProgressBarUpdate.IsBusy) ;
                    bwProgressBarUpdate.RunWorkerAsync();

                    while (totalSend < totalSize && stream.CanWrite && isActiveUser)
                    {
                        // Read data from file to buffer.
                        AmountOfByteRead = fileStream.Read(buffer, 0, totalLeft > BUFFER_SIZE ? buffer.Length : (int)totalLeft);

                        // Write this data on the stream.
                        await stream.WriteAsync(buffer, 0, AmountOfByteRead);

                        // Update totalSend.
                        totalSend += AmountOfByteRead;

                        double percentCompleted = ((double)totalSend / totalSize) * 100;
                        fileStatus.PercentCompleted = Convert.ToInt32(percentCompleted);

                        while (bwProgressBarUpdate.IsBusy) ;
                        bwProgressBarUpdate.RunWorkerAsync();
                    }

                    fileStatus.Status = "Standby";
                    while (bwProgressBarUpdate.IsBusy) ;
                    bwProgressBarUpdate.RunWorkerAsync();
                }

                catch (Exception e)
                {
                    fileStatus.Status = "Error uploading";

                    while (bwProgressBarUpdate.IsBusy) ;
                    bwProgressBarUpdate.RunWorkerAsync();

                    Console.WriteLine("An Exception occurred while uploading a file" + "\n\t Exception:" + e.ToString());
                    MessageBoxResult result = MessageBox.Show("There was a problem with uploading file: " + fileRequest.FileName, "Alert");
                }

                finally
                {
                    stream.Close();

                    if (fileStream != null)
                    {
                        fileStream.Dispose();
                        fileStream.Close();
                    }
                }
            }
        }

        // This class handle file downloads.
        public class DownloadFileHandler
        {
            //private string fileName;
            //private long fileSize;
            //private int numOfPeers;

            private AutoResetEvent[] autoResetEvents;
            private TransferFileDetails transferFileDetails;
            private DataGrid downloadDataGrid;
            private Stopwatch stopWatch;
            private FileStream fileStream;
            private FileInfo fileInfo;
            //private NetworkStream stream;

            private int bytesPerPeer;
            private bool isValidFile = true;

            public DownloadFileHandler(TransferFileDetails transferFileDetails, DataGrid downloadDataGrid)
            {
                this.transferFileDetails = transferFileDetails;
                this.downloadDataGrid = downloadDataGrid;

                //this.fileName = transferFileDetails.FileName;
                //this.fileSize = transferFileDetails.FileSize;
                //this.numOfPeers = transferFileDetails.NumOfPeers;

                bytesPerPeer = (int)this.transferFileDetails.FileSize / this.transferFileDetails.NumOfPeers;

                stopWatch = new Stopwatch();
                stopWatch.Start();

                Thread thread = new Thread(StartDownload);
                thread.Start();
            }

            private void StartDownload()
            {
                string filePath;

                try
                {
                    filePath = currentUser.DownloadPath + "\\" + transferFileDetails.FileName;
                    fileInfo = new FileInfo(filePath);
                    fileStream = new FileStream(fileInfo.FullName, FileMode.Create, FileAccess.Write);
                }

                catch (Exception e)
                {
                    MessageBoxResult result = MessageBox.Show(e.ToString(), " Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                }

                SplitDownloadWork();
            }

            // Split the download (by P2P concept).
            private void SplitDownloadWork()
            {
                // In order to determine when all peers finish their work.
                autoResetEvents = new AutoResetEvent[transferFileDetails.NumOfPeers];

                for (int i = 0; i < transferFileDetails.NumOfPeers; i++)
                    autoResetEvents[i] = new AutoResetEvent(false);

                long BytesCompleted = 0;
                int peerNumber = 1;
                long fromByte;

                // Divides the requests to all peers.
                foreach (Peer peer in transferFileDetails.PeersList)
                {
                    fromByte = BytesCompleted;
                    if (peerNumber == transferFileDetails.NumOfPeers)
                    {
                        DownloadRequestFromPeer(peer, fromByte, transferFileDetails.FileSize, peerNumber++);
                        //Thread thread = new Thread(() => DownloadRequestFromPeer(peer, fromByte, transferFileDetails.FileSize, peerNumber++));
                        //thread.Start();
                    }

                    else
                    {
                        DownloadRequestFromPeer(peer, fromByte, fromByte + bytesPerPeer, peerNumber++);
                        //Thread thread = new Thread(() => DownloadRequestFromPeer(peer, fromByte, fromByte + bytesPerPeer, peerNumber++));
                        //thread.Start();
                    }

                    BytesCompleted += bytesPerPeer;
                }

                // Wait until all peers finish their work.
                WaitHandle.WaitAll(autoResetEvents);

                FinishTransfer();
            }

            private async void DownloadRequestFromPeer(Peer peer, long fromByte, long toByte, int peerNumber)
            {
                FileRequest fileRequest = new FileRequest(transferFileDetails.FileName, fromByte, toByte);
                FileStatus fileStatus = null;

                foreach (FileStatus tempFileStatus in downloadFiles)
                {
                    if (tempFileStatus.FileName.Equals(transferFileDetails.FileName))
                        fileStatus = tempFileStatus;
                }

                // Connects to specific peer.
                TcpClient clientSocket = new TcpClient();
                await clientSocket.ConnectAsync(peer.Ip, peer.Port);
                NetworkStream stream = clientSocket.GetStream();

                SendFileRequest(stream, fileRequest);

                ReadFileRequest(stream, fileRequest, fileStatus, peerNumber);
            }

            private async void SendFileRequest(NetworkStream stream, FileRequest fileRequest)
            {
                string jsonString = JsonConvert.SerializeObject(fileRequest);
                byte[] jsonBytes = ASCIIEncoding.ASCII.GetBytes(jsonString);
                byte[] jsonSize = BitConverter.GetBytes(jsonBytes.Length);

                // Write size.
                await stream.WriteAsync(jsonSize, 0, jsonSize.Length);

                // Write FileRequest object as json.
                await stream.WriteAsync(jsonBytes, 0, jsonBytes.Length);
            }

            private void ReadFileRequest(NetworkStream stream, FileRequest fileRequest, FileStatus fileStatus, int peerNumber)
            {
                // Read file request you sent.
                byte[] buffer = new byte[BUFFER_SIZE];

                int AmountOfByteRead = 0;
                long totalBytesRead = 0;

                long currentPos = fileRequest.FromByte;

                try
                {
                    // Loop until read all relevant bytes.
                    while (currentPos < fileRequest.ToByte && fileStream.CanWrite && isActiveUser)
                    {
                        // Read data.
                        AmountOfByteRead = stream.Read(buffer, 0, buffer.Length);

                        lock (thisLock)
                        {
                            fileStream.Seek(currentPos, 0);
                            fileStream.Write(buffer, 0, (int)AmountOfByteRead);

                            totalBytesRead += AmountOfByteRead;
                            currentPos += AmountOfByteRead;

                            double percentCompleted = ((double)totalBytesRead / transferFileDetails.FileSize) * 100;
                            fileStatus.PercentCompleted = Convert.ToInt32(percentCompleted);

                            // Update UI.
                            while (bwProgressBarUpdate.IsBusy) ;
                            bwProgressBarUpdate.RunWorkerAsync();
                        }
                    }
                }

                catch (Exception e)
                {
                    isValidFile = false;

                    fileStream.Close();
                    Console.WriteLine(e);
                    MessageBoxResult result = MessageBox.Show("There was a problem with downloading file: " + transferFileDetails.FileName, "Alert");
                }

                finally
                {
                    stream.Close();
                    autoResetEvents[peerNumber - 1].Set();
                }
            }

            private void FinishTransfer()
            {
                FileStatus fileStatus = null;

                foreach (FileStatus tempFileStatus in downloadFiles)
                {
                    if (tempFileStatus.FileName.Equals(transferFileDetails.FileName))
                        fileStatus = tempFileStatus;
                }

                if (isValidFile)
                {
                    stopWatch.Stop();
                    TimeSpan timeSpan = stopWatch.Elapsed;

                    // Format and display the elapsed time.
                    string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}",
                        timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);

                    fileStatus.TotaTime = elapsedTime;
                    fileStatus.BitRate = fileStatus.FileSize / timeSpan.Milliseconds;
                    fileStatus.Status = "Download completed";

                    fileStream.Close();

                    // Visible reflection button if needed.
                    if (transferFileDetails.FileName.Trim().Equals("reflection.dll"))
                    {
                        while (bwReflactionButton.IsBusy) ;
                        bwReflactionButton.RunWorkerAsync();
                    }
                }

                else
                {
                    fileStatus.PercentCompleted = 0;
                    fileStatus.Status = "Error downloading";
                    MessageBoxResult result = MessageBox.Show("There was a problem with downloading file: " + transferFileDetails.FileName, "Alert");
                    // MessageBox.Show("There was a problem with " + transferFileDetails.FileName + "transfer");
                }

                while (bwProgressBarUpdate.IsBusy) ;
                bwProgressBarUpdate.RunWorkerAsync();
            }
        }

        private async void AppExit(object sender, CancelEventArgs e)
        {
            isActiveUser = false;

            try
            {
                ClientSearchReq clientSearchRequest = new ClientSearchReq("exit", currentUser.UserName, currentUser.Password);

                string jsonString = JsonConvert.SerializeObject(clientSearchRequest);
                byte[] jsonBytes = ASCIIEncoding.ASCII.GetBytes(jsonString);
                byte[] jsonSize = BitConverter.GetBytes(jsonBytes.Length);

                // Write size.
                await stream.WriteAsync(jsonSize, 0, jsonSize.Length);

                // Write exit request as json.
                await stream.WriteAsync(jsonBytes, 0, jsonBytes.Length);

                stream.Close();
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }

        private void Btn_reflection_Click(object sender, RoutedEventArgs e)
        {
            EnterNumbersMsg enterNumbers = new EnterNumbersMsg(currentUser.DownloadPath);
            enterNumbers.ShowDialog();
        }

        private void Btn_LogOut_Click(object sender, RoutedEventArgs e)
        {
            // Delete configuration file.
            File.Delete("MyConfig.xml");

            MainWindow m = new MainWindow();
            this.Close();
        }
    }
}
