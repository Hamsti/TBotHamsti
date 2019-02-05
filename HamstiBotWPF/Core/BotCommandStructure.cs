namespace HamstiBotWPF.Core
{
    public class BotCommandStructure
    {
        /// <summary>
        /// Command to execute by bot
        /// </summary>
        public string Command { get; set; }
        /// <summary>
        /// Command argument list
        /// </summary>
        public string[] Args { get; set; }
    }
}
