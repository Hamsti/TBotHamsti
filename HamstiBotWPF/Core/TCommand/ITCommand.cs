using System;
using TBotHamsti.LogicRepository;
using Telegram.Bot.Types;

namespace TBotHamsti.Core
{
    public interface ITCommand
    {
        int CountArgsCommand { get; }
        string Command { get; }
        string[] Args { get; }
        string ExampleCommand { get; }
        bool LevelDependent { get; set; } //delete after
        RepUsers.StatusUser StatusUser { get; set; }
        public BotLevelCommand.LevelCommand ParrentLevel { get; set;} //parrent
        BotLevelCommand.LevelCommand NameOfLevel { get; }
        Action<ITCommand, PatternUser, Message> OnError { get; }
        Action<ITCommand, PatternUser, Message> Execute { get; }
    }
}