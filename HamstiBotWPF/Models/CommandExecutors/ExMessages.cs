using System;
using System.Linq;
using System.Threading.Tasks;
using TBotHamsti.Models.Commands;
using TBotHamsti.Models.Users;
using Telegram.Bot.Types;
using User = TBotHamsti.Models.Users.User;

namespace TBotHamsti.Models.CommandExecutors
{
    public static class ExMessages
    {
        private static string RandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static async Task UserSentToAdmin(ICommand model, User user, Message message)
        {
            await StatusUser.Admin.SendMessageAsync($"Сообщение от пользователя \n[{user.IdUser_Nickname} | blocked: {user.IsBlocked}):\n\"{ExCommon.GetOriginalArgs(model, message)}\"");
            await user.SendMessageAsync("Сообщение успешно отправлено админу бота " + App.Api.GetMeAsync().Result);
        }

        public static async Task AdminSentToUser(User userSource, User userDestination, string[] args)
        {
            if (userDestination is null)
            {
                throw new ArgumentNullException(nameof(userDestination));
            }

            await userDestination.SendMessageAsync($"Сообщение от администратора бота {App.Api.GetMeAsync().Result}: \n\"{string.Join(" ", args.Skip(1))}\"" +
                $"\nВы можете написать администратору бота используя команду \"/sentToAdmin YourMessage\"");
            await userSource.SendMessageAsync($"Сообщение успешно отправлено пользователю \"{userDestination.IdUser_Nickname}\"");
        }

        public static async Task UserSpamFromAdmin(User userSource, User userDestination, int countMessages)
        {
            if (userDestination is null)
            {
                throw new ArgumentNullException(nameof(userDestination));
            }

            await userDestination.SendMessageAsync(
                $"Вы были выбраны жертвой для спама от администратора бота {await App.Api.GetMeAsync()}:\n" +
                $"\nВы можете написать администратору бота используя команду \"/sentToAdmin YourMessage\"");
            for (int i = 0; i < countMessages; i++)
            {
                await userDestination.SendMessageAsync(RandomString(new Random().Next(5, 40)));
            }
            await userDestination.SendMessageAsync("Спам успешно завершён. Хорошего дня ;>");
            await userSource.SendMessageAsync("Спам пользователя \"" + userDestination.IdUser_Nickname + "\" успешно завершён.");
        }
    }
}