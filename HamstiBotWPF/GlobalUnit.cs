using System.Collections.Generic;
using Telegram.Bot;
using HamstiBotWPF.Core;
using System.Collections.ObjectModel;

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
        public static TelegramBotClient Api { get; } = new TelegramBotClient(Properties.Settings.Default.ApiBot);
        
        /// <summary>
        /// List of all authorized users
        /// </summary>
        public static ObservableCollection<PatternUser> authUsers = new ObservableCollection<PatternUser>();

        /// <summary>
        /// List of all commands for working with the bot
        /// </summary>
        public static List<BotCommand> botCommands = new List<BotCommand>();

        /// <summary>
        /// Current level for commands
        /// </summary>
        public static BotLevelCommand.LevelCommand currentLevelCommand = BotLevelCommand.LevelCommand.Root;
    }
}
