using System;
using System.Linq;
using Telegram.Bot.Types;
using StatusUser = TBotHamsti.LogicRepository.RepUsers.StatusUser;

namespace TBotHamsti.Core
{
    /// <summary>
    /// Structure for execute user commands
    /// </summary>
    public class BotCommand
    {
        private string command;

        public string Command 
        { 
            get => command;
            set => command = value.ToLower();
        }
        public string ExampleCommand { get; set; }
        public int CountArgsCommand { get; set; } = 0;
        public BotLevelCommand.LevelCommand NameOfLevel { get; set; } = BotLevelCommand.LevelCommand.Root;
        public bool LevelDependent { get; set; } = true;
        public StatusUser StatusUser { get; set; } = StatusUser.User;
        public Action<BotCommandStructure, Message> Execute { get; set; }
        public Action<BotCommandStructure, Message> OnError { get; set; } = async (model, message) => await LogicRepository.RepBotActions.SendMessageWrongNumberOfArgs(message);

        /// <summary>
        /// Command converter to normal view
        /// </summary>
        /// <param name="messageText">Text of the incoming message from telegrams</param>
        /// <returns>Returns a command in normal form or emptiness in case of failure</returns>
        public static BotCommandStructure ParserCommand(string messageText)
        {
            if (messageText.StartsWith("/"))
            {
                var splits = messageText.Split(' ');
                var command = splits?.FirstOrDefault().ToLower();
                var args = splits.Skip(1).Take(splits.Count()).ToArray();

                return new BotCommandStructure
                {
                    Command = command,
                    Args = args
                };
            }
            return null;
        }
    }
}
