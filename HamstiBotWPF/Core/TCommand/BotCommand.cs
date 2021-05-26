using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using TBotHamsti.LogicRepository;
using StatusUser = TBotHamsti.LogicRepository.RepUsers.StatusUser;
using LevelCommand = TBotHamsti.Core.BotLevelCommand.LevelCommand;

namespace TBotHamsti.Core
{
    /// <summary>
    /// Structure for execute user commands
    /// </summary>
    public class BotCommand : ITCommand
    {
        private Func<ITCommand, PatternUser, Message, Task> onError;

        public string Command { get; }
        public string ExampleCommand { get; }
        public string[] Args { get; private set; }
        public int CountArgsCommand { get; private set; }
        public StatusUser StatusUser { get; set; }
        public LevelCommand NameOfLevel { get; set; }
        public Func<ITCommand, PatternUser, Message, Task> Execute { get; set; }
        public Func<ITCommand, PatternUser, Message, Task> OnError
        {
            get => onError ?? ConstDefaultErrorHandlersAsync;
            set
            {
                onError = value;
                if (value != null && onError.GetInvocationList().Length == 1)
                {
                    onError += ConstDefaultErrorHandlersAsync;
                }
            }
        }

        public BotCommand(string command)
        {
            ExampleCommand = command.Trim();
            Command = ExampleCommand.ToLower();
            Args = Array.Empty<string>();
            StatusUser = StatusUser.User;
        }

        public BotCommand(string command, string args, int countArgsCommand) : this(command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                throw new ArgumentNullException(nameof(command));
            }

            if (string.IsNullOrWhiteSpace(args))
            {
                throw new ArgumentNullException(nameof(args));
            }

            if (countArgsCommand == default)
            {
                throw new ArgumentException("Can't be zero", nameof(countArgsCommand));
            }

            args = args.Replace(command, string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(args))
            {
                ExampleCommand = command + " " + args;
                CountArgsCommand = countArgsCommand;
            }
            else
            {
                throw new ArgumentException("Has to contain " + nameof(args));
            }
        }

        /// <summary>
        /// Command converter to normal view
        /// </summary>
        /// <param name="messageText">Text of the incoming message from telegrams</param>
        /// <returns>Returns a command in normal form or emptiness in case of failure</returns>
        public static ITCommand ParserCommand(string messageText)
        {
            if (messageText.StartsWith("/"))
            {
                var splits = messageText.Split(' ');
                var command = splits?.FirstOrDefault().ToLower();
                var args = splits.Skip(1).Take(splits.Count()).ToArray();

                return new BotCommand(command)
                {
                    Args = args,
                    CountArgsCommand = args.Length,
                    NameOfLevel = LevelCommand.None
                };
            }

            return null;
        }

        private static async Task ConstDefaultErrorHandlersAsync(ITCommand model, PatternUser user, Message message) =>
            await user.SendMessageAsync($"An error occurred while execute \"{message.Text}\" command. " +
                                                $"To get the list of commands: {CollectionCommands.HelpCommand.ExampleCommand}");
    }
}
