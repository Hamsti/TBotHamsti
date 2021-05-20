using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using TBotHamsti.LogicRepository;
using System.Collections.Generic;
using static TBotHamsti.LogicRepository.RepUsers;

namespace TBotHamsti.Core
{
    public class BotLevelCommand : ITCommand
    {
        /// <summary>
        /// Command for change to up level
        /// </summary>
        public const string TOPREVLEVEL = "/up";

        [Flags]
        public enum LevelCommand
        {
            None = 0,
            Messages = 1,
            Users = 1 << 2,
            Bot = 1 << 3,
            PC = 1 << 4,
            All = ~(~0 << 4)
        }


        public LevelCommand NameOfLevel { get; } //id
        public List<ITCommand> CommandsOfLevel { get; private set; } //children
        public LevelCommand ParrentLevel { get; set; } //parrent - BotLevelCommand

        public BotLevelCommand(LevelCommand nameOfLevel)
        {
            NameOfLevel = nameOfLevel;
            Execute += async (ITCommand command, PatternUser user, Message message) => await ExecLevelUp(user, message);
            Command = (nameOfLevel.Equals(LevelCommand.None) ? TOPREVLEVEL : "/" + nameOfLevel.ToString()).ToLower();
        }

        //public void AppendCommand(ITCommand tCommand)
        //{
        //    if (CommandsOfLevel is null)
        //    {
        //        CommandsOfLevel = new List<ITCommand>();
        //    }

        //    if (NameOfLevel.HasFlag(tCommand.NameOfLevel) && !CommandsOfLevel.Contains(tCommand))
        //    {
        //        CommandsOfLevel.Add(tCommand);
        //        tCommand.ParrentLevel = this;
        //    }
        //}

        //public List<ITCommand> GetAllCommands()
        //{
        //    List<ITCommand> commands = CommandsOfLevel;
        //    foreach (var child in commands)
        //    {
        //        if (child is BotLevelCommand parrent)
        //        {
        //            commands.AddRange(parrent.GetAllCommands());
        //        }
        //    }

        //    return commands;
        //}





















        public bool LevelDependent { get; set; }



        /// <summary>
        /// Previos (parrent) level for commands
        /// </summary>
        public string Command { get; }
        public string[] Args => Array.Empty<string>();
        public string ExampleCommand => Command.ToUpper();
        public int CountArgsCommand => 0;

        public StatusUser StatusUser { get; set; } = StatusUser.User;


        /// <summary>
        /// Processing commands for change current level
        /// </summary>
        public Action<ITCommand, PatternUser, Message> Execute { get; } = async (model, user, message) =>
        {
            //first execute fuction "ExecLevelUp" (added in initialization BotLevelCommand), for next exec constraction typed down.
            if (await WhenLevelIsRoot(user, message)) return;

            //Change currentLevel on LevelCommand
            foreach (LevelCommand level in Enum.GetValues(typeof(LevelCommand)))
            {
                if (("/" + level.ToString().ToLower()).Equals(model.Command))
                {
                    user.CurrentLevel = level;
                    await SendMessageWhenLevelChanges(user);
                }
            }
        };

        /// <summary>
        /// Execute if have error when change level commands
        /// </summary>
        public Action<ITCommand, PatternUser, Message> OnError { get; } = async (model, user, message) => await SendMessage(user.Id, "Произошла ошибка при изменении уровня команд.\nСписок комманд: /help");

        private static async Task SendMessageWhenLevelChanges(PatternUser user)
        {
            string messageWhenLevelChanges = "Current level: " + user.CurrentLevel + "\n\nList of commands:\n";

            await SendMessage(user.Id, messageWhenLevelChanges + RepBotActions.GetHelp(user));
        }

        private static async Task<bool> WhenLevelIsRoot(PatternUser user, Message message)
        {
            if (message.Text.ToLower() == TOPREVLEVEL && user.CurrentLevel == LevelCommand.None)
            {
                await SendMessage(user.Id, "Вы находитесь на начальном уровне.");
                return true;
            }
            return false;
        }

        private async Task ExecLevelUp(PatternUser user, Message message)
        {
            if (message.Text.ToLower() == TOPREVLEVEL && user.CurrentLevel > LevelCommand.None)
            {
                user.CurrentLevel = ParrentLevel;
                await SendMessageWhenLevelChanges(user);
            }
        }
    }
}
