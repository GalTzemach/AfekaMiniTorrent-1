using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public void logOffAllUsers()
        {
            var users = from user in DB.Users
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

        public void clearFileTable()
        {
            var files = from file in DB.Files
                        select file;

            if (files.Count() != 0)
            {
                foreach (File f in files)
                {
                    DB.Files.DeleteOnSubmit(f);
                }
                lock (thisLock)
                {
                    DB.SubmitChanges();
                }
            }
        }

        public void AddNewUser(string userName, string password)
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
                    DataSet1TableAdapters.FileTableAdapter fta = new DataSet1TableAdapters.FileTableAdapter();
                    DataSet1 ds = new DataSet1();
                    fta.FillBy(ds.File, fileName);
                    return ds;
                }

                catch
                {
                    return null;
                }
            }
        }

        public int getUser(string userName, string password)
        {
            var users = from user in DB.Users
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
                            return 2;//already connected
                        else
                        {
                            user.IsActive = true;
                            lock (thisLock)
                            {
                                DB.SubmitChanges();
                            }
                            return 1; //all good
                        }
                    }
                    else
                        return 3; //not enabled
                }
            }

            return 0; //not created
        }

        public void logOffUser(string userName)
        {
            var users = from user in DB.Users
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

        public void removeFile(string fileName, long fileSize)
        {
            var files = from file in DB.Files
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

        public void updatePear(string fileName, long fileSize)
        {
            var files = from file in DB.Files
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

        public void addNewFile(string fileName, long fileSize)
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
