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
        public static class myBot
        {
            public static readonly TelegramBotClient Api = new TelegramBotClient("533190991:AAGHPQhmjcy-JEaZEnRYUhERtX16rM9xUQM");
        }
        /// <summary>
        /// List of all authorized users
        /// </summary>
        public static List<Core.patternUserList> authUsers = new List<Core.patternUserList>();

        /// <summary>
        /// List of all commands for working with the bot
        /// </summary>
        public static List<Core.BotCommand> botCommands = new List<Core.BotCommand>();
    }
}
