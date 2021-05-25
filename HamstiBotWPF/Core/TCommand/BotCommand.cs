using System;
using System.Linq;
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
        public string Command { get; }
        public string ExampleCommand { get; }
        public string[] Args { get; private set; }
        public int CountArgsCommand { get; set; }
        public StatusUser StatusUser { get; set; }
        public LevelCommand NameOfLevel { get; set; } 
        public Action<ITCommand, PatternUser, Message> Execute { get; set; }
        public Action<ITCommand, PatternUser, Message> OnError { get; set; }

        public BotCommand(string command) : this(command, command) 
        {
            CountArgsCommand = default;
        }

        public BotCommand(string command, string exampleCommand) 
        {
            Command = command.ToLower();
            ExampleCommand = exampleCommand;
            Args = Array.Empty<string>();
            StatusUser = StatusUser.User;
            OnError = async (model, user, message) => await RepBotActions.SendMessageWrongNumberOfArgs(user);
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
    }
}
