using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using File = System.IO.File;

namespace TBotHamsti.Models.Users
{
    /// <summary>
    /// To work with a list of authorized users
    /// </summary>
    public static class UsersFunc
    {
        private static ObservableCollection<User> authUsers;

        /// <summary>
        /// List of all authorized users
        /// </summary>
        public static ObservableCollection<User> AuthUsers => authUsers ??= new ObservableCollection<User>();

        static UsersFunc()
        {
            Upload();
        }

        public static async Task SendMessageAsync(this StatusUser status, string message)
        {
            IEnumerable<User> foundUsers = AuthUsers.Where(user => user.Status == status);
            if (foundUsers.Any())
            {
                throw new ArgumentNullException(nameof(foundUsers), $"No users with {status} status were found. Message not sent");
            }

            Exception exUsers = null;
            foreach (var user in foundUsers)
            {
                try
                {
                    await user.SendMessageAsync(message);
                }
                catch (Exception ex)
                {
                    (exUsers ??= ex).AppendExceptionMessage("Message didn't send to user [" + user.Id + "]");
                }
            }

            if (exUsers != null)
            {
                throw exUsers;
            }
        }

        public static Task<Message> SendMessageAsync(this User user, string message) => user.SendMessageAsync(message, null);

        public static async Task<Message> SendMessageAsync(this User user, string message, IReplyMarkup replyMarkup)
        {
            return await App.Api.SendTextMessageAsync(
                user?.Id ?? throw new ArgumentNullException(nameof(user), "User is null, message did't send: \"" + message + "\""),
                message ?? throw new ArgumentNullException(nameof(message), "Message is null, it doesn't send to user " + user.Id_Username),
                replyMarkup: replyMarkup);
        }

        /// <summary>
        /// Adding all authorized users from a file, including sorting
        /// </summary>
        /// <returns>Successful upload</returns>
        public static void Upload()
        {
            Update(File.Exists(Properties.Settings.Default.JsonFileName) 
                ? JsonConvert.DeserializeObject<ObservableCollection<User>>(File.ReadAllText(Properties.Settings.Default.JsonFileName)) 
                : new ObservableCollection<User>());
            Refresh();
        }

        /// <summary>
        /// Saving user data changes to a file
        /// </summary>
        /// <returns>Successful save</returns>
        public static void SaveRefresh()
        {
            File.WriteAllText(Properties.Settings.Default.JsonFileName, JsonConvert.SerializeObject(AuthUsers ?? throw new ArgumentNullException(nameof(AuthUsers))));
            Refresh();
        }

        /// <summary>
        /// Sorting and refresh users without of save
        /// </summary>
        public static void Refresh() => Update(AuthUsers.OrderByDescending(o => o.Status).ThenBy(t1 => t1.IsBlocked).ThenBy(t2 => t2.Id));

        /// <summary>
        /// Checks if this user is in the list of authorized users
        /// </summary>
        public static User GetUser(int id) => AuthUsers.Where(user => user.Id == id).DefaultIfEmpty(null).FirstOrDefault()
            ?? throw new ArgumentNullException(nameof(id), $"User [{id}] isn't found");

        /// <summary>
        /// Checks if this user is in the list of authorized users
        /// </summary>
        public static User GetUser(string localNickame)
        {
            localNickame = localNickame?.ToLower() ?? throw new ArgumentNullException(nameof(localNickame));
            return AuthUsers.Where(user => (user?.Username?.ToLower() ?? string.Empty) == localNickame).DefaultIfEmpty(null)?.FirstOrDefault()
                ?? throw new ArgumentNullException(nameof(localNickame), $"User [{localNickame}] isn't found");
        }

        /// <summary>
        /// Replace items in source collection with does creating new
        /// </summary>
        /// <param name="items">New collection</param>
        private static void Update<T>(T items) where T : IOrderedEnumerable<User>, IEnumerable<User>
        {
            var tempCollection = new ObservableCollection<User>(items);
            App.UiContext.Send(x => {
                AuthUsers.Clear();
                for (int i = 0; i < tempCollection.Count; i++)
                {
                    AuthUsers.Add(tempCollection[i]);
                }
            }, null);
        }

        /// <summary>
        /// Replace items in source collection with does creating new
        /// </summary>
        /// <param name="items">New collection</param>
        private static void Update(ObservableCollection<User> items)
        {
            var tempCollection = new ObservableCollection<User>(items);
            AuthUsers.Clear();
            for (int i = 0; i < tempCollection.Count; i++)
            {
                AuthUsers.Add(tempCollection[i]);
            }
        }
    }
}
