using System;
using TBotHamsti.Models.CommandExecutors;
using TBotHamsti.Models.Commands;
using TBotHamsti.Models.Users;

namespace TBotHamsti.Models
{
    /// <summary>
    /// To work with a list of commands
    /// </summary>
    public static class CollectionCommands
    {
        private static BotLevelCommand messagesLevel;
        private static BotLevelCommand usersLevel;
        private static BotLevelCommand pcLevel;
        private static BotLevelCommand botLevel;
        /// <summary>
        /// List of all commands for working with the bot
        /// </summary>
        public static BotLevelCommand RootLevel => BotLevelCommand.RootLevel;
        public static BotCommand HelpCommand { get; private set; }
        public static BotCommand SendMessageToAdminCommand { get; private set; }

        /// <summary>
        /// Add all commands to the command list
        /// </summary>
        public static void Init()
        {
            InitMessageLevel();
            InitUsersLevel();
            InitPCLevel();
            InitBotLevel();

            RootLevel.AppendOnlyToThisLevel(usersLevel);
            RootLevel.AppendOnlyToThisLevel(messagesLevel);
            RootLevel.AppendOnlyToThisLevel(pcLevel);
            RootLevel.AppendOnlyToThisLevel(botLevel);
            BotLevelCommand.AppendToSomeLevels(BotLevelCommand.UPLevel);

            HelpCommand = new BotCommand("/help")
            {
                NameOfLevel = LevelCommand.All,
                StatusUser = StatusUser.None,
                Execute = (model, user, message) => ExCommon.HelpBot(user)
            };
            BotLevelCommand.AppendToSomeLevels(HelpCommand);

            BotLevelCommand.AppendToSomeLevels(new BotCommand("/start")
            {
                NameOfLevel = LevelCommand.Root | LevelCommand.Users | LevelCommand.Messages,
                StatusUser = StatusUser.None,
                Execute = (model, user, message) => ExUsers.StartCommandUser(message)
            });

            BotLevelCommand.AppendToSomeLevels(new BotCommand("/keyboard", string.Join("; ", bool.TrueString, bool.FalseString, ExCommon.ARG_FULL_KEYBOARD))
            {
                NameOfLevel = LevelCommand.All,
                Execute = (model, user, message) => ExCommon.ShowScreenButtons(user, model.GetArg(0))
            });

            SendMessageToAdminCommand = new BotCommand("/writeToAdmin", "Message text", false)
            {
                NameOfLevel = LevelCommand.All,
                StatusUser = StatusUser.None,
                Execute = ExMessages.SentToAdmins
            };
            BotLevelCommand.AppendToSomeLevels(SendMessageToAdminCommand);

            BotLevelCommand.AppendToSomeLevels(new BotCommand("/listUsers")
            {
                NameOfLevel = LevelCommand.Users | LevelCommand.Messages,
                StatusUser = StatusUser.Admin,
                Execute = (model, user, message) => ExUsers.SendListOfUsers(user)
            });

            BotLevelCommand.SortCommandsOfAllLevels();
        }

        private static void InitBotLevel()
        {
            botLevel = new BotLevelCommand(LevelCommand.Bot) { StatusUser = StatusUser.Admin };
            botLevel.AppendOnlyToThisLevel(new BotCommand("/stopBot")
            {
                StatusUser = StatusUser.Admin,
                Execute = (model, user, message) => ExecutionBot.StopBotAsync()
            });

            botLevel.AppendOnlyToThisLevel(new BotCommand("/stopApp")
            {
                StatusUser = StatusUser.Admin,
                Execute = (model, user, message) => ExCommon.ComStopApp(user)
            });

            botLevel.AppendOnlyToThisLevel(new BotCommand("/reloadBot")
            {
                StatusUser = StatusUser.Admin,
                Execute = (model, user, message) => ExecutionBot.RestartBotAsync()
            });
        }

