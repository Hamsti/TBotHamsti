using System;
using System.Threading.Tasks;
using TBotHamsti.Models.Messages;
using TBotHamsti.Models.Users;
using Telegram.Bot.Types;
using User = TBotHamsti.Models.Users.User;

namespace TBotHamsti.Models.Commands
{
    public interface ICommand
    {
        string Command { get; }
        string ExampleCommand { get; }
        string[] Args { get; }
        int CountArgsCommand { get; }
        StatusUser StatusUser { get; set; }
        LevelCommand NameOfLevel { get; }
        Func<ICommand, User, Message, Task> Execute { get; }
        Func<TextMessage, User, Message, Task> OnError { get; }
    }
}