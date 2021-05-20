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
        public static ITCommand[] Values { get; }

        /// <summary>
        /// Current level for commands
        /// </summary>
        //public static LevelCommand currentLevelCommand { get; set; } = LevelCommand.None;

        //private static void Sort() => Values = new List<BotCommand>(Values.OrderBy(o => o is BotLevelCommand ? -1 : 1).ThenBy(t => t.NameOfLevel).ThenBy(s => s.StatusUser).ThenBy(t => t.CountArgsCommand).ThenBy(t => t.Command));
        private static void Sort() { }


        /// <summary>
        /// Add all commands to the command list
        /// </summary>
        static CollectionCommands()
        {
            Values = new ITCommand[]
            {
                new BotLevelCommand(LevelCommand.None) { LevelDependent = false },
                new BotLevelCommand(LevelCommand.Messages),
                new BotLevelCommand(LevelCommand.Users) { StatusUser = StatusUser.Admin },
                new BotLevelCommand(LevelCommand.PC),
                new BotLevelCommand(LevelCommand.Bot) { StatusUser = StatusUser.Admin },

                new BotCommand
                {
                    Command = "/help",
                    ExampleCommand = "/help",
                    LevelDependent = false,
                    Execute = async (model, user, message) => await RepBotActions.HelpBot(user)
                },

                new BotCommand
                {
                    Command = "/sentToAdmin",
                    CountArgsCommand = -1,
                    ExampleCommand = "/sentToAdmin [Message text]",
                    NameOfLevel = LevelCommand.Messages,
                    LevelDependent = false,
                    Execute = async (model, user, message) => await RepBotActions.Messages.UserSentToAdmin(user, message, model.Args)
                },

                new BotCommand
                {
                    Command = "/sentToUser",
                    CountArgsCommand = -1,
                    ExampleCommand = "/sentToUser [Id user] [Message text]",
                    NameOfLevel = LevelCommand.Messages,
                    StatusUser = StatusUser.Admin,
                    Execute = async (model, user, message) => await RepBotActions.Messages.AdminSentToUser(user, message, RepBotActions.ControlUsers.StrToInt(model.Args.FirstOrDefault()), model.Args)
                },

                new BotCommand
                {
                    Command = "/userSpam",
                    CountArgsCommand = 2,
                    ExampleCommand = "/userSpam [Id user] [Count of messages]",
                    NameOfLevel = LevelCommand.Messages,
                    StatusUser = StatusUser.Admin,
                    Execute = async (model, user, message) =>
                        await RepBotActions.Messages.UserSpamFromAdmin(user, message, RepBotActions.ControlUsers.StrToInt(model.Args.FirstOrDefault()), RepBotActions.ControlUsers.StrToInt(model.Args.LastOrDefault()))
                },

                new BotCommand
                {
                    Command = "/start",
                    ExampleCommand = "/start",
                    LevelDependent = false,
                    Execute = async (model, user, message) => await RepBotActions.ControlUsers.AuthNewUser(user, message, message.From.Id)
                },

                new BotCommand
                {
                    Command = "/listOfUsers",
                    ExampleCommand = "/listOfUsers",
                    NameOfLevel = LevelCommand.Users,
                    StatusUser = StatusUser.Admin,
                    Execute = async (model, user, message) => await RepBotActions.ControlUsers.SendListOfUsers(user)
                },

                new BotCommand
                {
                    Command = "/listOfUsers",
                    ExampleCommand = "/listOfUsers",
                    NameOfLevel = LevelCommand.Messages,
                    StatusUser = StatusUser.Admin,
                    Execute = async (model, user, message) => await RepBotActions.ControlUsers.SendListOfUsers(user)
                },

                new BotCommand
                {
                    Command = "/add",
                    CountArgsCommand = 1,
                    ExampleCommand = "/add [Id user]",
                    NameOfLevel = LevelCommand.Users,
                    StatusUser = StatusUser.Admin,
                    Execute = async (model, user, message) => await RepBotActions.ControlUsers.AuthNewUser(user, message, RepBotActions.ControlUsers.StrToInt(model.Args.FirstOrDefault()))
                },

                new BotCommand
                {
                    Command = "/add",
                    CountArgsCommand = 2,
                    ExampleCommand = "/add [Id user] [Nickname]",
                    NameOfLevel = LevelCommand.Users,
                    StatusUser = StatusUser.Admin,
                    Execute = async (model, user, message) => await RepBotActions.ControlUsers.AuthNewUser(user, message, RepBotActions.ControlUsers.StrToInt(model.Args.FirstOrDefault()), model.Args.LastOrDefault())
                },

                new BotCommand
                {
                    Command = "/lock",
                    CountArgsCommand = 1,
                    ExampleCommand = "/lock [Id user]",
                    NameOfLevel = LevelCommand.Users,
                    StatusUser = StatusUser.Admin,
                    Execute = async (model, user, message) =>
                    {
                        if (int.TryParse(model.Args.FirstOrDefault(), out int Id))
                            await RepBotActions.ControlUsers.LockUser(user, Id);
                    }
                },

                new BotCommand
                {
                    Command = "/lock",
                    CountArgsCommand = -1,
                    ExampleCommand = "/lock [Nickname]",
                    NameOfLevel = LevelCommand.Users,
                    StatusUser = StatusUser.Admin,
                    Execute = async (model, user, message) => await RepBotActions.ControlUsers.LockUser(user, model.Args)
                },

                new BotCommand
                {
                    Command = "/deauth",
                    CountArgsCommand = 1,
                    ExampleCommand = "/deauth [Id user]",
                    NameOfLevel = LevelCommand.Users,
                    StatusUser = StatusUser.Admin,
                    Execute = async (model, user, message) =>
                    {
                        if (int.TryParse(model.Args.FirstOrDefault(), out int Id))
                            await RepBotActions.ControlUsers.DeauthUser(user, Id);
                    }
                },

                new BotCommand
                {
                    Command = "/deauth",
                    CountArgsCommand = -1,
                    ExampleCommand = "/deauth [Nickname]",
                    NameOfLevel = LevelCommand.Users,
                    StatusUser = StatusUser.Admin,
                    Execute = async (model, user, message) => await RepBotActions.ControlUsers.DeauthUser(user, model.Args)
                },

                new BotCommand
                {
                    Command = "/nickname",
                    CountArgsCommand = -1,
                    ExampleCommand = "/nickname [Id user] [New nickname]",
                    NameOfLevel = LevelCommand.Users,
                    StatusUser = StatusUser.Moderator,
                    Execute = async (model, user, message) =>
                    {
                        if (model.Args.Length >= 2)
                            await RepBotActions.ControlUsers.ChangeLocalName(user, RepBotActions.ControlUsers.StrToInt(model.Args.FirstOrDefault()), model.Args);
                        else
                            await RepBotActions.SendMessageWrongNumberOfArgs(user);
                    }
                },

                new BotCommand
                {
                    Command = "/saveChanges",
                    ExampleCommand = "/saveChanges",
                    NameOfLevel = LevelCommand.Users,
                    StatusUser = StatusUser.Admin,
                    Execute = async (model, user, message) => await RepBotActions.ControlUsers.SaveChanges(user)
                },

                new BotCommand
                {
                    Command = "/cancelChanges",
                    ExampleCommand = "/cancelChanges",
                    NameOfLevel = LevelCommand.Users,
                    StatusUser = StatusUser.Admin,
                    Execute = async (model, user, message) => await RepBotActions.ControlUsers.CancelChanges(user)
                },

                new BotCommand
                {
                    Command = "/stopBot",
                    ExampleCommand = "/stopBot",
                    NameOfLevel = LevelCommand.Bot,
                    StatusUser = StatusUser.Admin,
                    Execute = async (model, user, message) => await ExecuteLaunchBot.StopBotAsync()
                },

                new BotCommand
                {
                    Command = "/stopApp",
                    ExampleCommand = "/stopApp",
                    NameOfLevel = LevelCommand.Bot,
                    StatusUser = StatusUser.Admin,
                    Execute = (model, user, message) => RepBotActions.ComStopApp()
                },

                new BotCommand
                {
                    Command = "/reloadBot",
                    ExampleCommand = "/reloadBot",
                    NameOfLevel = LevelCommand.Bot,
                    StatusUser = StatusUser.Admin,
                    Execute = async (model, user, message) => await ExecuteLaunchBot.RestartBotAsync()
                },

                new BotCommand
                {
                    Command = "/url",
                    CountArgsCommand = 1,
                    ExampleCommand = "/url [url:]",
                    NameOfLevel = LevelCommand.PC,
                    Execute = async (model, user, message) => await RepBotActions.ControlPC.ExecuteUrl(message, model.Args.FirstOrDefault())
                },

                new BotCommand
                {
                    Command = "/turnOff",
                    CountArgsCommand = 2,
                    ExampleCommand = "/turnOff [tMin: int] [tSec: int]",
                    NameOfLevel = LevelCommand.PC,
                    StatusUser = StatusUser.Admin,
                    Execute = async (model, user, message) =>
                        await RepBotActions.ControlPC.TurnOff(user, RepBotActions.ControlUsers.StrToInt(model.Args.FirstOrDefault()), RepBotActions.ControlUsers.StrToInt(model.Args.LastOrDefault()))
                },

                new BotCommand
                {
                    Command = "/cancelOff",
                    ExampleCommand = "/cancelOff",
                    NameOfLevel = LevelCommand.PC,
                    StatusUser = StatusUser.Admin,
                    Execute = async (model, user, message) =>
                    {
                        if (RepBotActions.ControlPC.ExecuteCmdCommand(@"C:\Windows\System32\shutdown.exe", "/a"))
                            await App.Api.SendTextMessageAsync(message.From.Id, "Успешно выполнено снятие таймера на выключение");
                        else
                            await RepUsers.SendMessage(message.From.Id, "При снятии таймера, произошла системная ошибка");
                    }
                },

                new BotCommand
                {
                    Command = "/lockSys",
                    ExampleCommand = "/lockSys",
                    NameOfLevel = LevelCommand.PC,
                    Execute = async (model, user, message) =>
                    {
                        if (RepBotActions.ControlPC.ExecuteCmdCommand(@"C:\Windows\System32\rundll32.exe", "USER32.DLL LockWorkStation"))
                            await RepUsers.SendMessage(message.From.Id, "Успешно заблокирована система");
                        else
                            await RepUsers.SendMessage(message.From.Id, "При блокировке системы, произошла системная ошибка");
                    }
                },

                new BotCommand
                {
                    Command = "/getScreen",
                    ExampleCommand = "/getScreen",
                    NameOfLevel = LevelCommand.PC,
                    Execute = async (model, user, message) => await RepBotActions.ControlPC.GetScreenshot(user)
                },

                new BotCommand
                {
                    Command = "/hiberSys",
                    ExampleCommand = "/hiberSys",
                    NameOfLevel = LevelCommand.PC,
                    Execute = async (model, user, message) =>
                    {
                        if (RepBotActions.ControlPC.ExecuteCmdCommand(@"C:\Windows\System32\shutdown.exe", "/h"))
                            await RepUsers.SendMessage(message.From.Id, "Успешно выполнен перевод в гибернацию");
                        else
                            await RepUsers.SendMessage(message.From.Id, "При переводе в гибернацию, произошла системная ошибка");
                    }
                },

                new BotCommand
                {
                    Command = "/volume",
                    CountArgsCommand = 1,
                    ExampleCommand = "/volume [int [-100..100]; mute]",
                    NameOfLevel = LevelCommand.PC,
                    Execute = async (model, user, message) => await RepBotActions.ControlPC.ContolVolume.ChangeVolume(user, model.Args.FirstOrDefault())
                },

                new BotCommand
                {
                    Command = "/keyboard",
                    CountArgsCommand = 1,
                    ExampleCommand = "/keyboard [true; false; all]",
                    Execute = async (model, user, message) => await RepBotActions.ShowScreenButtons(user, model.Args.FirstOrDefault())
                }
            };

            Sort();
        }
    }
}
