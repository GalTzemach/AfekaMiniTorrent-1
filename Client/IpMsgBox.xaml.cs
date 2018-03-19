using System.Collections.Generic;
using System.Windows;

namespace MiniTorrent
{
    /// <summary>
    /// Interaction logic for IpMsgBox.xaml
    /// </summary>
    public partial class ChooseIPAddress : Window
    {
        public string selectedIP;

        public ChooseIPAddress(List<string> IPList)
        {
            InitializeComponent();

            listView_IP.ItemsSource = IPList;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            selectedIP = (string)listView_IP.SelectedItem;
            this.Close();
            return;
        }
    }
}
