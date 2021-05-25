using System.Linq;
using System.Collections.Generic;
using TBotHamsti.Core;
using LevelCommand = TBotHamsti.Core.BotLevelCommand.LevelCommand;
using StatusUser = TBotHamsti.LogicRepository.RepUsers.StatusUser;
using System;

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
            RootLevel.AppendOnlyToThisLevel(MessagesLevel);
            RootLevel.AppendOnlyToThisLevel(PCLevel);
            RootLevel.AppendOnlyToThisLevel(BotLevel);

            RootLevel.AppendToSomeLevels(BotLevelCommand.UPLevel);


            HelpCommand = new BotCommand("/help")
            { 
                NameOfLevel = LevelCommand.All,
                Execute = async (model, user, message) => await RepBotActions.HelpBot(user)
            };
            RootLevel.AppendToSomeLevels(HelpCommand);

            RootLevel.AppendToSomeLevels(new BotCommand("/start")
            {
                NameOfLevel = LevelCommand.All,
                //LevelDependent = false,
                Execute = async (model, user, message) => await App.Current.Dispatcher.InvokeAsync(() => RepBotActions.ControlUsers.AuthNewUser(user, message, message.From.Id))
            });

            RootLevel.AppendToSomeLevels(new BotCommand("/keyboard", "/keyboard [true; false; all]")
            {
                CountArgsCommand = 1,
                NameOfLevel = LevelCommand.All,
                Execute = async (model, user, message) => await RepBotActions.ShowScreenButtons(user, model.Args.FirstOrDefault())
            });

            SendMessageToAdminCommand = new BotCommand("/sendToAdmin", "/sendToAdmin [Message text]")
            {
                CountArgsCommand = -1,
                NameOfLevel = LevelCommand.All,
                //LevelDependent = false,
                Execute = async (model, user, message) => await RepBotActions.Messages.UserSentToAdmin(user, message, model.Args)
            };
            MessagesLevel.AppendToSomeLevels(SendMessageToAdminCommand);

            MessagesLevel.AppendOnlyToThisLevel(new BotCommand("/toUser", "/toUser [Id user] [Message text]")
            {
                CountArgsCommand = -1,
                StatusUser = StatusUser.Admin,
                Execute = async (model, user, message) => await RepBotActions.Messages.AdminSentToUser(user, message, RepBotActions.ControlUsers.StrToInt(model.Args.FirstOrDefault()), model.Args)
            });

            MessagesLevel.AppendOnlyToThisLevel(new BotCommand("/spam", "/spam [Id user] [Count of messages]")
            {
                CountArgsCommand = 2,
                StatusUser = StatusUser.Admin,
                Execute = async (model, user, message) =>
                    await RepBotActions.Messages.UserSpamFromAdmin(user, message, RepBotActions.ControlUsers.StrToInt(model.Args.FirstOrDefault()), RepBotActions.ControlUsers.StrToInt(model.Args.LastOrDefault()))
            });

            UsersLevel.AppendToSomeLevels(new BotCommand("/getUsers")
            {
                NameOfLevel = LevelCommand.Users | LevelCommand.Messages,
                StatusUser = StatusUser.Admin,
                Execute = async (model, user, message) => await RepBotActions.ControlUsers.SendListOfUsers(user)
            });

            UsersLevel.AppendOnlyToThisLevel(new BotCommand("/add", "/add [Id user]")
            {
                CountArgsCommand = 1,
                StatusUser = StatusUser.Admin,
                Execute = async (model, user, message) => await App.Current.Dispatcher.InvokeAsync(() => RepBotActions.ControlUsers.AuthNewUser(user, message, RepBotActions.ControlUsers.StrToInt(model.Args.FirstOrDefault())))
            });

            UsersLevel.AppendOnlyToThisLevel(new BotCommand("/add", "/add [Id user] [Nickname]")
            {
                CountArgsCommand = 2,
                StatusUser = StatusUser.Admin,
                Execute = async (model, user, message) => await App.Current.Dispatcher.InvokeAsync(() => RepBotActions.ControlUsers.AuthNewUser(user, message, RepBotActions.ControlUsers.StrToInt(model.Args.FirstOrDefault()), model.Args.LastOrDefault()))
            });

            UsersLevel.AppendOnlyToThisLevel(new BotCommand("/lock", "/lock [Id user]")
            {
                CountArgsCommand = 1,
                StatusUser = StatusUser.Admin,
                Execute = async (model, user, message) =>
                {
                    if (int.TryParse(model.Args.FirstOrDefault(), out int Id))
                        await RepBotActions.ControlUsers.LockUser(user, Id);
                }
            });

            UsersLevel.AppendOnlyToThisLevel(new BotCommand("/lock", "/lock [Nickname]")
            {
                CountArgsCommand = -1,
                StatusUser = StatusUser.Admin,
                Execute = async (model, user, message) => await RepBotActions.ControlUsers.LockUser(user, model.Args)
            });

            UsersLevel.AppendOnlyToThisLevel(new BotCommand("/deauth", "/deauth [Id user]")
            {
                CountArgsCommand = 1,
                StatusUser = StatusUser.Admin,
                Execute = async (model, user, message) =>
                {
                    if (int.TryParse(model.Args.FirstOrDefault(), out int Id))
                        await RepBotActions.ControlUsers.DeauthUser(user, Id);
                }
            });

            UsersLevel.AppendOnlyToThisLevel(new BotCommand("/deauth", "/deauth [Nickname]")
            {
                CountArgsCommand = -1,
                StatusUser = StatusUser.Admin,
                Execute = async (model, user, message) => await RepBotActions.ControlUsers.DeauthUser(user, model.Args)
            });

            UsersLevel.AppendOnlyToThisLevel(new BotCommand("/nickname", "/nickname [Id user] [New nickname]")
            {
                CountArgsCommand = -1,
                StatusUser = StatusUser.Moderator,
                Execute = async (model, user, message) =>
                {
                    if (model.Args.Length >= 2)
                        await RepBotActions.ControlUsers.ChangeLocalName(user, RepBotActions.ControlUsers.StrToInt(model.Args.FirstOrDefault()), model.Args);
                    else
                        await RepBotActions.SendMessageWrongNumberOfArgs(user);
                }
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
                Execute = (model, user, message) => RepBotActions.ComStopApp()
            });

            BotLevel.AppendOnlyToThisLevel(new BotCommand("/reloadBot")
            {
                StatusUser = StatusUser.Admin,
                Execute = async (model, user, message) => await ExecuteLaunchBot.RestartBotAsync()
            });

            PCLevel.AppendOnlyToThisLevel(new BotCommand("/url", "/url [url:]")
            {
                CountArgsCommand = 1,
                Execute = async (model, user, message) => await RepBotActions.ControlPC.ExecuteUrl(message, model.Args.FirstOrDefault())
            });

            PCLevel.AppendOnlyToThisLevel(new BotCommand("/turnOff", "/turnOff [tMin: int] [tSec: int]")
            {
                CountArgsCommand = 2,
                StatusUser = StatusUser.Admin,
                Execute = async (model, user, message) =>
                    await RepBotActions.ControlPC.TurnOff(user, RepBotActions.ControlUsers.StrToInt(model.Args.FirstOrDefault()), RepBotActions.ControlUsers.StrToInt(model.Args.LastOrDefault()))
            });

            PCLevel.AppendOnlyToThisLevel(new BotCommand("/cancelOff")
            {
                StatusUser = StatusUser.Admin,
                Execute = async (model, user, message) =>
                {
                    if (RepBotActions.ControlPC.ExecuteCmdCommand(@"C:\Windows\System32\shutdown.exe", "/a"))
                        await App.Api.SendTextMessageAsync(message.From.Id, "Успешно выполнено снятие таймера на выключение");
                    else
                        await RepUsers.SendMessage(message.From.Id, "При снятии таймера, произошла системная ошибка");
                }
            });

            PCLevel.AppendOnlyToThisLevel(new BotCommand("/lock")
            {
                Execute = async (model, user, message) =>
                {
                    if (RepBotActions.ControlPC.ExecuteCmdCommand(@"C:\Windows\System32\rundll32.exe", "USER32.DLL LockWorkStation"))
                        await RepUsers.SendMessage(message.From.Id, "Успешно заблокирована система");
                    else
                        await RepUsers.SendMessage(message.From.Id, "При блокировке системы, произошла системная ошибка");
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
                    if (RepBotActions.ControlPC.ExecuteCmdCommand(@"C:\Windows\System32\shutdown.exe", "/h"))
                        await RepUsers.SendMessage(message.From.Id, "Успешно выполнен перевод в гибернацию");
                    else
                        await RepUsers.SendMessage(message.From.Id, "При переводе в гибернацию, произошла системная ошибка");
                }
            });

            PCLevel.AppendOnlyToThisLevel(new BotCommand("/volume", "/volume [int [-100..100]; mute]")
            {
                CountArgsCommand = 1,
                Execute = async (model, user, message) => await RepBotActions.ControlPC.ContolVolume.ChangeVolume(user, model.Args.FirstOrDefault())
            });


            Sort();
        }
    }
}
