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
        /// <summary>
        /// Sending a <see cref="Message"/> to all users with <see cref="StatusUser.Admin"/> by <see cref="ICommand"/>
        /// </summary>
        /// <inheritdoc cref="ExUsers.DeauthUser(ICommand, User, Message)"/>
        public static async Task SentToAdmins(ICommand model, User user, Message message)
        {
            string recivedMessage = model.GetOriginalArgs(message);
            await StatusUser.Admin.SendMessageAsync($"Message from user [{user.Id_Username} | {nameof(user.IsBlocked)}: {user.IsBlocked}):\n\"{recivedMessage}\"");
            await user.SendMessageAsync("The message was successfully sent to the administrators of the " + App.Api.GetMeAsync().Result);
        }

        /// <summary>
        /// Sending a <see cref="Message"/> to a group of users with <see cref="StatusUser"/> by <see cref="ICommand"/>
        /// </summary>
        /// <exception cref=" ArgumentOutOfRangeException">If status isn't found</exception>
        /// <inheritdoc cref="SentToAdmins(ICommand, User, Message)"/>
        public static async Task SentByStatus(ICommand model, User user, Message message)
        {
            if (!Enum.TryParse(model.GetArg(0), true, out StatusUser status))
            {
                throw new ArgumentOutOfRangeException($"Status {model.Args[0]} doesn't exist.");
            }

            string recivedMessage = model.GetOriginalArgs(message, 1);
            await status.SendMessageAsync($"Message from user [{user.Id_Username} | {nameof(user.IsBlocked)}: {user.IsBlocked}):\n\"{recivedMessage}\"");
            await user.SendMessageAsync($"The message was successfully sent to the {status} group of the {App.Api.GetMeAsync().Result}");
        }

        /// <summary>
        /// Sending a <see cref="Message"/> to a user with <see cref="User.Id"/> by <see cref="ICommand"/>
        /// </summary>
        /// <inheritdoc cref="SentToAdmins(ICommand, User, Message)"/>
        public static async Task SentById(ICommand model, User user, Message message)
        {
            string recivedMessage = model.GetOriginalArgs(message, 1);
            User userDestination = UsersFunc.GetUser(ExUsers.IdStrToInt(model.GetArg(0)));
            await userDestination.SendMessageAsync(
                $"Message from the {App.Api.GetMeAsync().Result} administrator: \n\"{recivedMessage}\"\n\n" +
                "You can write to the bot administrator using the command " + CollectionCommands.SendMessageToAdminCommand.ExampleCommand);
            await user.SendMessageAsync($"The message has been successfully sent to the [{userDestination.Id_Username}] user");
        }

        /// <summary>
        /// Spam a user by <see cref="ICommand"/>
        /// </summary>
        /// <exception cref="ArgumentException">If wrong type of number messages</exception>
        /// <inheritdoc cref="SentToAdmins(ICommand, User, Message)"/>
        public static async Task UserSpam(ICommand model, User user)
        {
            if (!int.TryParse(model.GetArg(1), out int countMessages))
            {
                throw new ArgumentException("Wrong type of number", nameof(countMessages));
            }

            User userDestination = UsersFunc.GetUser(ExUsers.IdStrToInt(model.GetArg(0)));
            await userDestination.SendMessageAsync(
                $"You have been selected as a spam victim by the {await App.Api.GetMeAsync()} admin\n\n" +
                $"You can write to the bot administrator using the command " + CollectionCommands.SendMessageToAdminCommand.ExampleCommand);
            
            for (int i = 0; i < countMessages; i++)
            {
                await userDestination.SendMessageAsync(RandomString(new Random().Next(5, 40)));
            }

            await userDestination.SendMessageAsync("Spam completed successfully. Have a nice day😉");
            await user.SendMessageAsync("Spam user [" + userDestination.Id_Username + "] successfully finished");
        }

        /// <summary>
        /// Creating a string with a random set of characters
        /// </summary>
        /// <param name="length">Output string length</param>
        /// <returns>A string with random characters of a certain length</returns>
        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}