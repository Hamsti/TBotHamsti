using System;
using System.Text;
using System.Threading.Tasks;
using TBotHamsti.Models.Commands;
using TBotHamsti.Models.Users;
using Telegram.Bot.Types;
using User = TBotHamsti.Models.Users.User;

namespace TBotHamsti.Models.CommandExecutors
{
    public static class ExUsers
    {
        /// <summary>
        /// Convert <paramref name="idUserString"/> to correct format
        /// </summary>
        /// <param name="idUserString">User id in string</param>
        /// <value>123456789</value>
        /// <returns>Number in format for <see cref="User.Id"/></returns>
        /// <exception cref="ArgumentException">If <paramref name="idUserString"/> has wrong format</exception>
        public static int IdStrToInt(string idUserString) => int.TryParse(idUserString, out int id)
            ? id
            : throw new ArgumentException("Id isn't in the correct format", nameof(idUserString));

        /// <summary>
        /// Saving changes in the <see cref="UsersFunc.AuthUsers"/>, made by the <see cref="ICommand"/>
        /// </summary>
        /// <inheritdoc cref="DeauthUser(ICommand, User, Message)"/>
        public static Task SaveChanges(User user)
        {
            UsersFunc.SaveRefresh();
            return user.SendMessageAsync("Save changes successfully");
        }

        /// <summary>
        /// Canceling changes in the <see cref="UsersFunc.AuthUsers"/>, made by the <see cref="ICommand"/>
        /// </summary>
        /// <inheritdoc cref="DeauthUser(ICommand, User, Message)"/>
        /// <exception cref="Exception">If upload users failed</exception>
        public static Task CancelChanges(User user)
        {
            Exception ex = null;
            App.UiContext.Send(x =>
            {
                try
                {
                    UsersFunc.Upload();
                }
                catch (Exception internalError)
                {
                    ex = internalError;
                }
            }, null);
            return user.SendMessageAsync(ex is null ? "Changes canceled successfully" : throw ex);
        }

