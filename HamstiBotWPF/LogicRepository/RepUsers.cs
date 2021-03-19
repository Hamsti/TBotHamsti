using System;
using System.Windows;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Newtonsoft.Json;
using TBotHamsti.Core;

namespace TBotHamsti.LogicRepository
{
    /// <summary>
    /// To work with a list of authorized users
    /// </summary>
    public static class RepUsers
    {
        private static ObservableCollection<PatternUser> authUsers;

        /// <summary>
        /// List of all authorized users
        /// </summary>
        public static ObservableCollection<PatternUser> AuthUsers
        {
            get
            {
                if (authUsers is null)
                    authUsers = new ObservableCollection<PatternUser>();
                return authUsers;
            }
        }

        public enum StatusUser
        {
            NotDefined,
            User,
            Moderator,
            Admin
        }

        public static async Task SendMessage(string message, StatusUser status = StatusUser.Admin) => await SendMessage(AuthUsers.Where(user => user.Status == status), message);

        public static async Task SendMessage(int idUser, string message)
        {
            try
            {
                await App.Api.SendTextMessageAsync(idUser, message);
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
                AuthUsers.Clear();
                foreach (var User in localItems)
                    AuthUsers.Add(User);
            });
        }

        /// <summary>
        /// Replace items in source collection with does creating new
        /// </summary>
        /// <param name="items">New collection</param>
        public static void Update(ObservableCollection<PatternUser> items)
        {
            var localItems = new ObservableCollection<PatternUser>(items);
            AuthUsers.Clear();
            foreach (var User in localItems)
                AuthUsers.Add(User);
        }

        /// <summary>
        /// Saving user data changes to a file
        /// </summary>
        /// <returns>Successful save</returns>
        public static bool Save()
        {
            try
            {
                if (AuthUsers == null) return false;

                System.IO.File.WriteAllText("AuthUsers.json", JsonConvert.SerializeObject(AuthUsers));

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
        public static void RefreshAndSort() => Update(AuthUsers.OrderByDescending(o => o.Status).ThenBy(t1 => t1.IsBlocked).ThenBy(t2 => t2.IdUser));

        /// <summary>
        /// Checks if this user is in the list of authorized users and not IsBlocked
        /// </summary>
        /// <param name="idUser">Message.From.Id</param>
        public static bool IsAuthNotIsBlockedUser(int idUser) => AuthUsers.Count(user => user.IdUser == idUser && user.IsBlocked == false) > 0;

        /// <summary>
        /// Checks if this user is in the list of authorized users
        /// </summary>
        /// <param name="idUser">Message.From.Id</param>
        public static bool IsAuthUser(int idUser) => AuthUsers.Count(user => user.IdUser == idUser) > 0;

        /// <summary>
        /// Find user status
        /// </summary>
        /// <param name="idUser">Message.From.Id</param>
        public static StatusUser GetStatusUser(int idUser) => AuthUsers.Where(w => w.IdUser == idUser).DefaultIfEmpty(new PatternUser()).Select(s => s.Status).FirstOrDefault();
    }
}
