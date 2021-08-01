using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TBotHamsti.Models.CommandExecutors;
using TBotHamsti.Models.Messages;
using TBotHamsti.Models.Users;
using Telegram.Bot.Types;
using User = TBotHamsti.Models.Users.User;

namespace TBotHamsti.Models.Commands
{
    public class BotLevelCommand : ICommand
    {
        private static readonly List<BotLevelCommand> allAddedLevels;
        private readonly List<ICommand> commandsOfLevel;
        private static string DefaultErrorMessage => "An error occurred while changing the command level. To get the list of commands: " + CollectionCommands.HelpCommand.ExampleCommand;

        public static BotLevelCommand RootLevel { get; }
        public static BotLevelCommand UPLevel { get; }

        public string Command { get; private set; }
        public string ExampleCommand => Command.ToUpper();
        public string[] Args => Array.Empty<string>();
        public int CountArgsCommand => default;
        public StatusUser StatusUser { get; set; } = StatusUser.User;
        public LevelCommand NameOfLevel { get; private set; } //id
        public IList<ICommand> CommandsOfLevel => commandsOfLevel.AsReadOnly(); //children
        public BotLevelCommand ParrentLevel { get; set; } = null; //parrent
        public Func<ICommand, User, Message, Task> Execute { get; private set; }
        public Func<TextMessage, User, Message, Task> OnError { get; private set; } = async (messageError, user, message)
            => await user.SendMessageAsync(messageError?.Text ?? DefaultErrorMessage);

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
                    user.CurrentLevel = GetBotLevelCommand(user).ParrentLevel.NameOfLevel;
                    await user.SendMessageAsync(MessageWhenLevelChanges(user));
                },
                OnError = async (messageError, user, message) => await user.SendMessageAsync("You're at the beginner level - " + RootLevel.NameOfLevel)
            };
        }

        public BotLevelCommand(LevelCommand nameOfLevel)
        {
            NameOfLevel = nameOfLevel;
            commandsOfLevel = new List<ICommand>();
            Command = "/" + nameOfLevel.ToString().ToLower();
            Execute = ChangeLevelAsync;
        }

        public void AppendOnlyToThisLevel(ICommand tCommand)
        {
            static bool isSetMoreThanOneBit(int number) => (number != 0) && ((number & (number - 1)) != 0);
            CheckContainsCommand(this, tCommand);

            switch (tCommand)
            {
                case BotCommand botCommand: botCommand.NameOfLevel |= NameOfLevel; break;
                case BotLevelCommand botLevel:
                    botLevel.ParrentLevel = !allAddedLevels.Contains(botLevel) ? this : throw new ArgumentException(botLevel.NameOfLevel + " level added already", nameof(allAddedLevels));
                    allAddedLevels.Add(botLevel);
                    break;
            }

            if (isSetMoreThanOneBit((int)tCommand.NameOfLevel))
            {
                throw new ArgumentException($"\"{tCommand.Command}\" has set more than 1 level! Use for setting 2 and more levels \"{nameof(AppendToSomeLevels)}\"", nameof(tCommand.NameOfLevel));
            }

            commandsOfLevel.Add(tCommand);
        }

        public static void AppendToSomeLevels(ICommand tCommand)
        {
            LevelCommand checkCorrectLevelAddition = tCommand.NameOfLevel;
            if (tCommand is BotLevelCommand && tCommand != UPLevel)
            {
                throw new ArgumentException($"It's allowed to add any {nameof(ICommand)}, but only {UPLevel.ExampleCommand} of the {nameof(BotLevelCommand)} type");
            }

            foreach (var botLevel in allAddedLevels)
            {
                CheckContainsCommand(botLevel, tCommand);
                if (tCommand.NameOfLevel.HasFlag(botLevel.NameOfLevel)) // !botLevel.commandsOfLevel.Contains(tCommand))
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

        public static BotLevelCommand GetBotLevelCommand(User user)
        {
            if (user is null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            BotLevelCommand botLevel = allAddedLevels.Find(f => f.NameOfLevel == user.CurrentLevel);
            if (botLevel is null)
            {
                user.CurrentLevel = RootLevel.NameOfLevel;
                throw new ArgumentException($"Unknown user level. Changed on {user.CurrentLevel} level", nameof(user.CurrentLevel));
            }

            return botLevel;
        }

        public static void SortCommandsOfAllLevels()
        {
            foreach (var botLevel in allAddedLevels)
            {
                var sortedCommands = botLevel.commandsOfLevel.OrderBy(o => o is BotCommand ? 1 : -1)
                                                             .ThenBy(t => t.CountArgsCommand == -1 ? 1 : -1)
                                                             .ThenBy(t => t.CountArgsCommand)
                                                             .ThenBy(t => t.ExampleCommand)
                                                             .ToArray();
                for (int i = 0; i < sortedCommands.Length; i++)
                {
                    botLevel.commandsOfLevel[i] = sortedCommands[i];
                }
            }
        }

        private Task ChangeLevelAsync(ICommand model, User user, Message message)
        {
            if (!user.CurrentLevel.HasFlag(ParrentLevel.NameOfLevel))
            {
                throw new ArgumentException(DefaultErrorMessage, nameof(user.CurrentLevel));
            }

            user.CurrentLevel = NameOfLevel;
            return user.SendMessageAsync(MessageWhenLevelChanges(user));
        }

        private static void CheckContainsCommand(BotLevelCommand botLevel, ICommand tCommand)
        {
            if (botLevel.commandsOfLevel.Any(p => p.Command.Equals(tCommand.Command) && p.CountArgsCommand.Equals(tCommand.CountArgsCommand)))
            {
                throw new ArgumentException($"Level {botLevel.NameOfLevel} contains {tCommand.ExampleCommand} already", nameof(commandsOfLevel));
            }
        }

        private static string MessageWhenLevelChanges(User user) => "Current level: " + user.CurrentLevel + "\n\nList of commands:\n" + ExCommon.GetHelp(user);
    }
}
