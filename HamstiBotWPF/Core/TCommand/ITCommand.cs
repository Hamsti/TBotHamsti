using System;
using System.Threading.Tasks;
using TBotHamsti.LogicRepository;
using TBotHamsti.Messages;
using Telegram.Bot.Types;

namespace TBotHamsti.Core
{
    public interface ITCommand
    {
        string Command { get; }
        string ExampleCommand { get; }
        string[] Args { get; }
        int CountArgsCommand { get; }
        RepUsers.StatusUser StatusUser { get; set; }
        BotLevelCommand.LevelCommand NameOfLevel { get; }
        Func<ITCommand, PatternUser, Message, Task> Execute { get; }
        Func<TextMessage, PatternUser, Message, Task> OnError { get; }
    }
}