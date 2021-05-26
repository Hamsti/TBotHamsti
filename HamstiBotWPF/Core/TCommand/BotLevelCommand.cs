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
        [Flags]
        public enum LevelCommand
        {
            None = 0,
            Root = 1,
            Messages = 1 << 1,
            Users = 1 << 2,
            Bot = 1 << 3,
            PC = 1 << 4,
            All = ~(~0 << 5)
        }

        private static readonly List<BotLevelCommand> allAddedLevels;
        private readonly List<ITCommand> commandsOfLevel;

        public static BotLevelCommand RootLevel { get; }
        public static BotLevelCommand UPLevel { get; }


        public string Command { get; private set; }
        public string ExampleCommand => Command.ToUpper();
        public string[] Args => Array.Empty<string>();
        public int CountArgsCommand => default;
        public StatusUser StatusUser { get; set; } = StatusUser.User;
        public LevelCommand NameOfLevel { get; private set; } //id
        public IList<ITCommand> CommandsOfLevel => commandsOfLevel.AsReadOnly(); //children
        public BotLevelCommand ParrentLevel { get; set; } = null; //parrent

        static BotLevelCommand()
        {
            RootLevel = new BotLevelCommand(LevelCommand.Root);
            allAddedLevels = new List<BotLevelCommand>() { RootLevel };

            UPLevel = new BotLevelCommand(LevelCommand.None)
            {
                Command = "/up",
                NameOfLevel = LevelCommand.All ^ LevelCommand.Root,
                Execute = async (model, user, message) =>
                {
                    if (user.CurrentLevel != RootLevel.NameOfLevel)
                    {
                        user.CurrentLevel = GetBotLevelCommand(user).Result.ParrentLevel.NameOfLevel;
                        await SendMessageWhenLevelChanges(user);
                    }
                    else
                    {
                        await user.SendMessageAsync("You're at the beginner level - " + RootLevel.NameOfLevel);
                    }
                }
            };
        }

        public BotLevelCommand(LevelCommand nameOfLevel)
        {
            NameOfLevel = nameOfLevel;
            commandsOfLevel = new List<ITCommand>();
            Command = "/" + nameOfLevel.ToString().ToLower();
        }

        /// <summary>
        /// Processing commands for change current level
        /// </summary>
        public Func<ITCommand, PatternUser, Message, Task> Execute { get; private set; } = async (model, user, message) =>
        {
            foreach (var level in allAddedLevels)
            {
                if (level.Command == model.Command && user.CurrentLevel.HasFlag(level.ParrentLevel.NameOfLevel))
                {
                    user.CurrentLevel = level.NameOfLevel;
                    await SendMessageWhenLevelChanges(user);
                }
            }
        };

        /// <summary>
        /// Execute if have error when change level commands
        /// </summary>
        public Func<ITCommand, PatternUser, Message, Task> OnError { get; private set; } = async (model, user, message) =>
               await user.SendMessageAsync("An error occurred while changing the command level. To get the list of commands: " + CollectionCommands.HelpCommand.ExampleCommand);

        public void AppendOnlyToThisLevel(ITCommand tCommand)
        {
            static bool isSetMoreThanOneBit(int number) => (number != 0) && ((number & (number - 1)) != 0);

            if (!commandsOfLevel.Contains(tCommand))
            {
                switch (tCommand)
                {
                    case BotCommand botCommand: botCommand.NameOfLevel |= NameOfLevel; break;
                    case BotLevelCommand botLevel:
                        botLevel.ParrentLevel = this;
                        allAddedLevels.Add(botLevel); break;
                }

                if (isSetMoreThanOneBit((int)tCommand.NameOfLevel))
                {
                    throw new ArgumentException($"\"{tCommand.Command}\" has set more than 1 level! Use for setting 2 and more levels \"AppendToSomeLevels\"", nameof(tCommand.NameOfLevel));
                }

                commandsOfLevel.Add(tCommand);
            }
        }

        public void AppendToSomeLevels(ITCommand tCommand)
        {
            LevelCommand checkCorrectLevelAddition = tCommand.NameOfLevel;

            foreach (var botLevel in allAddedLevels)
            {
                if (tCommand.NameOfLevel.HasFlag(botLevel.NameOfLevel) && !botLevel.commandsOfLevel.Contains(tCommand))
                {
                    botLevel.commandsOfLevel.Add(tCommand);
                    checkCorrectLevelAddition ^= botLevel.NameOfLevel;
                }
            }

            if (checkCorrectLevelAddition != LevelCommand.None)
            {
                throw new ArgumentException($"Command \"{tCommand.Command}\" has the levels \"{tCommand.NameOfLevel}\" and don't set \"{checkCorrectLevelAddition}\"", nameof(checkCorrectLevelAddition));
            }
        }

        public static async Task<BotLevelCommand> GetBotLevelCommand(PatternUser user)
        {
            BotLevelCommand botLevel = allAddedLevels.Find(f => f.NameOfLevel == user.CurrentLevel);

            if (botLevel is null)
            {
                botLevel = RootLevel;
                user.CurrentLevel = botLevel.NameOfLevel;
                await user.SendMessageAsync($"Unknown user level. Changed on {botLevel.NameOfLevel} level");
            }

            if (botLevel.ParrentLevel != null && !botLevel.CommandsOfLevel.Contains(UPLevel))
            {
                throw new ArgumentException($"Level \"{botLevel.NameOfLevel}\" doesn't contain of {nameof(BotLevelCommand)} for return to prevent level", nameof(UPLevel));
            }

            return botLevel;
        }


        private static async Task SendMessageWhenLevelChanges(PatternUser user)
        {
            string messageWhenLevelChanges = "Current level: " + user.CurrentLevel + "\n\nList of commands:\n";

            await user.SendMessageAsync(messageWhenLevelChanges + RepBotActions.GetHelp(user));
        }
    }
}
