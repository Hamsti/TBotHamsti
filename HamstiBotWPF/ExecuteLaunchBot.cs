using System;
using System.Linq;
using System.Windows;
using System.Threading.Tasks;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TBotHamsti.Core;
using TBotHamsti.LogicRepository;
using TBotHamsti.ViewModels;
using StatusUser = TBotHamsti.LogicRepository.RepUsers.StatusUser;
using BotCommand = TBotHamsti.Core.BotCommand;

namespace TBotHamsti
{
    /// <summary>
    /// To control the bot (reading messages, start, stop, reload)
    /// </summary>
    public static class ExecuteLaunchBot
    {
        /// <summary>
        /// Listening to all incoming messages
        /// </summary>
        public static async void CheckMessageBot(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            var user = RepUsers.AuthUsers.Where(user => user.Id == message.From.Id).FirstOrDefault();

            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (user is null)
            {
                user = await App.Current.Dispatcher.InvokeAsync(() => RepBotActions.ControlUsers.AuthNewUser(message, null, message.From.Id)).Result;
            }

            if (user.IsBlocked)
            {
                await user.SendMessageAsync($"You're locked out now. Ask the bot admin {await App.Api.GetMeAsync()} to add you to the list of allowed users\n\nTo write the bot admin: \"{CollectionCommands.SendMessageToAdminCommand.ExampleCommand}\"");
            }
            else
            {
                switch (message.Type)
                {
                    case MessageType.Text:
                        var model = BotCommand.ParserCommand(message.Text);
                        if (!ExecuteTextCommand(model, user, message).Result)
                        { 
                            // The question of trying to find a command at all levels
                            // search CLR by tree in deep
                        }
                        break;
                    //Image received from user
                    case MessageType.Photo:
                        await RepBotActions.ImageUploader(user, message); break;
                    //Document received from user
                    case MessageType.Document:
                        await RepBotActions.DocumentUploader(user, message); break;
                    default:
                        await user.SendMessageAsync($"\"{message.Type}\" - unknown type of message"); break;
                }
            }
        }

        internal static async Task<bool> ExecuteTextCommand(ITCommand model, PatternUser user, Message message)
        {
            //System.Collections.Generic.List<Task> IsAllTasksCompleted = new System.Collections.Generic.List<Task>();  ///implement after

            if (model is null)
            {
                await user.SendMessageAsync($"\"{message.Text}\" - incorrect command syntax. To get the list of commands: {CollectionCommands.HelpCommand.ExampleCommand}");
                return false;
            }

            var sourceOfCommands = await BotLevelCommand.GetBotLevelCommand(user);
            foreach (var tCommand in sourceOfCommands.CommandsOfLevel)
            {
                if (tCommand.Command == model.Command && (tCommand.CountArgsCommand == model.CountArgsCommand ||
                                                          tCommand.CountArgsCommand == -1 && model.CountArgsCommand > 0))
                {
                    if (tCommand.StatusUser <= user.Status)
                    {
                        try
                        {
                            //await tCommand.Execute(model, user, message);
                            await tCommand.Execute?.Invoke(model, user, message);

                            await App.Current.Dispatcher.InvokeAsync(() => LogsViewModel.MessageBus.SendTo<LogsViewModel>(
                                new Messages.TextMessage($"The command \"{message.Text}\" execution for [{user.IdUser_Nickname}]", HorizontalAlignment.Right)));
                        }
                        catch (Exception ex)
                        {
                            await tCommand.OnError?.Invoke(model, user, message);

                            await App.Current.Dispatcher.InvokeAsync(() => LogsViewModel.MessageBus.SendTo<LogsViewModel>(
                                new Messages.TextMessage($"An error occurred while execute \"{model.Command}\" command. The user [{user.IdUser_Nickname}] message : \"{message.Text}\"" +
                                $"\nThe exception message: \"{ex.Message}\"", HorizontalAlignment.Right)));
                        }
                        return true;
                    }
                    else
                    {
                        await user.SendMessageAsync($"To execute the command \"{model.Command}\", the user status is required " +
                            $"\"{tCommand.StatusUser}\"{(Enum.GetValues(typeof(StatusUser)).Cast<int>().Max() != (int)tCommand.StatusUser ? " and higher" : string.Empty)}");
                    }
                }
            }

            await user.SendMessageAsync($"The command \"{message.Text}\" wasn't found. To get the list of commands: {CollectionCommands.HelpCommand.ExampleCommand}");
            return false;
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
