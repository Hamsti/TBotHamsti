using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using HamstiBotWPF.LogicRepository;

namespace HamstiBotWPF.Core
{
    public class BotLevelCommand : BotCommand
    {
        /// <summary>
        /// Command for change to up level
        /// </summary>
        public const string TOPREVLEVEL = "/up";

        public enum LevelCommand
        {
            Root,
            Messages,
            ControlUsers,
            ControlBot,
            ControlPC
        }

        /// <summary>
        /// Hides the original property. Set only 0.
        /// </summary>
        public new int CountArgsCommand => base.CountArgsCommand;
        public new string ExampleCommand => Command.ToUpper();

        /// <summary>
        /// Previos (parrent) level for commands
        /// </summary>
        public LevelCommand ParrentLevel { get; private set; }

        public BotLevelCommand(LevelCommand nameOfLevel, LevelCommand parrentLevel = LevelCommand.Root)
        {
            Execute += async (BotCommandStructure command, Message message) => await ExecLevelUp(message);
            NameOfLevel = nameOfLevel;
            ParrentLevel = parrentLevel;
            Command = "/" + nameOfLevel.ToString();
            base.CountArgsCommand = 0;
        }

        /// <summary>
        /// Processing commands for change current level
        /// </summary>
        public new Action<BotCommandStructure, Message> Execute { get; } = async (model, message) =>
        {
            //first execute fuction "ExecLevelUp" (added in initialization BotLevelCommand), for next exec constraction typed down.
            if (await WhenLevelIsRoot(message)) return;

            //Change currentLevel on LevelCommand
            foreach (LevelCommand level in Enum.GetValues(typeof(LevelCommand)))
            {
                if (("/" + level.ToString().ToLower()).Equals(model.Command))
                {
                    RepCommands.currentLevelCommand = level;
                    await SendMessageWhenLevelChanges(message);
                }
            }
        };

        /// <summary>
        /// Execute if have error when change level commands
        /// </summary>
        public new Action<BotCommandStructure, Message> OnError { get; } = async (model, message) => await RepUsers.SendMessage(message.From.Id, "Произошла ошибка при изменении уровня команд.\nСписок комманд: /help");

        private static async Task SendMessageWhenLevelChanges(Message message)
        {
            string messageWhenLevelChanges = "Current level: " + RepCommands.currentLevelCommand + "\n\nList of commands:\n";

            await RepUsers.SendMessage(message.From.Id, messageWhenLevelChanges + RepBotActions.GetHelp(message.From.Id));
        }

        private static async Task<bool> WhenLevelIsRoot(Message message)
        {
            if (message.Text.ToLower() == TOPREVLEVEL && RepCommands.currentLevelCommand == LevelCommand.Root)
            {
                await RepUsers.SendMessage(message.From.Id, "Вы находитесь на начальном уровне.");
                return true;
            }
            return false;
        }

        private async Task ExecLevelUp(Message message)
        {
            if (message.Text.ToLower() == TOPREVLEVEL && RepCommands.currentLevelCommand > LevelCommand.Root)
            {
                RepCommands.currentLevelCommand = ParrentLevel;
                await SendMessageWhenLevelChanges(message);
            }
        }
    }
}
