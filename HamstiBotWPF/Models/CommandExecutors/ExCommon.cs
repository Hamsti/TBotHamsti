using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TBotHamsti.Models.Commands;
using TBotHamsti.Models.Users;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using User = TBotHamsti.Models.Users.User;

namespace TBotHamsti.Models.CommandExecutors
{
    /// <summary>
    /// Implementation of all bot functionality
    /// </summary>
    public static class ExCommon
    {
        /// <summary>
        /// The argument to display all commands on the keyboard
        /// </summary>
        public const string ARG_FULL_KEYBOARD = "All";

        /// <value>Creating a new exception with the wrong number of arguments</value>
        private static Exception ExceptionWrongNumberOfArgs => new ArgumentOutOfRangeException("Invalid number of arguments\nTo get a list of commands: " + CollectionCommands.HelpCommand.ExampleCommand);

        /// <summary>
        /// Creating a help string
        /// </summary>
        /// <param name="user">Search by <see cref="BotLevelCommand"/> and <see cref="StatusUser"/></param>
        /// <returns>Help string for current <paramref name="user"/></returns>
        public static string GetHelp(User user) => string.Join("\n", BotLevelCommand.GetBotLevelCommand(user).CommandsOfLevel.Where(w => w.StatusUser <= user.Status).Select(s => s.ExampleCommand));

        /// <summary>
        /// Sending help to the <paramref name="user"/>
        /// </summary>
        /// <inheritdoc cref="ExUsers.DeauthUser(ICommand, User, Message)"/>
        public static Task HelpBot(User user) => user.SendMessageAsync("List of commands:\n" + GetHelp(user));

        /// <summary>
        /// Getting a substring of arguments in a message
        /// </summary>
        /// <param name="skipArgs">Number of arguments to skip</param>
        /// <returns>The substring of arguments</returns>
        /// <exception cref="ExceptionWrongNumberOfArgs"/>
        public static string GetOriginalArgs(this ICommand model, Message message, int skipArgs = 0) => model.Args.Length > skipArgs
                ? message.Text.Substring(message.Text.LastIndexOf(model.Args[skipArgs]))
                : throw ExceptionWrongNumberOfArgs;

        /// <summary>
        /// Getting an argument from a message
        /// </summary>
        /// <param name="indexArg">Index of argument</param>
        /// <returns>The message argument</returns>
        /// <exception cref="ExceptionWrongNumberOfArgs"/>
        public static string GetArg(this ICommand model, int indexArg) => indexArg < model.CountArgsCommand
                ? model?.Args[indexArg] ?? throw new ArgumentNullException(nameof(model))
                : throw ExceptionWrongNumberOfArgs;

        /// <summary>
        /// Forced shutdown of the application by the <see cref="ICommand"/>.
        /// </summary>
        /// <inheritdoc cref="HelpBot(User)"/>
        public static async Task ComStopApp(User user)
        {
            await user.SendMessageAsync($"Force the user [{user.Id_Username}] to terminate the application");
            await ExecutionBot.StopBotAsync();
            App.UiContext.Send(x => Application.Current.Shutdown(0), null);
        }

        /// <summary>
        /// Updating keyboard shortcuts to the <paramref name="user"/>
        /// </summary>
        /// <param name="arg">Argument for show full, short or hide keyboard</param>
        /// <inheritdoc cref="HelpBot(User)"/> 
        public static Task ShowScreenButtons(User user, string arg)
        {
            string[] keys = ParserKeys(arg, user.Status);

            int countColsKeys = 3;
            var rkm = new ReplyKeyboardMarkup();
            var rows = new List<KeyboardButton[]>();
            var cols = new List<KeyboardButton>();

            for (int i = 0; i < keys.Count(); i++)
            {
                cols.Add(new KeyboardButton(keys[i]));
                if ((i + 1) % countColsKeys == 0 || i + 1 == keys.Count())
                {
                    rows.Add(cols.ToArray());
                    cols = new List<KeyboardButton>();
                }
            }

            rkm.Keyboard = rows.ToArray();
            rkm.OneTimeKeyboard = true;

            return keys.Count() == 0
                ? user.SendMessageAsync("The on-screen keyboard was successfully removed", new ReplyKeyboardRemove())
                : user.SendMessageAsync("Number of buttons added: " + keys.Count(), replyMarkup: rkm);
        }

        /// <summary>
        /// Creating string array type for keyboard
        /// </summary>
        /// <param name="arg"><paramref name="arg"/> for show full, short or hide keyboard</param>
        /// <param name="status">Commands which allowed by the user with <paramref name="status"/></param>
        /// <returns>A string array of <see cref="ICommand.Command"/> or empty, if <paramref name="arg"/> == false</returns>
        /// <exception cref="ArgumentException">Wrong <paramref name="arg"/> for execution</exception>
        private static string[] ParserKeys(string arg, StatusUser status)
        {
            if (bool.TryParse(arg, out bool isShowKeys))
            {
                return isShowKeys ? CollectionCommands.RootLevel.CommandsOfLevel.Where(x => x.CountArgsCommand == 0 && x.StatusUser <= status)
                    .Select(s => s.Command)
                    .ToArray() : new string[0];
            }
            else
            {
                return arg.ToLower() == ARG_FULL_KEYBOARD ? CollectionCommands.RootLevel.CommandsOfLevel.Where(w => w.StatusUser <= status)
                    .Select(s => s.Command)
                    .ToArray() : throw new ArgumentException("No argument found to execute command", nameof(arg));
            }
        }
    }
}