        private static void InitPCLevel()
        {
            pcLevel = new BotLevelCommand(LevelCommand.PC);
            pcLevel.AppendOnlyToThisLevel(new BotCommand("/url", "url")
            {
                Execute = ExPC.ExecuteUrl
            });

            pcLevel.AppendOnlyToThisLevel(new BotCommand("/turnOff", new string[] { "tMin: int", "tSec: int" })
            {
                StatusUser = StatusUser.Admin,
                Execute = (model, user, message) => ExPC.TurnOff(user, ExUsers.IdStrToInt(model.GetArg(0)), ExUsers.IdStrToInt(model.GetArg(1)))
            });

            pcLevel.AppendOnlyToThisLevel(new BotCommand("/cancelOff")
            {
                StatusUser = StatusUser.Admin,
                Execute = (model, user, message) =>
                {
                    ExPC.ExecuteCmdCommand(@"C:\Windows\System32\shutdown.exe", "/a");
                    return user.SendMessageAsync("The shutdown timer has been successfully removed");
                }
            });

            pcLevel.AppendOnlyToThisLevel(new BotCommand("/lock")
            {
                Execute = (model, user, message) =>
                {
                    ExPC.ExecuteCmdCommand(@"C:\Windows\System32\rundll32.exe", "USER32.DLL LockWorkStation");
                    return user.SendMessageAsync("System locked successfully");
                }
            });

            pcLevel.AppendOnlyToThisLevel(new BotCommand("/screen")
            {
                Execute = (model, user, message) => ExPC.GetScreenshot(user)
            });

            pcLevel.AppendOnlyToThisLevel(new BotCommand("/hibernate")
            {
                Execute = (model, user, message) =>
                {
                    ExPC.ExecuteCmdCommand(@"C:\Windows\System32\shutdown.exe", "/h");
                    return user.SendMessageAsync("Hibernation completed successfully");
                }
            });

            pcLevel.AppendOnlyToThisLevel(new BotCommand("/volume", "[-100..100]; mute")
            {
                Execute = (model, user, message) => ExPC.ContolVolume.ChangeVolume(user, model.GetArg(0))
            });
        }

        private static void InitUsersLevel()
        {
            usersLevel = new BotLevelCommand(LevelCommand.Users) { StatusUser = StatusUser.Admin };
            usersLevel.AppendOnlyToThisLevel(new BotCommand("/add", "Id user")
            {
                StatusUser = StatusUser.Admin,
                Execute = ExUsers.AuthNewUser
            });

            usersLevel.AppendOnlyToThisLevel(new BotCommand("/add", new string[] { "Id user", "Nickname" }, false)
            {
                StatusUser = StatusUser.Admin,
                Execute = ExUsers.AuthNewUser
            });

            usersLevel.AppendOnlyToThisLevel(new BotCommand("/lock", "Id user")
            {
                StatusUser = StatusUser.Admin,
                Execute = ExUsers.LockUser
            });

            usersLevel.AppendOnlyToThisLevel(new BotCommand("/lock", "Nickname", false)
            {
                StatusUser = StatusUser.Admin,
                Execute = ExUsers.LockUser
            });

            usersLevel.AppendOnlyToThisLevel(new BotCommand("/deauth", "Id user")
            {
                StatusUser = StatusUser.Admin,
                Execute = ExUsers.DeauthUser
            });

            usersLevel.AppendOnlyToThisLevel(new BotCommand("/deauth", "Nickname", false)
            {
                StatusUser = StatusUser.Admin,
                Execute = ExUsers.DeauthUser
            });

            usersLevel.AppendOnlyToThisLevel(new BotCommand("/nickname", new string[] { "Id user", "Nickname" }, false)
            {
                StatusUser = StatusUser.Moder,
                Execute = ExUsers.ChangeLocalName
            });

            usersLevel.AppendOnlyToThisLevel(new BotCommand("/save")
            {
                StatusUser = StatusUser.Admin,
                Execute = (model, user, message) => ExUsers.SaveChanges(user)
            });

            usersLevel.AppendOnlyToThisLevel(new BotCommand("/cancel")
            {
                StatusUser = StatusUser.Admin,
                Execute = (model, user, message) => ExUsers.CancelChanges(user)
            });
        }

        private static void InitMessageLevel()
        {
            messagesLevel = new BotLevelCommand(LevelCommand.Messages);
            messagesLevel.AppendOnlyToThisLevel(new BotCommand("/byId", new string[] { "Id user", "Message text" }, false)
            {
                StatusUser = StatusUser.Moder,
                Execute = ExMessages.SentById
            });

            messagesLevel.AppendOnlyToThisLevel(new BotCommand("/byStatus", new string[] { string.Join("; ", Enum.GetNames(typeof(StatusUser))), "Message text" }, false)
            {
                StatusUser = StatusUser.Moder,
                Execute = ExMessages.SentByStatus
            });

            messagesLevel.AppendOnlyToThisLevel(new BotCommand("/spam", new string[] { "Id user", "Count of messages" })
            {
                StatusUser = StatusUser.Admin,
                Execute = (model, user, message) => ExMessages.UserSpam(model, user)
            });
        }
    }
}
