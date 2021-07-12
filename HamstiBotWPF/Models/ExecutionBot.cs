using System;
using System.Linq;
using System.Windows;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TBotHamsti.ViewModels;
using TBotHamsti.Models.CommandExecutors;
using TBotHamsti.Models.Messages;
using TBotHamsti.Models.Commands;
using TBotHamsti.Models.Users;
using BotCommand = TBotHamsti.Models.Commands.BotCommand;
using User = TBotHamsti.Models.Users.User;

namespace TBotHamsti.Models
{
    /// <summary>
    /// To control the bot (reading messages, start, stop, reload)
    /// </summary>
    public static class ExecutionBot
    {
        /// <summary>
        /// Listening to all incoming messages
        /// </summary>
        public static async Task CheckMessage(Message message, User user = null)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var model = BotCommand.ParseMessage(message) ?? throw new ArgumentNullException("Parse message");
            if (user is null)
            {
                await ExUsers.StartCommandUser(message);
            }
            else if (user.IsBlocked && model.Command != CollectionCommands.SendMessageToAdminCommand.Command)
            {
                await user.SendMessageAsync($"You're [{user.Id}] locked out now. Ask the bot admin {await App.Api.GetMeAsync()} to add you to the list of allowed users\n\nTo write the bot admin: \"{CollectionCommands.SendMessageToAdminCommand.ExampleCommand}\"");
            }
            else
            {
                switch (message.Type)
                {
                    case MessageType.Text:
                        try
                        {
                            await ExecuteTextCommand(model, user, message);
                        }
                        catch (ArgumentOutOfRangeException ex)
                        {
                            // The trying to find a command at all levels and send result
                            // search CLR by tree in deep
                            throw ex;
                        }
                        break;
                    //Image received from user
                    case MessageType.Photo:
                        await ExCommon.ImageUploader(user, message); break;
                    //Document received from user
                    case MessageType.Document:
                        await ExCommon.DocumentUploader(user, message); break;
                    default:
                        await user.SendMessageAsync($"\"{message.Type}\" - unknown type of message"); break;
                }
            }
        }

        internal static async Task ExecuteTextCommand(ICommand model, User user, Message message)
        {
            //System.Collections.Generic.List<Task> IsAllTasksCompleted = new System.Collections.Generic.List<Task>();  ///implement after

            var commands = BotLevelCommand.GetBotLevelCommand(user).CommandsOfLevel.Where(w => w.Command.Equals(model.Command));
            var lastCommand = commands.DefaultIfEmpty(null)?.Last() ?? throw new ArgumentOutOfRangeException(nameof(commands),
                $"The command \"{message.Text}\" wasn't found. To get the list of commands: {CollectionCommands.HelpCommand.ExampleCommand}");

            foreach (ICommand tCommand in commands)
            {
                if (tCommand.CountArgsCommand == model.CountArgsCommand ||
                    tCommand.CountArgsCommand == -1 && model.CountArgsCommand > 0)
                {
                    if (tCommand.StatusUser <= user.Status)
                    {
                        try
                        {
                            await LogsViewModel.MessageBus.SendTo<LogsViewModel>(
                                new TextMessage($"The command \"{message.Text}\" execution for [{user.IdUser_Nickname}]", HorizontalAlignment.Right));

                            await tCommand.Execute?.Invoke(model, user, message);
                        }
                        catch (Exception ex)
                        {
                            string exMessage = ex.InnerException?.Message ?? ex.Message;
                            await LogsViewModel.MessageBus.SendTo<LogsViewModel>(new TextMessage(
                                $"An error occurred by message: \"{message.Text}\".\n" +
                                $"The user: [{user.IdUser_Nickname}]\n" +
                                $"Execution interrupted of the command: {model.ExampleCommand}\n" +
                                $"The exception: \"{exMessage}\"", HorizontalAlignment.Right));

                            await tCommand.OnError?.Invoke(new TextMessage(exMessage), user, message);
                        }
                        return;
                    }

                    throw new ArgumentException($"To execute the command \"{model.Command}\", the user status is required " +
                            $"\"{tCommand.StatusUser}\"{(Enum.GetValues(typeof(StatusUser)).Cast<int>().Max() != (int)tCommand.StatusUser ? " and higher" : string.Empty)}",
                            nameof(user.Status));
                }
                else if (lastCommand.Equals(tCommand))
                {
                    throw new ArgumentException("Wrong count of args", nameof(model.CountArgsCommand));
                }
            }
        }


        /// <summary>
        /// To launch this bot
        /// </summary>
        /// <param name="Attempt">The number of attempts to launch the bot</param>
        public async static Task RunBotAsync(int numberAttempt = 1)
        {
            if (numberAttempt <= 0)
            {
                await StatusUser.Admin.SendMessageAsync($"The bot {App.Api.GetMeAsync().Result} failed to start");
                return;
            }

            if (!App.Api.IsReceiving)
            {
                App.Api.StartReceiving(Array.Empty<UpdateType>());
                await StatusUser.Admin.SendMessageAsync($"The bot {App.Api.GetMeAsync().Result} has started by user: {Environment.UserDomainName}");
            }
            else
            {
                await StatusUser.Admin.SendMessageAsync($"The bot {App.Api.GetMeAsync().Result} is running already");
                await RunBotAsync(--numberAttempt);
            }
        }

        /// <summary>
        /// To stop this bot
        /// </summary>
        /// <param name="Attempt">The number of attempts to stop the bot</param>
        public async static Task StopBotAsync(int numberAttempt = 1)
        {
            if (numberAttempt <= 0)
            {
                await StatusUser.Admin.SendMessageAsync($"The bot {App.Api.GetMeAsync().Result} failed to stop");
                return;
            }

            if (App.Api.IsReceiving)
            {
                await StatusUser.Admin.SendMessageAsync($"The bot {App.Api.GetMeAsync().Result} was successfully stopped by user: {Environment.UserDomainName}");
                App.Api.StopReceiving();
            }
            else
            {
                await StatusUser.Admin.SendMessageAsync($"The bot {App.Api.GetMeAsync().Result} has already been stopped");
                await StopBotAsync(--numberAttempt);
            }
        }

        /// <summary>
        /// To reload this bot
        /// </summary>
        /// <param name="numberAttempt">The number of attempts to reload the bot</param>
        public static async Task RestartBotAsync(int numberAttempt = 1)
        {
            await StatusUser.Admin.SendMessageAsync($"The bot {App.Api.GetMeAsync().Result} is restarting");

            if (App.Api.IsReceiving)
            {
                await StopBotAsync(numberAttempt);
                await RunBotAsync(numberAttempt);
            }
            else
                await RunBotAsync(numberAttempt);
        }
    }
}
