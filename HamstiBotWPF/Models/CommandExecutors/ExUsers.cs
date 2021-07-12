using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TBotHamsti.Models.Commands;
using TBotHamsti.Models.Users;
using Telegram.Bot.Types;
using User = TBotHamsti.Models.Users.User;

namespace TBotHamsti.Models.CommandExecutors
{
    public static class ExUsers
    {
        private static ObservableCollection<User> ListUsers => UsersFunc.AuthUsers;
        public static int StrToInt(string idUserString) => int.TryParse(idUserString, out int idNewUser) ? idNewUser : -1;

        private static string ListOfUsersParseString()
        {
            int indexUser = 0;
            System.Text.StringBuilder messageText = new System.Text.StringBuilder($"Список пользователей бота {App.Api.GetMeAsync().Result}:\n\n");
            UsersFunc.Refresh();
            foreach (var user in ListUsers)
                messageText.Append($"{++indexUser}) {user.IdUser_Nickname} | Is blocked: {user.IsBlocked} | Status: {user.Status}\n");
            return messageText.ToString();
        }

        public static Task SendListOfUsers(User user) => user.SendMessageAsync(ListOfUsersParseString());
        public static Task SaveChanges(User user) => user.SendMessageAsync(UsersFunc.SaveRefresh() ? "Успешное сохранение изменений" : "При сохранении изменений произошла ошибка");
        public static Task CancelChanges(User user) => user.SendMessageAsync(Application.Current.Dispatcher.Invoke(() => UsersFunc.Upload()) ? "Изменения успешно отменены" : "При отмене изменений произошла ошибка");

        public static User AuthNewUser(ICommand model, Message message)
        {
            int id;
            if (model is null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (model.Args is null)
            {
                throw new ArgumentNullException(nameof(model.Args));
            }

            if (model.CountArgsCommand == 0)
            {
                id = message.From.Id;
            }
            else if ((id = StrToInt(model.Args[0])) < 0)
            {
                throw new ArgumentException("id isn't in the correct format", nameof(id));
            }

            return AddUser(id, null);
        }

        public static User AddUser(int id, string userName)
        {
            User newUser = new User() { Id = id, LocalNickname = userName };
            if (UsersFunc.GetUser(id) is null)
            {

                App.UiContext.Send(x => ListUsers.Add(newUser), null);

                //newUser.SendMessageAsync($"Вы были успешно добавлены в список пользователей бота.\n" +
                //    $"Запросите у администратора бота {App.Api.GetMeAsync().Result} вас добавить в список разрешённых пользователей.\n\n" +
                //    $"Вы можете написать администратору бота используя команду \"{CollectionCommands.SendMessageToAdminCommand.ExampleCommand}\"");
                return newUser;

            }

            throw new ArgumentException($"Пользователь [{newUser.IdUser_Nickname}] уже существует!", nameof(id));
        }

        public static async Task AuthNewUser(ICommand model, User userSource, Message message)
        {
            string nickname = null;
            if (userSource is null)
            {
                throw new ArgumentNullException(nameof(userSource));
            }

            if (model.CountArgsCommand > 1)
            {
                nickname = ExCommon.GetOriginalArgs(model, message, 1);
            }

            User newUser = AuthNewUser(model, message);
            newUser.LocalNickname = nickname;
            await userSource.SendMessageAsync($"Пользователь [{newUser.IdUser_Nickname}] успешно добавлен в список пользователей.");
        }

        public static async Task<User> StartCommandUser(Message message)
        {
            User user = AddUser(message.From.Id, message.From.Username);
            await user.SendMessageAsync($"Вы были успешно добавлены в список пользователей бота.\n" +
                $"Запросите у администратора бота {App.Api.GetMeAsync().Result} вас добавить в список разрешённых пользователей.\n\n" +
                $"Вы можете написать администратору бота используя команду \"{CollectionCommands.SendMessageToAdminCommand.ExampleCommand}\"");
            return user;
        }

        public static Task DeauthUser(User user, int IdSelectedUser)
        {
            if (Application.Current.Dispatcher.Invoke(() => ListUsers.Remove(ListUsers.Where(f => f.Id == IdSelectedUser).DefaultIfEmpty(new User()).FirstOrDefault())))
                user.SendMessageAsync($"Пользователь c id: \"{IdSelectedUser}\" был успешно успешно удалён из списка пользователей.").Wait();
            else
                return user.SendMessageAsync($"Пользователя c id: \"{IdSelectedUser}\" не существует...");
            return SendListOfUsers(user);
        }

        public static Task DeauthUser(User user, string[] args)
        {
            string LocalNickname = string.Join(" ", args);
            if (Application.Current.Dispatcher.Invoke(() => ListUsers.Remove(ListUsers.Where(f => f.LocalNickname != null && f.LocalNickname.ToLower() == LocalNickname.ToLower() && !StrToInt(f.LocalNickname).Equals(f.Id)).DefaultIfEmpty(new User()).FirstOrDefault())))
                user.SendMessageAsync($"Пользователь c Nickname: \"{LocalNickname}\" был успешно успешно удалён из списка пользователей.").Wait();
            else
                return user.SendMessageAsync($"Пользователя c Nickname: \"{LocalNickname}\" не существует...");
            return SendListOfUsers(user);
        }

        public static Task ChangeLocalName(ICommand model, User user, Message message)
        {
            var selectedUser = ListUsers.Where(f => f.Id == StrToInt(model.Args[0])).DefaultIfEmpty(null)?.FirstOrDefault()
                ?? throw new ArgumentException($"Пользователь c id: \"{model.Args[0]}\" не найден...");

            string beforeChangingNickname = selectedUser.LocalNickname;
            selectedUser.LocalNickname = ExCommon.GetOriginalArgs(model, message, 1);
            user.SendMessageAsync($"Пользователю c id: \"{model.Args[0]}\" измененён Nickname: \"{beforeChangingNickname}\" => \"{selectedUser.LocalNickname}\".").Wait();
            return SendListOfUsers(user);
        }

        public static Task LockUser(User user, int IdSelectedUser)
        {
            var selectedUser = ListUsers.Where(f => f.Id == IdSelectedUser).FirstOrDefault();
            if (selectedUser is null)
                return user.SendMessageAsync($"Пользователь c id: \"{IdSelectedUser}\" не найден...");

            selectedUser.IsBlocked = !selectedUser.IsBlocked;
            user.SendMessageAsync($"Пользователь c id: \"{IdSelectedUser}\" успешно {(selectedUser.IsBlocked ? "заблокирован" : "разблокирован")}.").Wait();
            return SendListOfUsers(user);
        }

        public static Task LockUser(User user, string[] args)
        {
            string LocalNickname = string.Join(" ", args);
            var selectedUser = ListUsers.Where(f => f.LocalNickname.ToLower() == LocalNickname.ToLower()).FirstOrDefault();
            if (selectedUser is null || StrToInt(selectedUser.LocalNickname).Equals(selectedUser.Id))
                return user.SendMessageAsync($"Пользователь c Nickname: \"{LocalNickname}\" не найден...");

            selectedUser.IsBlocked = !selectedUser.IsBlocked;
            user.SendMessageAsync($"Пользователь c Nickname: \"{LocalNickname}\" успешно {(selectedUser.IsBlocked ? "заблокирован" : "разблокирован")}.").Wait();
            return SendListOfUsers(user);
        }
    }
}