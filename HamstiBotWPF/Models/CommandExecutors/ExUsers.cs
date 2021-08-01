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
        public static int IdStrToInt(string idUserString) => int.TryParse(idUserString, out int id)
            ? id
            : throw new ArgumentException("Id isn't in the correct format", nameof(idUserString));

        public static Task SaveChanges(User user)
        {
            UsersFunc.SaveRefresh();
            return user.SendMessageAsync("Save changes successfully");
        }

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

        public static User AuthNewUser(ICommand model, Message message)
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

        public static void DeauthUser(User user)
        {
            bool isRemoved = false;
            App.UiContext.Send(x => isRemoved = UsersFunc.AuthUsers.Remove(user), null);
            if (!isRemoved)
            {
                throw new ArgumentException($"User [{user.Id_Username}] doesn't exist", nameof(user));
            }
        }

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

        public static Task StartCommandUser(Message message)
        {
            return AddUser(message.From.Id, message.From.Username).SendMessageAsync(
                $"You have been successfully added to the bot's user list.\n" +
                $"Ask the bot administrator {App.Api.GetMeAsync().Result} add you to the list of allowed users.\n\n" +
                $"You can write to the bot administrator using the command \"{CollectionCommands.SendMessageToAdminCommand.ExampleCommand}\"");
        }

        public static Task DeauthUser(ICommand model, User user, Message message)
        {
            User foundUser = int.TryParse(model.GetArg(0), out int idUser) && model.CountArgsCommand == 1
                ? UsersFunc.GetUser(idUser)
                : UsersFunc.GetUser(model.GetOriginalArgs(message));
            DeauthUser(foundUser);
            return user.SendMessageAsync($"User [{foundUser.Id_Username}] successfully removed from the user list");
        }

        public static Task ChangeLocalName(ICommand model, User user, Message message)
        {
            User foundUser = UsersFunc.GetUser(IdStrToInt(model.GetArg(0)));
            string beforeChangingNickname = foundUser.Username;
            foundUser.Username = model.GetOriginalArgs(message, 1);
            return user.SendMessageAsync($"New nickname of user [{foundUser.Id_Username}], changed from [{beforeChangingNickname}].");
        }

        public static Task LockUser(ICommand model, User user, Message message)
        {
            User foundUser = int.TryParse(model.GetArg(0), out int idUser) && model.CountArgsCommand == 1
                ? UsersFunc.GetUser(idUser)
                : UsersFunc.GetUser(model.GetOriginalArgs(message));

            foundUser.IsBlocked = !foundUser.IsBlocked;
            return user.SendMessageAsync($"User [{foundUser.Id_Username}] successfully {(foundUser.IsBlocked ? "locked" : "unlocked")}.");
        }

        public static Task SendListOfUsers(User userSource)
        {
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