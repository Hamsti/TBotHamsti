using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TBotHamsti.Core;
using TBotHamsti.Messages;
using TBotHamsti.ViewModels;

namespace TBotHamsti.LogicRepository
{
    /// <summary>
    /// To work with a list of authorized users
    /// </summary>
    public static class RepUsers
    {
        public enum StatusUser
        {
            None,
            User,
            Moder,
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

        public static async Task SendMessageAsync(this StatusUser status, string message)
        {
            IEnumerable<PatternUser> findedUsers = AuthUsers.Where(user => user.Status == status);

            foreach (var user in findedUsers)
            {
                await user.SendMessageAsync(message);
            }
        }

        public static async Task<Telegram.Bot.Types.Message> SendMessageAsync(this PatternUser user, string message)
        {
            if (user is null)
            {
                throw new ArgumentNullException(nameof(user), "User is null, message did't send: \"" + message + "\"");
            }

            if (message is null)
            {
                throw new ArgumentNullException(nameof(message), "Message is null, it doesn't send to user " + user.IdUser_Nickname);
            }

            return await App.Api.SendTextMessageAsync(user.Id, message);
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
        /// Checks if this user is in the list of authorized users
        /// </summary>
        /// <param name="id">Message.From.Id</param>
        public static PatternUser GetUser(int id) => AuthUsers.Where(user => user.Id == id).FirstOrDefault();

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
