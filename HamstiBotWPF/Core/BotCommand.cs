using System;
using System.Linq;
using Telegram.Bot.Types;

namespace HamstiBotWPF.Core
{
    /// <summary>
    /// Structure for execute user commands
    /// </summary>
    public class BotCommand
    {
        public string Command { get; set; }
        public string ExampleCommand { get; set; }
        public int CountArgsCommand { get; set; } = 0;
        public BotLevelCommand.LevelCommand NameOfLevel { get; set; } = BotLevelCommand.LevelCommand.Root;
        //public bool LevelDependent { get; set; } = true;
        public bool VisibleForUsers { get; set; } = true;
        public Action<BotCommandStructure, Message> Execute { get; set; }
        public Action<BotCommandStructure, Message> OnError { get; set; } = async (model, message) =>
        {
            await GlobalUnit.Api.SendTextMessageAsync(message.From.Id, "Не верное кол-во агрументов\nСписок комманд: /help");
        };

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
                var command = splits?.FirstOrDefault();
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
