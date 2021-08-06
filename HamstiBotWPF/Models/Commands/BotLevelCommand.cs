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
        /// <summary>
        /// All added <see cref="BotLevelCommand"/>
        /// </summary>
        private static readonly List<BotLevelCommand> allAddedLevels;
        
        /// <inheritdoc cref="CommandsOfLevel"/>
        private readonly List<ICommand> commandsOfLevel;

        /// <summary>
        /// Generating a default error message during changing <see cref="User.CurrentLevel"/>
        /// </summary>
        private static string DefaultErrorMessage => "An error occurred while changing the command level. To get the list of commands: " + CollectionCommands.HelpCommand.ExampleCommand;

        /// <summary>
        /// <inheritdoc cref="UPLevel"/>, adding automatically
        /// </summary>
        /// <remarks>
        /// Using the <see cref="RootLevel"/> as starting level for any <see cref="ICommand"/>
        /// </remarks>
        public static BotLevelCommand RootLevel { get; }

        /// <summary>
        /// Default <see cref="BotLevelCommand"/>
        /// </summary>
        /// <remarks>
        /// <see cref="BotLevelCommand"/> for return on the <see cref="BotLevelCommand.ParrentLevel"/><br/>
        /// Adding by <see cref="AppendToSomeLevels(ICommand)"/> to <see cref="allAddedLevels"/>, exclude <see cref="RootLevel"/>
        /// </remarks>
        public static BotLevelCommand UPLevel { get; }
        public string Command { get; private set; }
        public string ExampleCommand => Command.ToUpper();
        public string[] Args => Array.Empty<string>();
        public int CountArgsCommand => default;
        public StatusUser StatusUser { get; set; } = StatusUser.User;
        public LevelCommand NameOfLevel { get; private set; }

        /// <summary>
        /// Each added <see cref="ICommand"/> per <see cref="BotLevelCommand"/>
        /// </summary>
        /// <value>
        /// <see cref="ICommand"/> at the current <see cref="BotLevelCommand"/>
        /// </value>
        public IList<ICommand> CommandsOfLevel => commandsOfLevel.AsReadOnly(); //children

        /// <summary>
        /// Contains the previous <see cref="BotLevelCommand"/> to return by <see cref="UPLevel"/> command
        /// </summary>
        /// <value>
        /// previous level (parent level)
        /// </value>
        public BotLevelCommand ParrentLevel { get; set; } = null; //parrent
        public Func<ICommand, Users.User, Message, Task> Execute { get; private set; }
        public Func<TextMessage, Users.User, Message, Task> OnError { get; private set; } = async (messageError, user, message)
            => await user.SendMessageAsync(messageError?.Text ?? DefaultErrorMessage);

        /// <summary>
        /// Implements default and return levels
        /// </summary>
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

        /// <summary>
        /// Adding a new command level: /<paramref name="nameOfLevel"/>
        /// </summary>
        /// <param name="nameOfLevel">Name of the added <see cref="BotLevelCommand"/></param>
        public BotLevelCommand(LevelCommand nameOfLevel)
        {
            NameOfLevel = nameOfLevel;
            commandsOfLevel = new List<ICommand>();
            Command = "/" + nameOfLevel.ToString().ToLower();
            Execute = ChangeLevelAsync;
        }

        /// <summary>
        /// Adding a <paramref name="tCommand"/> only for the current <see cref="BotLevelCommand"/>
        /// </summary>
        /// <param name="tCommand">Added <paramref name="tCommand"/> by <see cref="ICommand.NameOfLevel"/></param>
        /// <exception cref="ArgumentException">If <see cref="BotLevelCommand"/> added already<para/>
        /// or<para/>
        /// <paramref name="tCommand"/> has set more than 1 <see cref="BotLevelCommand"/> (use <see cref="AppendToSomeLevels(ICommand)"/> instead or change <see cref="ICommand.NameOfLevel"/>)
        /// </exception>
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

        /// <summary>
        /// Adding a <paramref name="tCommand"/> for multiple <see cref="BotLevelCommand"/>
        /// </summary>
        /// <exception cref="ArgumentException">
        /// If try to add <see cref="BotLevelCommand"/> (excluding the <see cref="UPLevel"/>) - use <see cref="AppendOnlyToThisLevel(ICommand)"/> instead<para/>
        /// or<para/>
        /// if the <paramref name="tCommand"/> isn't added to the required <see cref="BotLevelCommand"/> (<see cref="ICommand.NameOfLevel"/>)
        /// </exception>
        /// <inheritdoc cref="AppendOnlyToThisLevel(ICommand)"/>
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
                if (tCommand.NameOfLevel.HasFlag(botLevel.NameOfLevel))
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


        /// <summary>
        /// Getting a <see cref="BotLevelCommand"/> from a <paramref name="user"/>
        /// </summary>
        /// <param name="user"><paramref name="user"/> to search</param>
        /// <returns>Current <see cref="BotLevelCommand"/> of the <paramref name="user"/></returns>
        /// <exception cref="ArgumentException">If <paramref name="user"/> <see cref="BotLevelCommand"/> doesn't exist (it don't added to <see cref="allAddedLevels"/>)</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="user"/> is null</exception>
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

        /// <summary>
        /// Foreach of <see cref="allAddedLevels"/> and sorting of their <see cref="CommandsOfLevel"/>
        /// </summary>
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

        /// <summary>
        /// Changing the <paramref name="user"/> <see cref="BotLevelCommand"/> to the current <see cref="BotLevelCommand"/>
        /// </summary>
        /// <inheritdoc cref="CommandExecutors.ExUsers.DeauthUser(ICommand, User, Message)"/>
        /// <exception cref="ArgumentException">If the <paramref name="user"/> is on the wrong level to change it</exception>
        private Task ChangeLevelAsync(ICommand model, User user, Message message)
        {
            if (!user.CurrentLevel.HasFlag(ParrentLevel.NameOfLevel))
            {
                throw new ArgumentException(DefaultErrorMessage, nameof(user.CurrentLevel));
            }

            user.CurrentLevel = NameOfLevel;
            return user.SendMessageAsync(MessageWhenLevelChanges(user));
        }

        /// <summary>
        /// Checking for added <paramref name="tCommand"/> per <paramref name="botLevel"/>
        /// </summary>
        /// <param name="botLevel"><paramref name="botLevel"/> to check</param>
        /// <param name="tCommand"><paramref name="tCommand"/> to check</param>
        /// <exception cref="ArgumentException">If <paramref name="botLevel"/> contains <paramref name="tCommand"/> already</exception>
        private static void CheckContainsCommand(BotLevelCommand botLevel, ICommand tCommand)
        {
            if (botLevel.commandsOfLevel.Any(p => p.Command.Equals(tCommand.Command) && p.CountArgsCommand.Equals(tCommand.CountArgsCommand)))
            {
                throw new ArgumentException($"Level {botLevel.NameOfLevel} contains {tCommand.ExampleCommand} already", nameof(commandsOfLevel));
            }
        }

        /// <summary>
        /// Generating a default error message during changing a <see cref="BotLevelCommand"/> of the <paramref name="user"/>
        /// </summary>
        /// <param name="user">Who changing his <see cref="BotLevelCommand"/></param>
        /// <returns>An error message</returns>
        private static string MessageWhenLevelChanges(User user) => "Current level: " + user.CurrentLevel + "\n\nList of commands:\n" + ExCommon.GetHelp(user);
    }
}
