using System;

namespace TBotHamsti.Models.Commands
{
    /// <summary>
    /// Using to create a new <see cref="BotLevelCommand"/> and when adding a new <see cref="BotCommand"/> to it
    /// </summary>
    /// <remarks>
    /// Create a new <see cref="BotLevelCommand"/>: <see cref="BotLevelCommand(LevelCommand)"/><br/>
    /// <para>
    /// Creating new <see cref="BotCommand"/> and marking with flags and it using like:
    /// <code>NameOfLevel = LevelCommand.Bot</code> or for several:<code>NameOfLevel = LevelCommand.Root | LevelCommand.Users</code></para>
    /// Adding a <see cref="ICommand"/> only one <see cref="BotLevelCommand"/>: <see cref="BotLevelCommand.AppendOnlyToThisLevel(ICommand)"/><br/>
    /// Adding a <see cref="ICommand"/> several <see cref="BotLevelCommand"/>: <see cref="BotLevelCommand.AppendToSomeLevels(ICommand)"/>
    /// </remarks>
    [Flags]
    public enum LevelCommand
    {
        /// <summary>
        /// Hide command from any levels
        /// </summary>
        None = 0,

        /// <summary>
        /// Default <see cref="BotLevelCommand"/>, adding automatically
        /// </summary>
        Root = 1,

        /// <summary>
        /// For sending <see cref="Telegram.Bot.Types.Message"/> somebody by bot
        /// </summary>
        Messages = 1 << 1,

        /// <summary>
        /// For <see cref="Users.User"/> management
        /// </summary>
        Users = 1 << 2,

        /// <summary>
        /// To control the runtime bot
        /// </summary>
        Bot = 1 << 3,

        /// <summary>
        /// To execute commands for the computer
        /// </summary>
        PC = 1 << 4,

        /// <summary>
        /// A command to all levels
        /// </summary>
        All = ~(~0 << 5)
    }
}