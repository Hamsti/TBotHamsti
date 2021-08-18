using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TBotHamsti.Models.CommandExecutors;
using TBotHamsti.Models.Commands;
using TBotHamsti.Models.Messages;
using TBotHamsti.Models.Users;
using TBotHamsti.ViewModels;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using BotCommand = TBotHamsti.Models.Commands.BotCommand;
using User = TBotHamsti.Models.Users.User;

namespace TBotHamsti.Models
{
    /// <summary>
    /// Main bot control (checking <see cref="Message"/>, handling <see cref="ICommand"/>, starting, stopping, reloading the bot)
    /// </summary>
    internal static class ExecutionBot
    {
        /// <summary>
        /// Taking further actions based on the <paramref name="user"/> and the <paramref name="message"/> type
        /// </summary>
        /// <param name="user">by whom the execution of the function was called</param>
        /// <param name="message"><paramref name="message"/> for analysis</param>
        /// <returns><see cref="Task"/> of waiting for the complete execution</returns>
        /// <exception cref="ArgumentNullException">if <paramref name="message"/> is null</exception>
        internal static Task CheckMessage(ref User user, Message message)
        {
            static Task TextMessage(User user, Message message)
            {
                try
                {
                    var model = BotCommand.ParseMessage(message);
                    return user.IsBlocked && model.Command != CollectionCommands.SendMessageToAdminCommand.Command
                        ? user.SendMessageAsync($"You're [{user.Id}] locked out now. Ask the bot admin {App.Api.GetMeAsync().Result} to add you to the list of allowed users\n\n" +
                            $"To write the bot admin: \"{CollectionCommands.SendMessageToAdminCommand.ExampleCommand}\"")
                        : HandleTextCommand(model, user, message);
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    // The trying to find a command at all levels and send result
                    // search CLR by tree in deep
                    throw ex;
                }
            }

            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (user is null)
            {
                try
                {
                    user = UsersFunc.GetUser(message.From.Id);
                    return CheckMessage(ref user, message);
                }
                catch (ArgumentNullException)
                {
                    return ExUsers.StartCommandUser(message);
                }
            }

            return message.Type switch
            {
                MessageType.Text => TextMessage(user, message),
                MessageType.Photo => ExPC.ImageUploaderAsync(user, message),
                MessageType.Document => ExPC.DocumentUploaderAsync(user, message),
                _ => user.SendMessageAsync($"\"{message.Type}\" - unknown type of message"),
            };
        }

        /// <summary>
        /// Starting execution of the found match of the <see cref="ICommand"/> in the <see cref="CollectionCommands"/> and the <paramref name="model"/>
        /// </summary>
        /// <param name="model"><paramref name="message"/> from the <paramref name="user"/> converted to an object of the type <see cref="ICommand"/><para/>
        /// Using to search for a command and analyze its correctness</param>
        /// <inheritdoc cref="CheckMessage(ref User, Message)" path="/param"/>
        /// <exception cref="ArgumentException">If <see cref="StatusUser"/> of the <paramref name="user"/> is lower than required</exception>
        /// <exception cref="ArgumentException">If the wrong number of arguments in the <paramref name="model"/></exception>
        /// <exception cref="ArgumentOutOfRangeException">If <see cref="ICommand"/> wasn't found at <paramref name="user"/> <see cref="BotLevelCommand"/></exception>
        internal static Task HandleTextCommand(ICommand model, User user, Message message)
        {
            var commands = BotLevelCommand.GetBotLevelCommand(user).CommandsOfLevel.Where(w => w.Command.Equals(model.Command));
            foreach (ICommand tCommand in commands)
            {
                if (tCommand.CountArgsCommand == model.CountArgsCommand ||
                    tCommand.CountArgsCommand == -1 && model.CountArgsCommand > 0)
                {
                    if (tCommand.StatusUser <= user.Status)
                    {
                        try
                        {
                            LogsViewModel.MessageBus.SendTo<LogsViewModel>(
                                new TextMessage($"The command \"{message.Text}\" execution for [{user.Id_Username}]", HorizontalAlignment.Right)).Wait();

                            return tCommand.Execute.Invoke(model, user, message);
                        }
                        catch (Exception ex)
                        {
                            string exMessage = HandlerException.GetExceptionMessage(ex);
                            LogsViewModel.MessageBus.SendTo<LogsViewModel>(new TextMessage(
                                $"An error occurred by message: \"{message.Text}\".\n" +
                                $"The user: [{user.Id_Username}]\n" +
                                $"Execution interrupted of the command: {model.ExampleCommand}\n{exMessage}", HorizontalAlignment.Right)).Wait();

                            return tCommand.OnError.Invoke(new TextMessage(exMessage), user, message);
                        }
                    }

                    throw new ArgumentException($"To execute the command \"{model.Command}\", the user status is required " +
                            $"\"{tCommand.StatusUser}\"{(Enum.GetValues(typeof(StatusUser)).Cast<int>().Max() != (int)tCommand.StatusUser ? " and higher" : string.Empty)}",
                            nameof(user.Status));
                }
            }

            throw commands.Any()
                ? new ArgumentException("Wrong count of args", nameof(model.CountArgsCommand))
                : new ArgumentOutOfRangeException(nameof(commands),
                    $"The command \"{message.Text}\" wasn't found. To get the list of commands: {CollectionCommands.HelpCommand.ExampleCommand}");
        }


        /// <summary>
        /// Start receiving messages by the bot
        /// </summary>
        /// <returns><see cref="Task"/> of waiting for the complete execution</returns>
        internal static async Task StartReceivingBotAsync()
        {
            string message;
            UsersFunc.Upload();
            if (!App.Api.IsReceiving)
            {
                App.Api.StartReceiving(Array.Empty<UpdateType>());
                message = $"The bot {await App.Api.GetMeAsync()} started receiving successfully";
            }
            else
            {
                message = $"The bot {await App.Api.GetMeAsync()} is receiving already";
            }

            await LogsViewModel.MessageBus.SendTo<LogsViewModel>(new TextMessage(message, HorizontalAlignment.Right));
            await StatusUser.Admin.SendMessageAsync(message);
        }

        /// <summary>
        /// Stop receiving messages by the bot
        /// </summary>
        /// <inheritdoc cref="StartReceivingBotAsync" path="/returns"/>
        internal static async Task StopBotAsync()
        {
            UsersFunc.SaveToFile();
            try
            {
                string message = App.Api.IsReceiving
                    ? $"The bot {await App.Api.GetMeAsync()} stopped receiving successfully"
                    : $"The bot {await App.Api.GetMeAsync()} has already been stopped";
                await LogsViewModel.MessageBus.SendTo<LogsViewModel>(new TextMessage(message, HorizontalAlignment.Right));
                await StatusUser.Admin.SendMessageAsync(message);
            }
            finally
            {
                if (App.Api.IsReceiving)
                {
                    App.Api.StopReceiving();
                }
            }
        }

        /// <summary>
        /// Restart receiving messages by the bot
        /// </summary>
        /// <inheritdoc cref="StartReceivingBotAsync" path="/returns"/>
        internal static async Task RestartBotAsync()
        {
            try
            {
                await StopBotAsync();
            }
            finally
            {
                await StartReceivingBotAsync();
                string message = $"The bot {await App.Api.GetMeAsync()} restarted receiving successfully";
                await LogsViewModel.MessageBus.SendTo<LogsViewModel>(new TextMessage(message, HorizontalAlignment.Right));
                await StatusUser.Admin.SendMessageAsync(message);
            }
        }
    }
}
