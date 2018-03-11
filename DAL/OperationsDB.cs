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
            throw new NotImplementedException();
        }

        public void clearFileTable()
        {
            throw new NotImplementedException();
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
    }
}
