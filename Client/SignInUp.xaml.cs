using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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


namespace Client
{
    /// <summary>
    /// Interaction logic for SignInn.xaml
    /// </summary>
    public partial class SignInn : Window
    {
        public SignInn()
        {
            InitializeComponent();
        }

        private void reset_Button_Click(object sender, RoutedEventArgs e)
        {
            user_name_TextBox.Text = "";
            password_TextBox.Clear();
        }

        private void sign_in_Button_Click(object sender, RoutedEventArgs e)
        {
            string userName = user_name_TextBox.Text;
            string password = password_TextBox.Password;
        }

        private void upload_Button_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.ShowDialog();
                upload_folder_TextBox.Text = dialog.SelectedPath;
            }
        }

        private void download_Button_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.ShowDialog();
                download_folder_TextBox.Text = dialog.SelectedPath;
            }
        }
    }
}
