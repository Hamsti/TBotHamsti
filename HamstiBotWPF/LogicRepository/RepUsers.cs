using System;
using System.Windows;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;
using HamstiBotWPF.Core;
using System.Collections;
using System.Collections.Generic;

namespace HamstiBotWPF.LogicRepository
{
    /// <summary>
    /// To work with a list of authorized users
    /// </summary>
    public static class RepUsers
    {
        /// <summary>
        /// Adding all authorized users from a file, including sorting
        /// </summary>
        /// <returns>Successful upload</returns>
        public static bool Upload()
        {
            try
            {
                Update(System.IO.File.Exists("AuthUsers.json") ? JsonConvert.DeserializeObject<ObservableCollection<PatternUser>>(System.IO.File.ReadAllText("AuthUsers.json"))
                     : new ObservableCollection<PatternUser>() { new PatternUser { IdUser = Properties.Settings.Default.AdminId }});

                RefreshAndSort();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("При загрузке данных, произошла ошибка: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Replace items in source collection with does creating new
        /// </summary>
        /// <param name="items">New collection</param>
        public static void Update<T>(T items) where T : IOrderedEnumerable<PatternUser>, IEnumerable<PatternUser>
        {
            var localItems = new ObservableCollection<PatternUser>(items);
            Application.Current.Dispatcher.Invoke(() =>
            {
                GlobalUnit.AuthUsers.Clear();
                foreach (var User in localItems)
                    GlobalUnit.AuthUsers.Add(User);
            });
        }

        /// <summary>
        /// Replace items in source collection with does creating new
        /// </summary>
        /// <param name="items">New collection</param>
        public static void Update(ObservableCollection<PatternUser> items)
        {
            var localItems = new ObservableCollection<PatternUser>(items);
            GlobalUnit.AuthUsers.Clear();
            foreach (var User in localItems)
                GlobalUnit.AuthUsers.Add(User);
        }

        /// <summary>
        /// Saving user data changes to a file
        /// </summary>
        /// <returns>Successful save</returns>
        public static bool Save()
        {
            try
            {
                if (GlobalUnit.AuthUsers == null) return false;
               
                System.IO.File.WriteAllText("AuthUsers.json", JsonConvert.SerializeObject(GlobalUnit.AuthUsers));

                RefreshAndSort();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("При сохранении данных, произошла ошибка: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Sorting and refresh users without of save
        /// </summary>
        public static void RefreshAndSort() => Update(GlobalUnit.AuthUsers.OrderByDescending(o => o.IsUserAdmin).ThenBy(t1 => t1.IsBlocked).ThenBy(t2 => t2.IdUser));

        /// <summary>
        /// Checks if this user is an administrator
        /// </summary>
        /// <param name="userId">Message.From.Id</param>
        /// <returns></returns>
        public static bool IsHaveAccessAdmin(int userId) => GlobalUnit.AuthUsers.Count(_ => userId == Properties.Settings.Default.AdminId) > 0;

        /// <summary>
        /// Checks if this user is in the list of authorized users and not IsBlocked
        /// </summary>
        /// <param name="userId">Message.From.Id</param>
        /// <returns></returns>
        public static bool IsAuthNotIsBlockedUser(int userId) => GlobalUnit.AuthUsers.Count(user => user.IdUser == userId && user.IsBlocked == false) > 0;

        /// <summary>
        /// Checks if this user is in the list of authorized users
        /// </summary>
        /// <param name="userId">Message.From.Id</param>
        /// <returns></returns>
        public static bool IsAuthUser(int userId) => GlobalUnit.AuthUsers.Count(user => user.IdUser == userId) > 0;
    }
}
