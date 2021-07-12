﻿using System.Linq;
using TBotHamsti.Core;
using LevelCommand = TBotHamsti.Core.BotLevelCommand.LevelCommand;
using StatusUser = TBotHamsti.LogicRepository.RepUsers.StatusUser;

namespace TBotHamsti.LogicRepository
{
    /// <summary>
    /// To work with a list of commands
    /// </summary>
    public static class CollectionCommands
    {
        /// <summary>
        /// List of all commands for working with the bot
        /// </summary>
        public static BotLevelCommand RootLevel => BotLevelCommand.RootLevel;
        public static BotCommand HelpCommand { get; private set; }
        public static BotCommand SendMessageToAdminCommand { get; private set; }

        //private static void Sort() => Values = new List<BotCommand>(Values.OrderBy(o => o is BotLevelCommand ? -1 : 1).ThenBy(t => t.NameOfLevel).ThenBy(s => s.StatusUser).ThenBy(t => t.CountArgsCommand).ThenBy(t => t.Command));
        private static void Sort() { }


        /// <summary>
        /// Add all commands to the command list
        /// </summary>
        public static void Init()
        {
            BotLevelCommand MessagesLevel = new BotLevelCommand(LevelCommand.Messages);
            BotLevelCommand UsersLevel = new BotLevelCommand(LevelCommand.Users) { StatusUser = StatusUser.Admin };
            BotLevelCommand PCLevel = new BotLevelCommand(LevelCommand.PC);
            BotLevelCommand BotLevel = new BotLevelCommand(LevelCommand.Bot) { StatusUser = StatusUser.Admin };

            RootLevel.AppendOnlyToThisLevel(UsersLevel);
            UsersLevel.AppendOnlyToThisLevel(MessagesLevel);
            RootLevel.AppendOnlyToThisLevel(PCLevel);
            RootLevel.AppendOnlyToThisLevel(BotLevel);

            BotLevelCommand.AppendToSomeLevels(BotLevelCommand.UPLevel);


            HelpCommand = new BotCommand("/help")
            {
                NameOfLevel = LevelCommand.All,
                StatusUser = StatusUser.None,
                Execute = async (model, user, message) => await RepBotActions.HelpBot(user)
            };
            BotLevelCommand.AppendToSomeLevels(HelpCommand);

            BotLevelCommand.AppendToSomeLevels(new BotCommand("/start")
            {
                NameOfLevel = LevelCommand.Root | LevelCommand.Users | LevelCommand.Messages,
                StatusUser = StatusUser.None,
                Execute = async (model, user, message) => await RepBotActions.ControlUsers.StartCommandUser(message)
            });

            BotLevelCommand.AppendToSomeLevels(new BotCommand("/keyboard", "true; false; all")
            {
                NameOfLevel = LevelCommand.All,
                Execute = async (model, user, message) => await RepBotActions.ShowScreenButtons(user, model.Args.FirstOrDefault())
            });

            SendMessageToAdminCommand = new BotCommand("/writeToAdmin", "Message text", false)
            {
                NameOfLevel = LevelCommand.All,
                StatusUser = StatusUser.None,
                Execute = RepBotActions.Messages.UserSentToAdmin
            };
            BotLevelCommand.AppendToSomeLevels(SendMessageToAdminCommand);

            MessagesLevel.AppendOnlyToThisLevel(new BotCommand("/toUser", new string[] { "Id user", "Message text" }, false)
            {
                StatusUser = StatusUser.Admin,
                Execute = async (model, user, message) => await RepBotActions.Messages.AdminSentToUser(user, RepUsers.GetUser(RepBotActions.ControlUsers.StrToInt(model.Args.FirstOrDefault())), model.Args)
            });

            MessagesLevel.AppendOnlyToThisLevel(new BotCommand("/spam", new string[] { "Id user", "Count of messages" })
            {
                StatusUser = StatusUser.Admin,
                Execute = async (model, user, message) =>
                    await RepBotActions.Messages.UserSpamFromAdmin(user, RepUsers.GetUser(RepBotActions.ControlUsers.StrToInt(model.Args.FirstOrDefault())), RepBotActions.ControlUsers.StrToInt(model.Args.LastOrDefault()))
            });

            BotLevelCommand.AppendToSomeLevels(new BotCommand("/getUsers")
            {
                NameOfLevel = LevelCommand.Users | LevelCommand.Messages,
                StatusUser = StatusUser.Admin,
                Execute = async (model, user, message) => await RepBotActions.ControlUsers.SendListOfUsers(user)
            });

            UsersLevel.AppendOnlyToThisLevel(new BotCommand("/add", "Id user")
            {
                StatusUser = StatusUser.Admin,
                Execute = RepBotActions.ControlUsers.AuthNewUser
            });

            UsersLevel.AppendOnlyToThisLevel(new BotCommand("/add", new string[] { "Id user", "Nickname" }, false)
            {
                StatusUser = StatusUser.Admin,
                Execute = RepBotActions.ControlUsers.AuthNewUser
            });

            UsersLevel.AppendOnlyToThisLevel(new BotCommand("/lock", "Id user")
            {
                StatusUser = StatusUser.Admin,
                Execute = async (model, user, message) =>
                {
                    if (int.TryParse(model.Args.FirstOrDefault(), out int Id))
                    {
                        await RepBotActions.ControlUsers.LockUser(user, Id);
                    }
                }
            });

            UsersLevel.AppendOnlyToThisLevel(new BotCommand("/lock", "Nickname", false)
            {
                StatusUser = StatusUser.Admin,
                Execute = async (model, user, message) => await RepBotActions.ControlUsers.LockUser(user, model.Args)
            });

            UsersLevel.AppendOnlyToThisLevel(new BotCommand("/deauth", "Id user")
            {
                StatusUser = StatusUser.Admin,
                Execute = (model, user, message) =>
                {
                    if (int.TryParse(model.Args.FirstOrDefault(), out int Id))
                    {
                        return RepBotActions.ControlUsers.DeauthUser(user, Id);
                    }
                    return System.Threading.Tasks.Task.CompletedTask;
                }
            });

            UsersLevel.AppendOnlyToThisLevel(new BotCommand("/deauth", "Nickname", false)
            {
                StatusUser = StatusUser.Admin,
                Execute = async (model, user, message) => await RepBotActions.ControlUsers.DeauthUser(user, model.Args)
            });

            UsersLevel.AppendOnlyToThisLevel(new BotCommand("/nickname", new string[] { "Id user", "New nickname" }, false)
            {
                StatusUser = StatusUser.Moder,
                Execute = RepBotActions.ControlUsers.ChangeLocalName
            });

            UsersLevel.AppendOnlyToThisLevel(new BotCommand("/save")
            {
                StatusUser = StatusUser.Admin,
                Execute = async (model, user, message) => await RepBotActions.ControlUsers.SaveChanges(user)
            });

            UsersLevel.AppendOnlyToThisLevel(new BotCommand("/cancel")
            {
                StatusUser = StatusUser.Admin,
                Execute = async (model, user, message) => await RepBotActions.ControlUsers.CancelChanges(user)
            });

            BotLevel.AppendOnlyToThisLevel(new BotCommand("/stopBot")
            {
                StatusUser = StatusUser.Admin,
                Execute = async (model, user, message) => await ExecuteLaunchBot.StopBotAsync()
            });

            BotLevel.AppendOnlyToThisLevel(new BotCommand("/stopApp")
            {
                StatusUser = StatusUser.Admin,
                Execute = (model, user, message) => RepBotActions.ComStopApp(user)
            });

            BotLevel.AppendOnlyToThisLevel(new BotCommand("/reloadBot")
            {
                StatusUser = StatusUser.Admin,
                Execute = async (model, user, message) => await ExecuteLaunchBot.RestartBotAsync()
            });

            PCLevel.AppendOnlyToThisLevel(new BotCommand("/url", "url:")
            {
                Execute = async (model, user, message) => await RepBotActions.ControlPC.ExecuteUrl(model, user, message)
            });

            PCLevel.AppendOnlyToThisLevel(new BotCommand("/turnOff", new string[] { "tMin: int", "tSec: int" })
            {
                StatusUser = StatusUser.Admin,
                Execute = async (model, user, message) =>
                    await RepBotActions.ControlPC.TurnOff(user, RepBotActions.ControlUsers.StrToInt(model.Args.FirstOrDefault()), RepBotActions.ControlUsers.StrToInt(model.Args.LastOrDefault()))
            });

            PCLevel.AppendOnlyToThisLevel(new BotCommand("/cancelOff")
            {
                StatusUser = StatusUser.Admin,
                Execute = async (model, user, message) =>
                {
                    await user.SendMessageAsync(RepBotActions.ControlPC.ExecuteCmdCommand(@"C:\Windows\System32\shutdown.exe", "/a") ?
                        "Успешно выполнено снятие таймера на выключение" :
                        "При снятии таймера, произошла системная ошибка");
                }
            });

            PCLevel.AppendOnlyToThisLevel(new BotCommand("/lock")
            {
                Execute = async (model, user, message) =>
                {
                    await user.SendMessageAsync(RepBotActions.ControlPC.ExecuteCmdCommand(@"C:\Windows\System32\rundll32.exe", "USER32.DLL LockWorkStation") ?
                        "Успешно заблокирована система" :
                        "При блокировке системы, произошла системная ошибка");
                }
            });

            PCLevel.AppendOnlyToThisLevel(new BotCommand("/screen")
            {
                Execute = async (model, user, message) => await RepBotActions.ControlPC.GetScreenshot(user)
            });

            PCLevel.AppendOnlyToThisLevel(new BotCommand("/hibernate")
            {
                Execute = async (model, user, message) =>
                {
                    await user.SendMessageAsync(RepBotActions.ControlPC.ExecuteCmdCommand(@"C:\Windows\System32\shutdown.exe", "/h") ?
                        "Успешно выполнен перевод в гибернацию" :
                        "При переводе в гибернацию, произошла системная ошибка");
                }
            });

            PCLevel.AppendOnlyToThisLevel(new BotCommand("/volume", "[-100..100]; mute")
            {
                Execute = async (model, user, message) => await RepBotActions.ControlPC.ContolVolume.ChangeVolume(user, model.Args.FirstOrDefault())
            });


            Sort();
        }
    }
}
