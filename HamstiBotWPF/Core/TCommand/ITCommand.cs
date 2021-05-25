using System;
using TBotHamsti.LogicRepository;
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
        Action<ITCommand, PatternUser, Message> Execute { get; }
        Action<ITCommand, PatternUser, Message> OnError { get; }
    }
}