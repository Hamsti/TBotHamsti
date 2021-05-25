using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TBotHamsti.Core;

namespace TBotHamsti.LogicRepository
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

        static RepUsers()
        {
            Upload();
        }

        public static async Task SendMessage(string message, StatusUser status = StatusUser.Admin)
        {
            IEnumerable<PatternUser> findedUsers = AuthUsers.Where(user => user.Status == status);

            foreach (var user in findedUsers)
                await SendMessage(user.Id, message);
        }

        public static PatternUser GetUser(int id) => AuthUsers.Where(user => user.Id == id).DefaultIfEmpty(new PatternUser()).FirstOrDefault();

        public static async Task SendMessage(int Id, string message)
        {
            try
            {
                await App.Api.SendTextMessageAsync(Id, message);
            }
            catch (Exception)
            {
                //_ = Task.Run(() => MessageBox.Show(ex.Message));
            };
        }

        /// <summary>
        /// Adding all authorized users from a file, including sorting
        /// </summary>
        /// <returns>Successful upload</returns>
        public static bool Upload()
        {
            try
            {
                Update(File.Exists(Properties.Settings.Default.JsonFileName) ?
                            JsonConvert.DeserializeObject<ObservableCollection<PatternUser>>(File.ReadAllText(Properties.Settings.Default.JsonFileName)) :
                            new ObservableCollection<PatternUser>() { new PatternUser { Id = Properties.Settings.Default.RecoverIdAdmin } }
                );

                Refresh();
                return true;
            }
            catch (Exception)
            {
                //MessageBox.Show("При загрузке данных, произошла ошибка: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Saving user data changes to a file
        /// </summary>
        /// <returns>Successful save</returns>
        public static bool SaveRefresh()
        {
            try
            {
                if (AuthUsers == null) return false;

                File.WriteAllText(Properties.Settings.Default.JsonFileName, JsonConvert.SerializeObject(AuthUsers));

                Refresh();
                return true;
            }
            catch (Exception)
            {
                //MessageBox.Show("При сохранении данных, произошла ошибка: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Sorting and refresh users without of save
        /// </summary>
        public static void Refresh() => Update(AuthUsers.OrderByDescending(o => o.Status).ThenBy(t1 => t1.IsBlocked).ThenBy(t2 => t2.Id));

        /// <summary>
        /// Checks if this user is in the list of authorized users and not IsBlocked
        /// </summary>
        /// <param name="Id">Message.From.Id</param>
        public static bool IsAuthNotIsBlockedUser(int Id) => AuthUsers.Count(user => user.Id == Id && user.IsBlocked == false) > 0;

        /// <summary>
        /// Checks if this user is in the list of authorized users
        /// </summary>
        /// <param name="Id">Message.From.Id</param>
        public static bool IsAuthUser(int Id) => AuthUsers.Count(user => user.Id == Id) > 0;

        /// <summary>
        /// Find user status
        /// </summary>
        /// <param name="Id">Message.From.Id</param>
        public static StatusUser GetStatusUser(int Id) => AuthUsers.Where(w => w.Id == Id).DefaultIfEmpty(new PatternUser()).Select(s => s.Status).FirstOrDefault();

        /// <summary>
        /// Replace items in source collection with does creating new
        /// </summary>
        /// <param name="items">New collection</param>
        private static void Update<T>(T items) where T : IOrderedEnumerable<PatternUser>, IEnumerable<PatternUser>
        {
            var localItems = new ObservableCollection<PatternUser>(items);
            App.Current.Dispatcher.Invoke(() =>
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
        private static void Update(ObservableCollection<PatternUser> items)
        {
            var localItems = new ObservableCollection<PatternUser>(items);
            AuthUsers.Clear();
            foreach (var User in localItems)
                AuthUsers.Add(User);
        }
    }
}
