using System;
using System.Linq;

namespace DAL
{
    public class OperationsDB
    {
        private DataClasses1DataContext DB = new DataClasses1DataContext();
        private static object thisLock = new object();

        public int[] GetUsersCount()
        {
            int[] res = new int[2];
            // res[0] = all users.
            // res[1] = active users.

            var users = from user
                        in DB.Users
                        select user;

            if (users.Count() != 0)
            {
                foreach (User user in users)
                {
                    res[0]++;
                    if (user.IsActive)
                        res[1]++;
                }
            }

            return res;
        }

        public int GetFilesCount()
        {
            return (from file
                    in DB.Files
                    select file).Count();
        }

        public void LogOffAllUsers()
        {
            var users = from user
                        in DB.Users
                        select user;

            if (users.Count() != 0)
            {
                foreach (User u in users)
                {
                    u.IsActive = false;
                }

                lock (thisLock)
                {
                    DB.SubmitChanges();
                }
            }
        }

        public void DeleteAllFiles()
        {
            var files = from file
                        in DB.Files
                        select file;

            DB.Files.DeleteAllOnSubmit(files);
            lock (thisLock)
            {
                DB.SubmitChanges();
            }

            //if (files.Count() != 0)
            //{
            //    foreach (File f in files)
            //    {
            //        DB.Files.DeleteOnSubmit(f);
            //    }
            //    lock (thisLock)
            //    {
            //        DB.SubmitChanges();
            //    }
            //}
        }

        public void AddUser(string userName, string password)
        {
            User user = new User
            {
                UserName = userName,
                Password = password,
                IsActive = false,
                IsDisable = false
            };

            DB.Users.InsertOnSubmit(user);

            lock (thisLock)
            {
                DB.SubmitChanges();
            }
        }

        public bool UserAlreadyExist(string userName)
        {
            var userExist = (from user
                             in DB.Users
                             where user.UserName == userName
                             select user).Count();

            return userExist != 0;
        }

        public DataSet1 SearchFileName(string fileName)
        {
            if (String.IsNullOrEmpty(fileName))
            {
                return null;
            }

            else
            {
                try
                {
                    DataSet1TableAdapters.FileTableAdapter adapter = new DataSet1TableAdapters.FileTableAdapter();
                    DataSet1 dataSet = new DataSet1();
                    adapter.FillBy(dataSet.File, fileName);
                    return dataSet;
                }

                catch
                {
                    return null;
                }
            }
        }

        public int GetUserStatus(string userName, string password)
        {
            // 0 = User not exist.
            // 1 = Connecting the user.
            // 2 = User alredy connected.
            // 3 = User Disable.

            var users = from user
                        in DB.Users
                        where user.UserName == userName
                        where user.Password == password
                        select user;

            if (users.Count() != 0)
            {
                foreach (User user in users)
                {
                    if (!user.IsDisable)
                    {
                        if (user.IsActive)
                            return 2; // User alredy connected.
                        else
                        {
                            user.IsActive = true;
                            lock (thisLock)
                            {
                                DB.SubmitChanges();
                            }
                            return 1; // Connecting the user.
                        }
                    }
                    else
                        return 3; // User Disable.
                }
            }

            return 0; // User not exist.
        }

        public void LogOffUser(string userName)
        {
            var users = from user
                        in DB.Users
                        where user.UserName == userName
                        select user;

            if (users.Count() != 0)
            {
                foreach (User user in users)
                {
                    user.IsActive = false;
                }
                lock (thisLock)
                {
                    DB.SubmitChanges();
                }
            }
        }

        public void DeletePeerFromFile(string fileName, long fileSize)
        {
            var files = from file
                        in DB.Files
                        where file.FileName == fileName
                        where file.FileSize == fileSize
                        select file;

            if (files.Count() != 0)
            {
                foreach (File file in files)
                {
                    if (file.NumOfPeers == 1)
                        DB.Files.DeleteOnSubmit(file);
                    else
                        file.NumOfPeers--;
                }

                lock (thisLock)
                {
                    DB.SubmitChanges();
                }
            }
        }

        public void AddPeerToFile(string fileName, long fileSize)
        {
            var files = from file
                        in DB.Files
                        where file.FileName == fileName
                        where file.FileSize == fileSize
                        select file;

            if (files.Count() != 0)
            {
                foreach (File file in files)
                {
                    file.NumOfPeers++;
                }
                lock (thisLock)
                {
                    DB.SubmitChanges();
                }
            }
        }

        public void AddFile(string fileName, long fileSize)
        {
            File f = new File
            {
                FileName = fileName,
                FileSize = fileSize,
                NumOfPeers = 1
            };

            DB.Files.InsertOnSubmit(f);

            lock (thisLock)
            {
                DB.SubmitChanges();
            }
        }
    }
}
