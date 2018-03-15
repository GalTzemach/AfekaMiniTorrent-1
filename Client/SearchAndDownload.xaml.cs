using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace MiniTorrent
{
    /// <summary>
    /// Interaction logic for SearchAndDownload.xaml
    /// </summary>
    public partial class SearchAndDownload : Window
    {
        // FileNotFoundLabel
        private string emptyFields = "Search field is empty";
        private string fileNotFound = "File not found";

        public TransferFileDetails TransferFileDetails { get; set; }

        private List<TransferFileDetails> transferFileList;
        private User currentUser;
        private NetworkStream stream;

        public SearchAndDownload(NetworkStream stream, User currentUser)
        {
            InitializeComponent();

            this.currentUser = currentUser;
            this.stream = stream;

            transferFileList = new List<TransferFileDetails>();
        }

        private void Btn_search_Click(object sender, RoutedEventArgs e)
        {
            string fileName = fileNameTextBox.Text.Trim();

            if (string.IsNullOrEmpty(fileName.Trim()))
            {
                SearchStatusLabel.Content = emptyFields;
                SearchStatusLabel.Visibility = Visibility.Visible;
            }
            else
            {
                SearchStatusLabel.Visibility = Visibility.Hidden;
                SendSearchFileRequest(fileName);
            }
        }

        public async void SendSearchFileRequest(string fileName)
        {
            ClientSearchReq clientSearchRequest = new ClientSearchReq(fileName, currentUser.UserName, currentUser.Password);

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
            transferFileList = JsonConvert.DeserializeObject<List<TransferFileDetails>>(jsonString);
            dataGrid.ItemsSource = transferFileList;
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DownloadButton.Visibility = Visibility.Visible;
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            TransferFileDetails = (TransferFileDetails)dataGrid.SelectedItem;
            this.Close();
        }

        private void Btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
