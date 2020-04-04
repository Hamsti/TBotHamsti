using System;
using System.Windows;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;
using HamstiBotWPF.Core;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace HamstiBotWPF.LogicRepository
{
    /// <summary>
    /// To work with a list of authorized users
    /// </summary>
    public static class RepUsers
    {
        public enum StatusUser
        {
            NotDefined,
            User,
            Moderator,
            Admin
        }

        public static async Task SendMessage(string message, StatusUser status = StatusUser.Admin) => await SendMessage(GlobalUnit.AuthUsers.Where(user => user.Status == status), message);

        public static async Task SendMessage(int idUser, string message)
        {
            try
            {
                await GlobalUnit.Api.SendTextMessageAsync(idUser, message);
            }
            catch (Exception ex)
            {
                _ = Task.Run(() => MessageBox.Show(ex.Message));
            };
        }

        private static async Task SendMessage(IEnumerable<PatternUser> findedUsers, string message)
        {
            foreach (var user in findedUsers)
                await SendMessage(user.IdUser, message);
        }

        /// <summary>
        /// Adding all authorized users from a file, including sorting
        /// </summary>
        /// <returns>Successful upload</returns>
        public static bool Upload()
        {
            try
            {
                Update(System.IO.File.Exists("AuthUsers.json") ? JsonConvert.DeserializeObject<ObservableCollection<PatternUser>>(System.IO.File.ReadAllText("AuthUsers.json"))
                     : new ObservableCollection<PatternUser>() { new PatternUser { IdUser = Properties.Settings.Default.RecoverIdAdmin } });

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
        public static void RefreshAndSort() => Update(GlobalUnit.AuthUsers.OrderByDescending(o => o.Status).ThenBy(t1 => t1.IsBlocked).ThenBy(t2 => t2.IdUser));

        /// <summary>
        /// Checks if this user is in the list of authorized users and not IsBlocked
        /// </summary>
        /// <param name="userId">Message.From.Id</param>
        public static bool IsAuthNotIsBlockedUser(int userId) => GlobalUnit.AuthUsers.Count(user => user.IdUser == userId && user.IsBlocked == false) > 0;

        /// <summary>
        /// Checks if this user is in the list of authorized users
        /// </summary>
        /// <param name="userId">Message.From.Id</param>
        public static bool IsAuthUser(int userId) => GlobalUnit.AuthUsers.Count(user => user.IdUser == userId) > 0;

        /// <summary>
        /// Find user status
        /// </summary>
        /// <param name="userId">Message.From.Id</param>
        public static StatusUser GetStatusUser(int userId) => GlobalUnit.AuthUsers.Where(w => w.IdUser == userId).DefaultIfEmpty(new PatternUser()).Select(s => s.Status).FirstOrDefault();
    }
}
