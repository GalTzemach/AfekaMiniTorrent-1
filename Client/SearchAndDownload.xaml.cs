using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MiniTorrent
{
    /// <summary>
    /// Interaction logic for SearchAndDownload.xaml
    /// </summary>
    public partial class SearchAndDownload : Window
    {//FileNotFoundLabel
        private string emptyFieldsError = "Search field have to be filled";
        private string fileNotFound = "File not found";
        private List<TransferFileDetails> tfd;
        private TransferFileDetails tfdForTransfer;
        private Users currentUser;
        private NetworkStream ns;

        public SearchAndDownload(NetworkStream ns, Users user)
        {
            InitializeComponent();
            this.ns = ns;
            this.currentUser = user;
        }

        public TransferFileDetails getTfdForTransfer()
        { return tfdForTransfer; }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string fileName = fileNameTextBox.Text.Trim();
            if(fileName.Trim().Equals(""))
            {
                FileNotFoundLabel.Content = emptyFieldsError;
                FileNotFoundLabel.Visibility = Visibility.Visible;
            }
            else
            {
                FileNotFoundLabel.Visibility = Visibility.Hidden;
                sendSearchFileRequest(fileName);
            }
            
        }

        public async void sendSearchFileRequest(string fName)
        {
            ClientSearchReq csr = new ClientSearchReq(fName, currentUser.UserName, currentUser.Password);

            string jasonStriing = JsonConvert.SerializeObject(csr);

            byte[] jsonFile = ASCIIEncoding.ASCII.GetBytes(jasonStriing);
            byte[] jsonFileLength = BitConverter.GetBytes(jsonFile.Length);

            await ns.WriteAsync(jsonFileLength, 0, jsonFileLength.Length);
            await ns.WriteAsync(jsonFile, 0, jsonFile.Length);

            byte[] answer = new byte[1];
            await ns.ReadAsync(answer, 0, 1);

            if (answer[0] == 0)
            {
                FileNotFoundLabel.Content = fileNotFound;
                FileNotFoundLabel.Visibility = Visibility.Visible;
                return;
            }
            else
            {
                FileNotFoundLabel.Visibility = Visibility.Hidden;
                getJsonFile();
            }
        }

        public async void getJsonFile()
        {
            string jsonFile;
            byte[] jsonBytes;
            byte[] jsonLengthBytes = new byte[4]; //int32

            List<TransferFileDetails> tfList = new List<TransferFileDetails>();

            await ns.ReadAsync(jsonLengthBytes, 0, 4); // int32
            jsonBytes = new byte[BitConverter.ToInt32(jsonLengthBytes, 0)];
            await ns.ReadAsync(jsonBytes, 0, jsonBytes.Length);

            jsonFile = ASCIIEncoding.ASCII.GetString(jsonBytes);

            tfd = new List<TransferFileDetails>();
            tfd = JsonConvert.DeserializeObject<List<TransferFileDetails>>(jsonFile);
            dataGrid.ItemsSource = tfd;
            
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DownloadButton.Visibility = Visibility.Visible;
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            TransferFileDetails temp = (TransferFileDetails) dataGrid.SelectedItem;
            //UserControlPanel.newDownload(temp);
            tfdForTransfer = temp;
            this.Close();
        }
    }
}
