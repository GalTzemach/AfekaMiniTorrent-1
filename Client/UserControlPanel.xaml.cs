using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MiniTorrent
{
    /// <summary>
    /// Interaction logic for UserControlPanel.xaml
    /// </summary>
    public partial class UserControlPanel : Window
    {
        private NetworkStream ns;
        private static List<FileStatus> uploadFiles;
        private static List<FileStatus> downloadFiles;
        private static User currentUser;
        private delegate void myDelegate();
        private static BackgroundWorker bw;
        private static BackgroundWorker bwReflactionButton;
        private static object _lock = new object();
        private static bool activeFlag;

        public UserControlPanel(NetworkStream ns, List<FileStatus> uploadFiles, User currentUser)
        {
            InitializeComponent();
            downloadFiles = new List<FileStatus>();
            bw = new BackgroundWorker();
            bwReflactionButton = new BackgroundWorker();
            activeFlag = true;
            this.ns = ns;
            UserControlPanel.uploadFiles = uploadFiles;
            UserControlPanel.currentUser = currentUser;
            uploadDataGrid.ItemsSource = uploadFiles;
            downloadDataGrid.ItemsSource = downloadFiles;
            bw.DoWork += bw_DoWork;
            bwReflactionButton.DoWork += bwReflactionButton_DoWork;
            checkForReflactionFile();
            clientStartListening();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            SearchAndDownload sad = new SearchAndDownload(ns, currentUser);
            sad.ShowDialog();
            if(sad.getTfdForTransfer() != null)
            {
                TransferFileDetails tfd = sad.getTfdForTransfer();
                downloadFiles.Add(new FileStatus(tfd.FileName, tfd.FileSize, "Downloading"));

                updateDataGrid();
                
                DownloadHandler dh = new DownloadHandler();
                dh.download(tfd, downloadDataGrid);
            }            
        }

        private void updateDataGrid()
        {
            downloadDataGrid.Items.Refresh();
            uploadDataGrid.Items.Refresh();
        }

        private void showReclactionButton()
        {
            reflection.Visibility = Visibility.Visible;
        }

        //BackgroundWorker for progress bar update
        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            myDelegate deli = new myDelegate(updateDataGrid);
            downloadDataGrid.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, deli);
        }

        //BackgroundWorker for reflaction button visibality
        private void bwReflactionButton_DoWork(object sender, DoWorkEventArgs e)
        {
            myDelegate deli = new myDelegate(showReclactionButton);
            reflection.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, deli);
        }

        //check if there is a reflaction dll
        public void checkForReflactionFile()
        {
            if (File.Exists(currentUser.DownloadPath + "\\reflection.dll"))
            {
                while (bwReflactionButton.IsBusy) ;
                bwReflactionButton.RunWorkerAsync();
            }
        }

        //client waiting for other clients request
        public async void clientStartListening()
        {
            TcpListener cleintListener = null;

            try
            {
                cleintListener = new TcpListener(IPAddress.Parse(currentUser.Ip), currentUser.UpPort);
                cleintListener.Start();

                while (activeFlag)
                {
                    TcpClient clientSocket = await cleintListener.AcceptTcpClientAsync();
                    UploadHandler client = new UploadHandler();
                    client.startUpload(clientSocket);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                cleintListener.Stop();
            }
        }

        //class that deal with uploads
        public class UploadHandler
        {
            private TcpClient clientSocket;

            public void startUpload(TcpClient inClientSocket)
            {
                this.clientSocket = inClientSocket;
                Thread t = new Thread(newClient);
                t.Start();
            }
            private void newClient()
            {
                NetworkStream ns = clientSocket.GetStream();
                getFileToDownload(ns);                
            }

            public async void getFileToDownload(NetworkStream ns)
            {
                string jsonFile;
                byte[] jsonBytes;
                byte[] jsonLengthBytes = new byte[4]; //int32

                await ns.ReadAsync(jsonLengthBytes, 0, 4); // int32
                jsonBytes = new byte[BitConverter.ToInt32(jsonLengthBytes, 0)];
                await ns.ReadAsync(jsonBytes, 0, jsonBytes.Length);

                jsonFile = ASCIIEncoding.ASCII.GetString(jsonBytes);
                
                ClientUploadDownload cup  = JsonConvert.DeserializeObject<ClientUploadDownload>(jsonFile);
                startSendFile(cup, ns);
            }

            private async void startSendFile(ClientUploadDownload cup, NetworkStream ns)
            {
                FileStream fileStream = null;
                FileStatus fas = null;

                foreach (FileStatus tempFas in uploadFiles)
                {
                    if (tempFas.FileName.Equals(cup.FileName))
                        fas = tempFas;
                }

                try
                { 
                    string path = currentUser.UploadPath + "\\" + cup.FileName;
                    FileInfo file = new FileInfo(path);
                    fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);
                    
                    long total = cup.ToByte - cup.FromByte;
                    long ToatlSent = 0;
                    long totalLeft = total;
                    int len = 0;
                    byte[] buffer = new byte[50000];

                    fileStream.Seek(cup.FromByte, 0);
                    while (ToatlSent < total && ns.CanWrite && activeFlag)
                    {
                        //Read from the File (len contains the number of bytes read)
                        if (totalLeft > 50000)
                            len = fileStream.Read(buffer, 0, buffer.Length);
                        else
                            len = fileStream.Read(buffer, 0, (int)totalLeft);
                        //Write the Bytes on the Socket
                        await ns.WriteAsync(buffer, 0, len);
                        //Increase the bytes Read counter
                        ToatlSent += len;                        

                            double pctread = ((double)ToatlSent / total) * 100;
                            fas.PercentCompleted = Convert.ToInt32(pctread);
                            fas.Status = "Uploading";

                            while (bw.IsBusy) ;
                            bw.RunWorkerAsync();                                    
 
                    }                   
                }
                catch (Exception e)
                {
                    Console.WriteLine("A Exception occured in transfer" + e.ToString());
                    fas.Status = "Error";
                    while (bw.IsBusy) ;
                    bw.RunWorkerAsync();
                    MessageBoxResult result = MessageBox.Show("There was a problem with " + cup.FileName + "transfer");
                }
                finally
                {

                    fas.Status = "Completed";
                    while (bw.IsBusy) ;
                    bw.RunWorkerAsync();

                    ns.Close();
                    if (fileStream != null)
                    {
                        fileStream.Dispose();
                        fileStream.Close();
                    }                        
                }
            }
        }

        //class that deals with downloads from clients
        public class DownloadHandler
        {
            private TransferFileDetails tfd;
            private string fileName;
            private long fileSize;
            private long downloaded;
            private int numOfPears;
            private int bytesPerPear;
            private FileInfo fi;
            private FileStream fileStream;
            private string path;
            private Stopwatch stopWatch;
            private DataGrid downloadDataGrid;
            private AutoResetEvent[] autoEvents;
            private bool vaildFlag = true;

            public void download(TransferFileDetails tfd, DataGrid downloadDataGrid)
            {
                this.tfd = tfd;
                this.fileName = tfd.FileName;
                fileSize = tfd.FileSize;
                numOfPears = tfd.PearsCount;
                this.downloadDataGrid = downloadDataGrid;
                bytesPerPear = (int)fileSize / numOfPears;
                stopWatch = new Stopwatch();
                stopWatch.Start();
                Thread t = new Thread(beginDownload);
                t.Start();
            }

            public void beginDownload()
            { 
                try
                {
                    path = currentUser.DownloadPath + "\\" + fileName;
                    fi = new FileInfo(path);
                    fileStream = new FileStream(fi.FullName, FileMode.Create, FileAccess.Write);
                }
                catch(Exception e)
                {
                    MessageBoxResult result = MessageBox.Show(e.ToString(), "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                }                
                
                splitWork();
                
            }

            //split the download (P2P)
            public void splitWork()
            {
                autoEvents = new AutoResetEvent[numOfPears];
                for (int i = 0; i < numOfPears; i++)
                    autoEvents[i] = new AutoResetEvent(false);
                
                long bytesSpred = 0;
                int count = 1;
                foreach (Pear p in tfd.Pears)
                {
                    if(count == numOfPears)
                    {
                        long temp1 = bytesSpred;
                        int tempCount = count++;
                        Thread thread = new Thread(() => connectToPear(p, temp1, fileSize, tempCount));
                        thread.Start();                        
                    }
                    else
                    {
                        long temp2 = bytesSpred;
                        int tempCount = count++;
                        Thread thread = new Thread(() => connectToPear(p, temp2, temp2 + bytesPerPear, tempCount));
                        thread.Start();
                    }
                    
                    bytesSpred += bytesPerPear;
                }

                WaitHandle.WaitAll(autoEvents);
                finishTransfer();
            }

            private async void connectToPear(Pear p, long fromByte, long toByte, int num)
            {
                long startPos = fromByte;
                FileStatus fas = null;

                foreach (FileStatus tempFas in downloadFiles)
                {
                    if (tempFas.FileName.Equals(fileName))
                        fas = tempFas;
                }

                TcpClient client = new TcpClient();
                await client.ConnectAsync(p.Ip, p.Port);
                NetworkStream ns = client.GetStream();

                sendFileNameAndPart(ns, fileName, fromByte, toByte);

                int i = 1;
                try
                {
                    //loop till the Full bytes have been read
                    while (startPos< toByte && fileStream.CanWrite && activeFlag)
                    {
                        byte[] buffer = new byte[50000];
                        i = ns.Read(buffer, 0, buffer.Length);
                        lock (_lock)
                        {
                            downloaded += i;
                            fileStream.Seek(startPos, 0);
                            fileStream.Write(buffer, 0, (int)i);
                        }
                        
                        lock (_lock)
                        {                                    
                            double pctread = ((double)downloaded / tfd.FileSize) * 100;
                            fas.PercentCompleted = Convert.ToInt32(pctread);                                    
                            while (bw.IsBusy) ;
                            bw.RunWorkerAsync();
                        } 
                        startPos += i;
                    }                   
                }
                catch (Exception e)
                {
                    vaildFlag = false;
                    fileStream.Close();
                    Console.WriteLine(e);
                    //MessageBoxResult result = MessageBox.Show("There was a problem with " + fileName + "transfer", "Alert");
                }
                finally
                {
                    ns.Close();
                    autoEvents[num - 1].Set();
                }
            }

            private async void sendFileNameAndPart(NetworkStream ns, string fileName, long fromByte, long toByte)
            {
                ClientUploadDownload cup = new ClientUploadDownload(fileName, fromByte, toByte);

                string jasonStriing = JsonConvert.SerializeObject(cup);

                byte[] jsonFile = ASCIIEncoding.ASCII.GetBytes(jasonStriing);
                byte[] jsonFileLength = BitConverter.GetBytes(jsonFile.Length);

                await ns.WriteAsync(jsonFileLength, 0, jsonFileLength.Length);
                await ns.WriteAsync(jsonFile, 0, jsonFile.Length);
            }

            private void finishTransfer()
            {
                FileStatus fas = null;

                foreach (FileStatus tempFas in downloadFiles)
                {
                    if (tempFas.FileName.Equals(fileName))
                        fas = tempFas;
                }

                if (!vaildFlag)
                {
                    fas.PercentCompleted = 0;
                    fas.Status = "Error";
                    MessageBox.Show("There was a problem with " + fileName + "transfer");                    
                }

                else
                {
                    stopWatch.Stop();
                    TimeSpan ts = stopWatch.Elapsed;
                    // Format and display the TimeSpan value.
                    string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}",
                        ts.Hours, ts.Minutes, ts.Seconds);
                    fas.BitRate = fas.FileSize / ts.Milliseconds;
                    fas.Status = "Completed";
                    fas.TotaTime = elapsedTime;
                    fileStream.Close();

                    if (fileName.Trim().Equals("reflection.dll"))
                    {
                        while (bwReflactionButton.IsBusy) ;
                        bwReflactionButton.RunWorkerAsync();
                    }
                }
                while (bw.IsBusy) ;
                bw.RunWorkerAsync();
            }
        }

        private async void appExit(object sender, CancelEventArgs e)
        {
            activeFlag = false;
            try
            {
                ClientSearchReq csr = new ClientSearchReq("exit", currentUser.UserName, currentUser.Password);

                string jasonStriing = JsonConvert.SerializeObject(csr);

                byte[] jsonFile = ASCIIEncoding.ASCII.GetBytes(jasonStriing);
                byte[] jsonFileLength = BitConverter.GetBytes(jsonFile.Length);

                await ns.WriteAsync(jsonFileLength, 0, jsonFileLength.Length);
                await ns.WriteAsync(jsonFile, 0, jsonFile.Length);

                ns.Close();
            }
            catch(Exception ed)
            {
                Console.WriteLine(ed.ToString());
            }
            
        }

        private void reflection_Click(object sender, RoutedEventArgs e)
        {
            enterNumbersMsg msg = new enterNumbersMsg(currentUser.DownloadPath);
            msg.ShowDialog();
        }

        //logout button
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            File.Delete("MyConfig.xml");
            MainWindow m = new MainWindow();
            this.Close();
        }
    }
}
