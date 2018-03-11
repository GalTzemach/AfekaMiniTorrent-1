using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Interaction logic for IpMsgBox.xaml
    /// </summary>
    public partial class IpMsgBox : Window
    {
        public string chosenIp;
        public IpMsgBox(List<string> ip)
        {
            InitializeComponent();
            
            listView.ItemsSource = ip;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            chosenIp = (string)listView.SelectedItem;
            this.Close();
            return;
        }
    }
}
