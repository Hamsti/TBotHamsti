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
    /// Authorized <see cref="User"/>s of the bot and a set of functions for interacting with them
    /// </summary>
    public static class UsersFunc
    {
        private static ObservableCollection<User> authUsers;

        /// <summary>
        /// List of all authorized <see cref="User"/> of the bot
        /// </summary>
        /// <value>
        /// Returns a collection of users, otherwise creates a new one and return it
        /// </value>
        public static ObservableCollection<User> AuthUsers => authUsers ??= new ObservableCollection<User>();

        /// <summary>
        /// Fisrt uploading of the <see cref="AuthUsers"/> 
        /// </summary>
        static UsersFunc()
        {
            Upload();
        }

        /// <summary>
        /// Send <paramref name="message"/> to the group <see cref="User"/> with <paramref name="status"/>
        /// </summary>
        /// <param name="status">Group users status</param>
        /// <param name="message">Message text</param>
        /// <returns>A <see cref="Task"/> to wait for all messages to be sent</returns>
        /// <exception cref="ArgumentNullException">If users with this <paramref name="status"/> don't exist</exception>
        /// <exception cref="Exception">If a <paramref name="message"/> hasn't been sent to some user</exception>
        public static async Task SendMessageAsync(this StatusUser status, string message)
        {
            IEnumerable<User> foundUsers = AuthUsers.Where(user => user.Status == status);
            if (!foundUsers.Any())
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

        /// <summary>
        /// Send <paramref name="message"/> to the <paramref name="user"/>
        /// </summary>
        /// <param name="user">To whom the <paramref name="message"/> will be sent</param>
        /// <param name="message">Message text</param>
        /// <returns>A <see cref="Task{TResult}"/> waiting for a <paramref name="message"/> to be sent</returns>
        public static Task<Message> SendMessageAsync(this User user, string message) => user.SendMessageAsync(message, null);

        /// <param name="replyMarkup"><inheritdoc cref="Telegram.Bot.Types.ReplyMarkups.IReplyMarkup" path="/summary"/></param>
        /// <inheritdoc cref="SendMessageAsync(User, string)"/>
        public static async Task<Message> SendMessageAsync(this User user, string message, IReplyMarkup replyMarkup) => await App.Api.SendTextMessageAsync(
                user?.Id ?? throw new ArgumentNullException(nameof(user), "User is null, message did't send: \"" + message + "\""),
                message ?? throw new ArgumentNullException(nameof(message), "Message is null, it doesn't send to user " + user.Id_Username),
                replyMarkup: replyMarkup);

        /// <summary>
        /// Uploading, sorting and updating a collection <see cref="AuthUsers"/> from a file, if it does not exist, creating a new one
        /// </summary>
        public static void Upload()
        {
            SortUpdate(File.Exists(Properties.Settings.Default.JsonFileName)
                ? JsonConvert.DeserializeObject<ObservableCollection<User>>(File.ReadAllText(Properties.Settings.Default.JsonFileName))
                : new ObservableCollection<User>());
        }

        /// <summary>
        /// Saving the collection <see cref="AuthUsers"/> to a file, then sorting and updating it
        /// </summary>
        public static void SaveToFile()
        {
            File.WriteAllText(Properties.Settings.Default.JsonFileName, JsonConvert.SerializeObject(AuthUsers ?? throw new ArgumentNullException(nameof(AuthUsers))));
            SortUpdate();
        }

        /// <summary>
        /// Sorting and updating objects of the collection <see cref="AuthUsers"/>
        /// </summary>
        public static void SortUpdate(ObservableCollection<User> sourseCollection = null) =>
            Update((sourseCollection ?? AuthUsers).OrderByDescending(o => o.IsSetBookmark).ThenByDescending(t => t.Status).ThenBy(t => t.IsBlocked).ThenBy(t => t.Id));

        /// <summary>
        /// Search for a <see cref="User"/> by criterion in the collection <see cref="AuthUsers"/>
        /// </summary>
        /// <param name="id">Search by <see cref="User.Id"/></param>
        /// <returns>The first <see cref="User"/> found, otherwise throws an exception <see cref="ArgumentNullException"/></returns>
        /// <exception cref="ArgumentNullException">If an <see cref="User"/> ins't found</exception>
        public static User GetUser(int id) => AuthUsers.Where(user => user.Id == id).DefaultIfEmpty(null).FirstOrDefault()
            ?? throw new ArgumentNullException(nameof(id), $"User [{id}] isn't found");

        /// <param name="username">Search by <see cref="User.Username"/></param>
        /// <exception cref="ArgumentNullException">If <paramref name="username"/> is null</exception>
        /// <inheritdoc cref="GetUser(int)"/>
        public static User GetUser(string username)
        {
            username = username?.ToLower() ?? throw new ArgumentNullException(nameof(username));
            return AuthUsers.Where(user => (user?.Username?.ToLower() ?? string.Empty) == username).DefaultIfEmpty(null)?.FirstOrDefault()
                ?? throw new ArgumentNullException(nameof(username), $"User [{username}] isn't found");
        }

        /// <summary>
        /// Replacing all items of the collection <see cref="AuthUsers"/> without creating a link to a new one
        /// </summary>
        /// <param name="enumerator"><paramref name="enumerator"/> of the modified collection</param>
        private static void Update(IEnumerable<User> enumerator)
        {
            App.UiContext.Send(x =>
            {
                var tempArray = Enumerable.ToArray(enumerator);
                AuthUsers.Clear();
                foreach (var user in tempArray)
                {
                    AuthUsers.Add(user);
                }
            }, null);
        }
    }
}