        /// <summary>
        /// Adding new user by id from <paramref name="message"/> if <paramref name="model"/>.Args is empty
        /// </summary>
        /// <returns>Added <see cref="User"/> without nickname</returns>
        /// <inheritdoc cref="DeauthUser(ICommand, User, Message)"/>
        /// <exception cref="ArgumentNullException">If <paramref name="model"/> or <paramref name="model.Args"/> is null</exception>
        private static User AuthNewUser(ICommand model, Message message)
        {
            if (model is null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (model.Args is null)
            {
                throw new ArgumentNullException(nameof(model.Args));
            }

            int id = model.CountArgsCommand == 0 ? message.From.Id : IdStrToInt(model.Args[0]);

            return AddUser(id, null);
        }

        /// <summary>
        /// Adding new user with <paramref name="id"/> and <paramref name="username"/>
        /// </summary>
        /// <param name="id">User <see cref="User.Id"/></param>
        /// <param name="username">User <see cref="User.Username"/></param>
        /// <returns>Added <see cref="User"/></returns>
        /// <exception cref="ArgumentException">If <see cref="User"/> exists already</exception>
        public static User AddUser(int id, string username)
        {
            try
            {
                throw new ArgumentException($"User [{UsersFunc.GetUser(id).Id_Username}] exists already!", nameof(id));
            }
            catch (ArgumentNullException)
            {
                User newUser = new User() { Id = id, Username = username };
                App.UiContext.Send(x => UsersFunc.AuthUsers.Add(newUser), null);
                return newUser;
            }
        }

        /// <summary>
        /// Remove <paramref name="user"/> from the <see cref="UsersFunc.AuthUsers"/>
        /// </summary>
        /// <param name="user">The <paramref name="user"/> for deauthentication</param>
        /// <exception cref="ArgumentException">If the <paramref name="user"/> doesn't exist</exception>
        public static void DeauthUser(User user)
        {
            bool isRemoved = false;
            App.UiContext.Send(x => isRemoved = UsersFunc.AuthUsers.Remove(user), null);
            if (!isRemoved)
            {
                throw new ArgumentException($"User [{user.Id_Username}] doesn't exist", nameof(user));
            }
        }

        /// <summary>
        /// Adding new <see cref="User"/> by the <see cref="ICommand"/>
        /// </summary>
        /// <inheritdoc cref="SendListOfUsers(User)" />
        /// <inheritdoc cref="DeauthUser(ICommand, User, Message)" />
        public static Task AuthNewUser(ICommand model, User userSource, Message message)
        {
            string username = null;
            if (userSource is null)
            {
                throw new ArgumentNullException(nameof(userSource));
            }

            if (model.CountArgsCommand > 1)
            {
                username = model.GetOriginalArgs(message, 1);
            }

            User newUser = AuthNewUser(model, message);
            newUser.Username = username;
            return userSource.SendMessageAsync($"User [{newUser.Id_Username}] successfully added to the user list.");
        }

        /// <summary>
        /// Adding a new <see cref="User"/> with the "start" command with a special response <see cref="Message"/>
        /// </summary>
        /// <inheritdoc cref="DeauthUser(ICommand, User, Message)"/>
        public static Task StartCommandUser(Message message)
        {
            return AddUser(message.From.Id, message.From.Username).SendMessageAsync(
                $"You have been successfully added to the bot's user list.\n" +
                $"Ask the bot administrator {App.Api.GetMeAsync().Result} add you to the list of allowed users.\n\n" +
                $"You can write to the bot administrator using the command \"{CollectionCommands.SendMessageToAdminCommand.ExampleCommand}\"");
        }

        /// <summary>
        /// Remove <paramref name="user"/> from the <see cref="UsersFunc.AuthUsers"/> by the <see cref="ICommand"/>
        /// </summary>
        /// <param name="model">Converted <see cref="ICommand"/> from <see cref="Commands.BotCommand.ParseMessage(Message)"/></param>
        /// <param name="user">The <paramref name="user"/> who called the execution of the function</param>
        /// <param name="message">The <paramref name="message"/> which called the execution of the function</param>
        /// <returns>Task for sending a response <see cref="Message"/> to the <paramref name="user"/></returns>
        public static Task DeauthUser(ICommand model, User user, Message message)
        {
            User foundUser = int.TryParse(model.GetArg(0), out int idUser) && model.CountArgsCommand == 1
                ? UsersFunc.GetUser(idUser)
                : UsersFunc.GetUser(model.GetOriginalArgs(message));
            DeauthUser(foundUser);
            return user.SendMessageAsync($"User [{foundUser.Id_Username}] successfully removed from the user list");
        }

        /// <summary>
        /// Change <see cref="User.Username"/> in the <see cref="UsersFunc.AuthUsers"/> by the <see cref="ICommand"/>
        /// </summary>
        /// <inheritdoc cref="DeauthUser(ICommand, User, Message)"/>
        public static Task ChangeLocalName(ICommand model, User user, Message message)
        {
            User foundUser = UsersFunc.GetUser(IdStrToInt(model.GetArg(0)));
            string beforeChangingNickname = foundUser.Username;
            foundUser.Username = model.GetOriginalArgs(message, 1);
            return user.SendMessageAsync($"New nickname of user [{foundUser.Id_Username}], changed from [{beforeChangingNickname}].");
        }

        /// <summary>
        /// Locking <see cref="User"/> in the <see cref="UsersFunc.AuthUsers"/> by the <see cref="ICommand"/>
        /// </summary>
        /// <inheritdoc cref="DeauthUser(ICommand, User, Message)"/>
        public static Task LockUser(ICommand model, User user, Message message)
        {
            User foundUser = int.TryParse(model.GetArg(0), out int idUser) && model.CountArgsCommand == 1
                ? UsersFunc.GetUser(idUser)
                : UsersFunc.GetUser(model.GetOriginalArgs(message));

            foundUser.IsBlocked = !foundUser.IsBlocked;
            return user.SendMessageAsync($"User [{foundUser.Id_Username}] successfully {(foundUser.IsBlocked ? "locked" : "unlocked")}.");
        }

        /// <summary>
        /// Sending the <see cref="UsersFunc.AuthUsers"/> to the <paramref name="userSource"/>
        /// </summary>
        /// <param name="userSource">The <paramref name="userSource"/> who called the execution of the function</param>
        /// <returns>Task for sending a response <see cref="Message"/> to the <paramref name="userSource"/></returns>
        /// <exception cref="ArgumentNullException">If <paramref name="userSource"/> is null</exception>
        public static Task SendListOfUsers(User userSource)
        {
            if (userSource is null)
            {
                throw new ArgumentNullException(nameof(userSource));
            }

            int indexUser = 0;
            StringBuilder messageText = new StringBuilder($"A list of users {App.Api.GetMeAsync().Result}:\n\n");
            UsersFunc.Refresh();
            foreach (User user in UsersFunc.AuthUsers)
            {
                messageText.Append($"{++indexUser}) {user.Id_Username} | {nameof(user.IsBlocked)}: {user.IsBlocked} | {nameof(user.Status)}: {user.Status}\n");
            }

            return userSource.SendMessageAsync(messageText.ToString());
        }
    }
}