using DAL;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Server
{
    //This class shows all active users and their files.

    /// <summary>
    /// Interaction logic for ServerInformation.xaml
    /// </summary>
    public partial class ServerInformation : Window
    {
        private static BackgroundWorker bwUpdateUserList;
        private static BackgroundWorker bwUpdateFileList;
        private delegate void selectDelegate(User u);
        private delegate void updateDelegate();
        private static object thisLock = new object();

        public List<User> ActiveUsers { get; set; }
        public Dictionary<FileDetails, List<User>> ServerFileList { get; set; }

        public ServerInformation()
        {
            InitializeComponent();

            ActiveUsers = new List<User>();
            ServerFileList = new Dictionary<FileDetails, List<User>>();
            dataGrid_users.ItemsSource = ActiveUsers;
            dataGrid_files.ItemsSource = ServerFileList;

            bwUpdateUserList = new BackgroundWorker();
            bwUpdateFileList = new BackgroundWorker();
            bwUpdateUserList.DoWork += BwUpdate_DoWork;
            bwUpdateFileList.DoWork += BwSelect_DoWork;
        }

        private void UpdateFileList(User selectedUser)
        {
            dataGrid_files.ItemsSource = selectedUser.FileList;
        }

        private void UpdateUserList()
        {
            dataGrid_users.Items.Refresh();
        }

        private void BwSelect_DoWork(object sender, DoWorkEventArgs e)
        {
            selectDelegate selectDel = new selectDelegate(UpdateFileList);
            dataGrid_files.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, selectDel, (User)e.Argument);
        }

        private void BwUpdate_DoWork(object sender, DoWorkEventArgs e)
        {
            updateDelegate updateDel = new updateDelegate(UpdateUserList);
            dataGrid_users.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, updateDel);
        }

        // Execute when user Log in.
        public void AddUserFiles(User user, OperationsDB DB)
        {
            lock (thisLock)
            {
                if (!ActiveUsers.Contains(user))
                    ActiveUsers.Add(user);
            }

            foreach (FileDetails file in user.FileList)
            {
                if (ServerFileList.ContainsKey(file)) 
                {
                    // This file already exist in list.
                    // Add this user as adition peer to download.
                    lock (thisLock)
                    {
                        if (!ServerFileList[file].Contains(user))
                        {
                            ServerFileList[file].Add(user);
                            DB.AddPeerToFile(file.FileName, file.FileSize);
                        }
                    }
                }

                else
                {
                    // This file new in list.
                    lock (thisLock)
                    {
                        ServerFileList.Add(file, new List<User>());
                        ServerFileList[file].Add(user);
                        DB.AddFile(file.FileName, file.FileSize);
                    }
                }
            }

            while (bwUpdateUserList.IsBusy) ;
            bwUpdateUserList.RunWorkerAsync();
        }

        // Execute when user Log out.
        public void DeleteUserFiles(User user) 
        {
            foreach (FileDetails file in user.FileList)
            {
                if (ServerFileList.ContainsKey(file)) 
                {
                    // File exsits in list.
                    if (ServerFileList[file].Contains(user))
                    {
                        // This user in owner of this file. 
                        ServerFileList[file].Remove(user);

                        // Delete this file if there is no longer more owners.
                        if (ServerFileList[file].Count == 0)
                            ServerFileList.Remove(file);
                    }
                }
            }

            // Delete user from list if is active.
            if (ActiveUsers.Contains(user))
                ActiveUsers.Remove(user);

            while (bwUpdateUserList.IsBusy) ;
            bwUpdateUserList.RunWorkerAsync();
        }

        public bool IsActiveUser(string userName, string password)
        {
            foreach (User user in ActiveUsers)
            {
                if (user.UserName.Equals(userName) && user.Password.Equals(password))
                    return true;
            }
            return false;
        }

        private void DataGrid_users_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
                bwUpdateFileList.RunWorkerAsync(e.AddedItems[0]);

            else
                dataGrid_files.ItemsSource = null;
        }
    }
}
