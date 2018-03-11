using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using DAL;

namespace Server
{
    //class that shows who is online and which files he shares

    /// <summary>
    /// Interaction logic for ServerInformation.xaml
    /// </summary>
    public partial class ServerInformation : Window
    {
        private Dictionary<FileDetails, List<Users>> serverFiles;
        private List<Users> onLineUsers;
        private static object _lock = new object();
        private static BackgroundWorker bwUpdate = new BackgroundWorker();
        private static BackgroundWorker bwSelect = new BackgroundWorker();
        private delegate void mySelectDelegate(Users u);
        private delegate void myUpdateDelegate();

        public List<Users> OnLineUsers { get { return onLineUsers; } set { onLineUsers = value; } }
        public Dictionary<FileDetails, List<Users>> ServerFiles { get { return serverFiles; } set { serverFiles = value; } }

        public ServerInformation()
        {
            InitializeComponent();
            serverFiles = new Dictionary<FileDetails, List<Users>>();
            onLineUsers = new List<Users>();
            dataGrid.ItemsSource = serverFiles;
            dataGrid1.ItemsSource = onLineUsers;
            bwUpdate.DoWork += bwUpdate_DoWork;
            bwSelect.DoWork += bwSelect_DoWork;
        }

        private void updateSelect(Users u)
        {
            dataGrid.ItemsSource = u.FileList;
        }

        private void updateTable()
        {
            dataGrid1.Items.Refresh();
        }

        private void bwSelect_DoWork(object sender, DoWorkEventArgs e)
        {
            mySelectDelegate deli = new mySelectDelegate(updateSelect);
            dataGrid.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, deli, (Users)e.Argument);
        }

        private void bwUpdate_DoWork(object sender, DoWorkEventArgs e)
        {
            myUpdateDelegate deli = new myUpdateDelegate(updateTable);
            dataGrid1.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, deli);
        }

        public void addUserFiles(Users user, OperationsDB dba) //user log in
        {
            lock (_lock)
            {
                if (!onLineUsers.Contains(user))
                    onLineUsers.Add(user);
            }

            foreach (FileDetails f in user.FileList)
            {
                if (serverFiles.ContainsKey(f)) //file exsits
                {
                    //file in list -> add new user as source to download
                    lock (_lock)
                    {
                        if (!serverFiles[f].Contains(user))
                        {
                            serverFiles[f].Add(user);
                            dba.updatePear(f.FileName, f.FileSize);
                        }
                    }
                }

                else
                {
                    //new file to share
                    lock (_lock)
                    {
                        serverFiles.Add(f, new List<Users>());
                        serverFiles[f].Add(user);
                        dba.addNewFile(f.FileName, f.FileSize);
                    }
                }
            }

            while (bwUpdate.IsBusy) ;
            bwUpdate.RunWorkerAsync();
        }

        public void removeUserFiles(Users user) //user log out
        {
            foreach (FileDetails f in user.FileList)
            {
                if (serverFiles.ContainsKey(f)) //file exsits
                {
                    if (serverFiles[f].Contains(user))
                    {
                        serverFiles[f].Remove(user);

                        if (serverFiles[f].Count == 0)
                            serverFiles.Remove(f);
                    }
                }
            }

            if (onLineUsers.Contains(user))
                onLineUsers.Remove(user);

            while (bwUpdate.IsBusy) ;
            bwUpdate.RunWorkerAsync();
        }

        public bool isUserActive(string userName, string password)
        {
            foreach (Users u in OnLineUsers)
            {
                if (u.UserName.Equals(userName) && u.Password.Equals(password))
                    return true;
            }
            return false;
        }

        private void dataGrid1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
                bwSelect.RunWorkerAsync(e.AddedItems[0]);
            else
                dataGrid.ItemsSource = null;
        }
    }
}
