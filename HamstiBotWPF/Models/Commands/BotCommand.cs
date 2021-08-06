using System;
using System.Linq;
using System.Threading.Tasks;
using TBotHamsti.Models.Messages;
using TBotHamsti.Models.Users;
using Telegram.Bot.Types;

namespace TBotHamsti.Models.Commands
{
    /// <summary>
    /// Structure for execute user commands
    /// </summary>
    public class BotCommand : ICommand
    {
        private const char ARG_START = '[';
        private const char ARG_END = ']';

        /// <summary>
        /// Is the <see cref="CountArgsCommand"/> limited?
        /// </summary>
        private readonly bool isLimitCountArgs = true;
        public string Command { get; }
        public string ExampleCommand { get; }
        public string[] Args { get; private set; }
        public int CountArgsCommand => isLimitCountArgs ? Args.Length : -1;
        public StatusUser StatusUser { get; set; }
        public LevelCommand NameOfLevel { get; set; }
        public Func<ICommand, Users.User, Message, Task> Execute { get; set; }
        public Func<TextMessage, Users.User, Message, Task> OnError { get; set; } = async (messageError, user, message)
            => await user.SendMessageAsync(messageError?.Text ?? DefaultErrorMessage(message));

        /// <summary>
        /// Creating a <paramref name="command"/> with no arguments
        /// </summary>
        /// <param name="command">Value of form: /<paramref name="command"/></param>
        /// <exception cref="ArgumentException"><paramref name="command"/> is empty or white space</exception>
        /// <exception cref="ArgumentNullException"><paramref name="command"/> is null</exception>
        public BotCommand(string command)
        {
            if (command is null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            if (string.IsNullOrWhiteSpace(command))
            {
                throw new ArgumentException(nameof(command));
            }

            ExampleCommand = command.Trim();
            Command = ExampleCommand.ToLower();
            Args = Array.Empty<string>();
            StatusUser = StatusUser.User;
        }

        /// <summary>
        /// Creating a <paramref name="command"/> with <paramref name="args"/>
        /// </summary>
        /// <param name="args">An item of <paramref name="args"/> of the form: <see cref="ARG_START"/> + <paramref name="args"/>[0] + <see cref="ARG_END"/></param>
        /// <param name="isLimitCountArgs">Is the <see cref="CountArgsCommand"/> limited?</param>
        /// <exception cref="ArgumentException"><paramref name="args"/> is empty</exception>
        /// <exception cref="ArgumentNullException"><paramref name="args"/> is null</exception>
        /// <inheritdoc cref="BotCommand(string)"/>
        public BotCommand(string command, string[] args, bool isLimitCountArgs = true) : this(command)
        {
            if (args is null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            Args = args.Where(w => !w.Equals(command) || !string.IsNullOrWhiteSpace(w)).Select(s => ARG_START + s + ARG_END).ToArray();
            if (Args.Length.Equals(0))
            {
                throw new ArgumentException(nameof(Args) + " is empty, use " + nameof(BotCommand) + "(command) or correct args", nameof(Args));
            }

            this.isLimitCountArgs = isLimitCountArgs;
            ExampleCommand = string.Concat(command, ' ', string.Join(" ", Args));
        }

        /// <param name="args">A single <paramref name="args"/> of the form: <see cref="ARG_START"/> + <paramref name="args"/> + <see cref="ARG_END"/></param>
        /// <inheritdoc cref="BotCommand(string, string[], bool"/>
        public BotCommand(string command, string args, bool isLimitCountArgs = true) :
            this(command, new string[] { args ?? throw new ArgumentNullException(nameof(args)) }, isLimitCountArgs)
        { }

        /// <summary>
        /// Parse a user <paramref name="message"/> to <see cref="BotCommand"/>
        /// </summary>
        /// <param name="message">A <paramref name="message"/> from a telegram user</param>
        /// <returns>A <see cref="BotCommand"/> for interacting with any <see cref="ICommand"/> in <see cref="CollectionCommands"/> of the bot</returns>
        public static BotCommand ParseMessage(Message message)
        {
            string messageText = message.Text ?? throw new ArgumentNullException(nameof(message.Text));
            if (!messageText.StartsWith("/"))
            {
                throw new ArgumentException($"\"{messageText}\" - incorrect command syntax.\nTo get the list of commands: {CollectionCommands.HelpCommand.ExampleCommand}");
            }

            string[] splits = messageText.Split(' ') ?? throw new ArgumentNullException(nameof(splits));
            return new BotCommand(splits.FirstOrDefault().ToLower())
            {
                Args = splits.Skip(1).Take(splits.Count()).ToArray()
            };
        }

        /// <summary>
        /// Generating a default error message during executing a command
        /// </summary>
        /// <param name="message">The <paramref name="message"/> that caused the error</param>
        /// <returns>An error message</returns>
        private static string DefaultErrorMessage(Message message) =>
            $"An error occurred while execute \"{message.Text}\" command. To get the list of commands: {CollectionCommands.HelpCommand.ExampleCommand}";
    }
}
