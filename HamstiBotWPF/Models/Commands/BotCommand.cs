using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using TBotHamsti.Models.Messages;
using TBotHamsti.Models.Users;
using StatusUser = TBotHamsti.Models.Users.StatusUser;
using User = TBotHamsti.Models.Users.User;

namespace TBotHamsti.Models.Commands
{
    /// <summary>
    /// Structure for execute user commands
    /// </summary>
    public class BotCommand : ICommand
    {
        private const char ARG_START = '[';
        private const char ARG_END = ']';
        private readonly bool isLimitCountArgs = true;

        public string Command { get; }
        public string ExampleCommand { get; }
        public string[] Args { get; private set; }
        public int CountArgsCommand => isLimitCountArgs ? Args.Length : -1;
        public StatusUser StatusUser { get; set; }
        public LevelCommand NameOfLevel { get; set; }
        public Func<ICommand, User, Message, Task> Execute { get; set; }
        public Func<TextMessage, User, Message, Task> OnError { get; set; } = async (messageError, user, message)
            => await user.SendMessageAsync(messageError?.Text ?? DefaultErrorMessage(message));

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

        public BotCommand(string command, string[] args, bool isLimitCountArgs = true) : this(command)
        {
            if (args is null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            Args = args.Where(w => !w.Equals(command) || !string.IsNullOrWhiteSpace(w)).Select(s => ARG_START + s + ARG_END).ToArray();
            if (Args.Length.Equals(0))
            {
                throw new ArgumentException(nameof(Args) + " is empty, use " + nameof(BotCommand) + " (command) or correct args", nameof(Args));
            }

            this.isLimitCountArgs = isLimitCountArgs;
            ExampleCommand = string.Concat(command, ' ', string.Join(" ", Args));
        }

        public BotCommand(string command, string args, bool isLimitCountArgs = true) :
            this(command, new string[] { args ?? throw new ArgumentNullException(nameof(args)) }, isLimitCountArgs)
        { }

        /// <summary>
        /// Command converter to normal view
        /// </summary>
        /// <param name="messageText">Text of the incoming message from telegrams</param>
        /// <returns>Returns a command in normal form or emptiness in case of failure</returns>
        public static BotCommand ParseMessage(Message message)
        {
            string messageText = message.Text;
            if (!messageText.StartsWith("/"))
            {
                throw new ArgumentException($"\"{messageText}\" - incorrect command syntax. To get the list of commands: {CollectionCommands.HelpCommand.ExampleCommand}");
            }

            string[] splits = messageText.Split(' ');
            return new BotCommand(splits?.FirstOrDefault().ToLower())
            {
                Args = splits.Skip(1).Take(splits.Count()).ToArray()
            };
        }

        private static string DefaultErrorMessage(Message message)
        {
            return $"An error occurred while execute \"{message.Text}\" command. To get the list of commands: {CollectionCommands.HelpCommand.ExampleCommand}";
        }
    }
}
