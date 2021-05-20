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
        private string command;

        public string Command
        {
            get => command;
            set => command = value.ToLower();
        }

        public string[] Args { get; set; } = Array.Empty<string>();
        public string ExampleCommand { get; set; }
        public int CountArgsCommand { get; set; } = 0;
        public LevelCommand ParrentLevel { get; set; } //parrent
        public LevelCommand NameOfLevel { get; set; } = LevelCommand.None;
        public StatusUser StatusUser { get; set; } = StatusUser.User;
        public Action<ITCommand, PatternUser, Message> Execute { get; set; }
        public Action<ITCommand, PatternUser, Message> OnError { get; set; } = async (model, user, message) => await RepBotActions.SendMessageWrongNumberOfArgs(user);

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

                return new BotCommand
                {
                    Command = command,
                    Args = args
                };
            }
            return null;
        }

        public bool LevelDependent { get; set; }
    }
}
