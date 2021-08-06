using System;
using System.Threading.Tasks;
using TBotHamsti.Models.Messages;
using TBotHamsti.Models.Users;
using Telegram.Bot.Types;

namespace TBotHamsti.Models.Commands
{
    /// <summary>
    /// A bot commands and a user messages parsed by <see cref="BotCommand.ParseMessage(Message)"/>
    /// </summary>
    public interface ICommand
    {
        /// <value>
        /// <see cref="Command"/> of the form: /command
        /// </value>
        string Command { get; }

        /// <summary>
        /// Contains an example command, for example: for help
        /// </summary>
        /// <value>
        /// <see cref="Command"/> [<see cref="Args"/>[0]] [<see cref="Args"/>[1]]...
        /// </value>
        string ExampleCommand { get; }

        /// <value>
        /// Arguments of command
        /// </value>
        string[] Args { get; }

        /// <value>
        /// Count of arguments
        /// </value>
        int CountArgsCommand { get; }

        /// <value>
        /// Required <see cref="Users.StatusUser"/> for <see cref="ICommand"/> execution
        /// </value>
        StatusUser StatusUser { get; set; }

        /// <summary>
        /// Name of <see cref="BotLevelCommand"/> added with <see cref="BotLevelCommand(LevelCommand)"/> <para/>
        /// On which some <see cref="BotLevelCommand"/> adding a <see cref="BotCommand"/> with
        /// <see cref="BotLevelCommand.AppendOnlyToThisLevel(ICommand)"/> and <see cref="BotLevelCommand.AppendToSomeLevels(ICommand)"/>
        /// </summary>
        LevelCommand NameOfLevel { get; }

        /// <summary>
        /// Delegate to add and execute the command
        /// </summary>
        Func<ICommand, Users.User, Message, Task> Execute { get; }

        /// <summary>
        /// Delegate to add and execute the error
        /// </summary>
        Func<TextMessage, Users.User, Message, Task> OnError { get; }
    }
}