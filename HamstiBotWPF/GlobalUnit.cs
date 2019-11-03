using System.Collections.Generic;
using Telegram.Bot;

namespace HamstiBotWPF 
{
    /// <summary>
    /// Global variables for the entire project
    /// </summary>
    public static class GlobalUnit 
    {
        /// <summary>
        /// Creating a bot and working with it
        /// </summary>
        public static readonly TelegramBotClient Api = new TelegramBotClient(Properties.Settings.Default.ApiBot);
        
        /// <summary>
        /// List of all authorized users
        /// </summary>
        public static List<Core.patternUserList> authUsers = new List<Core.patternUserList>();

        /// <summary>
        /// List of all commands for working with the bot
        /// </summary>
        public static List<Core.BotCommand> botCommands = new List<Core.BotCommand>();

        /// <summary>
        /// Current level for commands
        /// </summary>
        public static Core.BotLevelCommand.LevelCommand currentLevelCommand = Core.BotLevelCommand.LevelCommand.Root;

    }
}
