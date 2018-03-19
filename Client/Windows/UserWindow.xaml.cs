﻿using Newtonsoft.Json;
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
using System.Windows.Input;

namespace MiniTorrent
{
    /// <summary>
    /// Interaction logic for UserControlPanel.xaml
    /// </summary>
    public partial class UserWindow : Window
    {
        private const int BUFFER_SIZE = 50000;
        public const string REFLECTION_DLL_FILE_NAME = "MyReflection.dll";

        // FileNotFoundLabel
        private string emptyFields = "Search field is empty";
        private string fileNotFound = "File not found";

        private NetworkStream stream;
        private static User currentUser;

        private static List<FileStatus> uploadFiles;
        private static List<FileStatus> downloadFiles;

        private delegate void delegate1();
        private static BackgroundWorker bwProgressBarUpdate;
        private static BackgroundWorker bwReflactionButton;

        private static object thisLock;
        private static bool isActiveUser;

        private List<TransferFileDetails> transferFileList;

        private TransferFileDetails TransferFileDetails { get; set; }


        public UserWindow(NetworkStream stream, List<FileStatus> uploadFiles, User currentUser)
        {
            InitializeComponent();

            thisLock = new object();
            isActiveUser = true;
            this.stream = stream;

            UserWindow.downloadFiles = new List<FileStatus>();
            UserWindow.uploadFiles = new List<FileStatus>();
            UserWindow.uploadFiles = uploadFiles;
            UserWindow.currentUser = currentUser;
            upload_DataGrid.ItemsSource = uploadFiles;
            download_DataGrid.ItemsSource = downloadFiles;

            bwProgressBarUpdate = new BackgroundWorker();
            bwReflactionButton = new BackgroundWorker();
            bwProgressBarUpdate.DoWork += BwProgressBarUpdate_DoWork;
            bwReflactionButton.DoWork += BwReflactionButton_DoWork;

            Title = currentUser.UserName;

            CheckForReflactionFile();
            StartListeningForFileReq();
        }

        // Check if exist reflaction.dll file.
        public void CheckForReflactionFile()
        {
            if (File.Exists(currentUser.DownloadPath + "\\" + REFLECTION_DLL_FILE_NAME))
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

            private long totalCompleted;
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
                            totalCompleted += AmountOfByteRead;
                            fileStream.Seek(currentPos, 0);
                            fileStream.Write(buffer, 0, (int)AmountOfByteRead);
                        }

                        totalBytesRead += AmountOfByteRead;
                        currentPos += AmountOfByteRead;

                        lock (thisLock)
                        {
                            double percentCompleted = ((double)totalCompleted / transferFileDetails.FileSize) * 100;
                            fileStatus.PercentCompleted = Convert.ToInt32(percentCompleted);
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
                    fileStatus.BitRate = (fileStatus.FileSize / 1024 / 1024 * 8) / timeSpan.TotalSeconds;
                    fileStatus.Status = "Download completed";

                    fileStream.Close();

                    // Visible reflection button if needed.
                    if (transferFileDetails.FileName.Trim().Equals(REFLECTION_DLL_FILE_NAME))
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
                SearchRequest clientSearchRequest = new SearchRequest("exit", currentUser.UserName, currentUser.Password);

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
            //EnterNumbersMsg enterNumbers = new EnterNumbersMsg(currentUser.DownloadPath);
            //enterNumbers.ShowDialog();

            string reflectionMessage = HandleReflection.GetAuthors(currentUser.DownloadPath);
            MessageBox.Show(reflectionMessage);
        }

        private void Btn_LogOut_Click(object sender, RoutedEventArgs e)
        {
            // Delete configuration file.
            File.Delete("MyConfig.xml");

            MainWindow m = new MainWindow();
            this.Close();
        }

        private void Btn_search_Click(object sender, RoutedEventArgs e)
        {
            string fileName = fileNameTextBox.Text.Trim();

            DownloadButton.Visibility = Visibility.Hidden;

            if (string.IsNullOrEmpty(fileName.Trim()))
            {
                SearchStatusLabel.Content = emptyFields;
                SearchStatusLabel.Visibility = Visibility.Visible;

                ///G
                transferFileList = new List<TransferFileDetails>();
                dataGrid.ItemsSource = transferFileList;
                DownloadButton.Visibility = Visibility.Hidden;
            }
            else
            {
                SearchStatusLabel.Visibility = Visibility.Hidden;
                SendSearchFileRequest(fileName);
                DownloadButton.Visibility = Visibility.Hidden;
            }
        }

        public async void SendSearchFileRequest(string fileName)
        {
            SearchRequest clientSearchRequest = new SearchRequest(fileName, currentUser.UserName, currentUser.Password);

            string jsonString = JsonConvert.SerializeObject(clientSearchRequest);
            byte[] jsonBytes = ASCIIEncoding.ASCII.GetBytes(jsonString);
            byte[] jsonSize = BitConverter.GetBytes(jsonBytes.Length);

            // Write size.
            await stream.WriteAsync(jsonSize, 0, jsonSize.Length);

            // Write ClientSearchReq as json to server.
            await stream.WriteAsync(jsonBytes, 0, jsonBytes.Length);

            byte[] answer = new byte[1];

            // Read answer from server.
            await stream.ReadAsync(answer, 0, 1);

            if (answer[0] == 0)
            {
                // Error.
                SearchStatusLabel.Content = fileNotFound;
                SearchStatusLabel.Visibility = Visibility.Visible;

                ///G
                DownloadButton.Visibility = Visibility.Hidden;
                transferFileList = new List<TransferFileDetails>();
                dataGrid.ItemsSource = transferFileList;
                DownloadButton.Visibility = Visibility.Hidden;
                return;
            }

            else
            {
                // Success.
                SearchStatusLabel.Visibility = Visibility.Hidden;
                GetResponseFromServer();
            }
        }

        public async void GetResponseFromServer()
        {
            string jsonString;
            byte[] jsonBytes;
            byte[] jsonSize = new byte[4]; // int32

            // Read size.
            await stream.ReadAsync(jsonSize, 0, 4);
            jsonBytes = new byte[BitConverter.ToInt32(jsonSize, 0)];

            // Read List<TransferFileDetails> as json.
            await stream.ReadAsync(jsonBytes, 0, jsonBytes.Length);
            jsonString = ASCIIEncoding.ASCII.GetString(jsonBytes);

            // Convert json to List<TransferFileDetails>.
            transferFileList = new List<TransferFileDetails>();
            transferFileList = JsonConvert.DeserializeObject<List<TransferFileDetails>>(jsonString);
            dataGrid.ItemsSource = transferFileList;
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            TransferFileDetails = (TransferFileDetails)dataGrid.SelectedItem;

            if (TransferFileDetails != null)
            {
                //TransferFileDetails transferFileDetails = TransferFileDetails;///G
                downloadFiles.Add(new FileStatus(TransferFileDetails.FileName, TransferFileDetails.FileSize, "Downloading.."));

                UpdateDataGrid();

                DownloadFileHandler downloadFile = new DownloadFileHandler(TransferFileDetails, download_DataGrid);

                dataGrid.SelectedItem = null;
                dataGrid_LostFocus(null, null);
            }
        }

        private void fileNameTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                Btn_search_Click(null, null);
            }
        }

        private void dataGrid_LostFocus(object sender, RoutedEventArgs e)
        {
            if (dataGrid.SelectedItems.Count != 1)
                DownloadButton.Visibility = Visibility.Hidden;
        }

        private void dataGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (dataGrid.SelectedItems.Count == 1)
                DownloadButton.Visibility = Visibility.Visible;
        }
    }
}
