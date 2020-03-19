using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HamstiBotWPF.Core;
using LevelCommand = HamstiBotWPF.Core.BotLevelCommand.LevelCommand;

namespace HamstiBotWPF.LogicRepository
{
    /// <summary>
    /// To work with a list of commands
    /// </summary>
    public static class RepCommands
    {
        private static void Sort() => GlobalUnit.botCommands = new List<BotCommand>(GlobalUnit.botCommands.OrderBy(o => o is BotLevelCommand ? -1 : 1).ThenBy(t => t.NameOfLevel).ThenBy(t => t.CountArgsCommand).ThenBy(t => t.Command));
        /// <summary>
        /// Add all commands to the command list
        /// </summary>
        public static void Refresh()
        {
            GlobalUnit.botCommands.Add(new BotLevelCommand(LevelCommand.Root) { Command = BotLevelCommand.TOPREVLEVEL, LevelDependent = false });
            GlobalUnit.botCommands.Add(new BotLevelCommand(LevelCommand.Messages));
            GlobalUnit.botCommands.Add(new BotLevelCommand(LevelCommand.ControlUsers) { VisibleForUsers = false });
            GlobalUnit.botCommands.Add(new BotLevelCommand(LevelCommand.ControlPC));

            GlobalUnit.botCommands.Add(new BotCommand
            {
                Command = "/help",
                ExampleCommand = "/help",
                LevelDependent = false,
                Execute = async (model, message) =>
                {
                    await RepBotActions.HelpBot(message);
                }
            });

            GlobalUnit.botCommands.Add(new BotCommand
            {
                Command = "/helpAdmin",
                ExampleCommand = "/helpAdmin",
                VisibleForUsers = false,
                LevelDependent = false,
                Execute = async (model, message) =>
                {
                    await RepBotActions.HelpBotAdmin(message);
                }
            });

            GlobalUnit.botCommands.Add(new BotCommand
            {
                Command = "/messageToAdmin",
                CountArgsCommand = -1,
                ExampleCommand = "/messageToAdmin [Message text]",
                NameOfLevel = LevelCommand.Messages,
                LevelDependent = false,
                Execute = (model, message) =>
                {
                    RepBotActions.Messages.UserSentToAdmin(message, model.Args);
                }
            });

            GlobalUnit.botCommands.Add(new BotCommand
            {
                Command = "/messageToUser",
                CountArgsCommand = -1,
                ExampleCommand = "/messageToUser [Id user] [Message text]",
                NameOfLevel = LevelCommand.Messages,
                VisibleForUsers = false,
                Execute = (model, message) =>
                {
                    RepBotActions.Messages.AdminSentToUser(message, RepBotActions.ControlUsers.StrToInt(model.Args.FirstOrDefault()), model.Args);
                }
            });

            GlobalUnit.botCommands.Add(new BotCommand
            {
                Command = "/messageSpamToUser",
                CountArgsCommand = 2,
                ExampleCommand = "/messageSpamToUser [Id user] [Count of messages]",
                NameOfLevel = LevelCommand.Messages,
                VisibleForUsers = false,
                Execute = (model, message) =>
                {
                    RepBotActions.Messages.UserSpamFromAdmin(message, RepBotActions.ControlUsers.StrToInt(model.Args.FirstOrDefault()), RepBotActions.ControlUsers.StrToInt(model.Args.LastOrDefault()));
                }
            });

            GlobalUnit.botCommands.Add(new BotCommand
            {
                Command = "/start",
                ExampleCommand = "/start",
                LevelDependent = false,
                Execute = async (model, message) =>
                {
                    await RepBotActions.ControlUsers.AuthNewUser(message, message.From.Id);
                }
            });

            GlobalUnit.botCommands.Add(new BotCommand
            {
                Command = "/listOfUsers",
                ExampleCommand = "/listOfUsers",
                NameOfLevel = LevelCommand.ControlUsers,
                VisibleForUsers = false,
                Execute = async (model, message) =>
                {
                    await RepBotActions.ControlUsers.SendListOfUsers(message);
                }
            });
            
            GlobalUnit.botCommands.Add(new BotCommand
            {
                Command = "/listOfUsers",
                ExampleCommand = "/listOfUsers",
                NameOfLevel = LevelCommand.Messages,
                VisibleForUsers = false,
                Execute = async (model, message) =>
                {
                    await RepBotActions.ControlUsers.SendListOfUsers(message);
                }
            });

            GlobalUnit.botCommands.Add(new BotCommand
            {
                Command = "/addUser",
                CountArgsCommand = 1,
                ExampleCommand = "/addUser [Id user]",
                NameOfLevel = LevelCommand.ControlUsers,
                VisibleForUsers = false,
                Execute = async (model, message) =>
                {
                    await RepBotActions.ControlUsers.AuthNewUser(message, RepBotActions.ControlUsers.StrToInt(model.Args.FirstOrDefault()));
                }
            });

            GlobalUnit.botCommands.Add(new BotCommand
            {
                Command = "/addUser",
                CountArgsCommand = 2,
                ExampleCommand = "/addUser [Id user] [Nickname]",
                NameOfLevel = LevelCommand.ControlUsers,
                VisibleForUsers = false,
                Execute = async (model, message) =>
                {
                    await RepBotActions.ControlUsers.AuthNewUser(message, RepBotActions.ControlUsers.StrToInt(model.Args.FirstOrDefault()), model.Args.LastOrDefault());
                }
            });

            GlobalUnit.botCommands.Add(new BotCommand
            {
                Command = "/lockUser",
                CountArgsCommand = 1,
                ExampleCommand = "/lockUser [Id user]",
                NameOfLevel = LevelCommand.ControlUsers,
                VisibleForUsers = false,
                Execute = async (model, message) =>
                {
                    if (int.TryParse(model.Args.FirstOrDefault(), out int IdUser))
                        await RepBotActions.ControlUsers.LockUser(message, IdUser);
                }
            });

            GlobalUnit.botCommands.Add(new BotCommand
            {
                Command = "/lockUser",
                CountArgsCommand = -1,
                ExampleCommand = "/lockUser [Nickname]",
                NameOfLevel = LevelCommand.ControlUsers,
                VisibleForUsers = false,
                Execute = async (model, message) =>
                {
                    await RepBotActions.ControlUsers.LockUser(message, model.Args);
                }
            });

            GlobalUnit.botCommands.Add(new BotCommand
            {
                Command = "/deauthUser",
                CountArgsCommand = 1,
                ExampleCommand = "/deauthUser [Id user]",
                NameOfLevel = LevelCommand.ControlUsers,
                VisibleForUsers = false,
                Execute = async (model, message) =>
                {
                    if (int.TryParse(model.Args.FirstOrDefault(), out int IdUser))
                        await RepBotActions.ControlUsers.DeauthUser(message, IdUser);
                }
            });

            GlobalUnit.botCommands.Add(new BotCommand
            {
                Command = "/deauthUser",
                CountArgsCommand = -1,
                ExampleCommand = "/deauthUser [Nickname]",
                NameOfLevel = LevelCommand.ControlUsers,
                VisibleForUsers = false,
                Execute = async (model, message) =>
                {
                    await RepBotActions.ControlUsers.DeauthUser(message, model.Args);
                }
            });

            GlobalUnit.botCommands.Add(new BotCommand
            {
                Command = "/changeNickname",
                CountArgsCommand = -1,
                ExampleCommand = "/changeNickname [Id user] [New nickname]",
                NameOfLevel = LevelCommand.ControlUsers,
                VisibleForUsers = false,
                Execute = async (model, message) =>
                {
                    if (model.Args.Length >= 2)
                        await RepBotActions.ControlUsers.ChangeLocalName(message, RepBotActions.ControlUsers.StrToInt(model.Args.FirstOrDefault()), model.Args);
                    else
                        await RepBotActions.SendMessageWrongNumberOfArgs(message);
                }
            });

            GlobalUnit.botCommands.Add(new BotCommand
            {
                Command = "/saveChanges",
                ExampleCommand = "/saveChanges",
                NameOfLevel = LevelCommand.ControlUsers,
                VisibleForUsers = false,
                Execute = async (model, message) =>
                {
                    await RepBotActions.ControlUsers.SaveChanges(message);
                }
            });

            GlobalUnit.botCommands.Add(new BotCommand
            {
                Command = "/cancelChanges",
                ExampleCommand = "/cancelChanges",
                NameOfLevel = LevelCommand.ControlUsers,
                VisibleForUsers = false,
                Execute = async (model, message) =>
                {
                    await RepBotActions.ControlUsers.CancelChanges(message);
                }
            });

            GlobalUnit.botCommands.Add(new BotCommand
            {
                Command = "/stopBot",
                ExampleCommand = "/stopBot",
                VisibleForUsers = false,
                Execute = (model, message) =>
                {
                    RepBotActions.ComStopBot();
                }
            });

            GlobalUnit.botCommands.Add(new BotCommand
            {
                Command = "/stopApp",
                ExampleCommand = "/stopApp",
                VisibleForUsers = false,
                Execute = (model, message) =>
                {
                    RepBotActions.ComStopApp(message);
                }
            });

            GlobalUnit.botCommands.Add(new BotCommand
            {
                Command = "/reloadBot",
                ExampleCommand = "/reloadBot",
                VisibleForUsers = false,
                Execute = (model, message) =>
                {
                    Task.Run(() => ExecuteLaunchBot.RestartBotAsync());
                }
            });

            GlobalUnit.botCommands.Add(new BotCommand
            {
                Command = "/url",
                CountArgsCommand = 1,
                ExampleCommand = "/url [url:]",
                NameOfLevel = LevelCommand.ControlPC,
                Execute = (model, message) =>
                {
                    RepBotActions.ControlPC.ExecuteUrl(message, model.Args.FirstOrDefault());
                }
            });

            GlobalUnit.botCommands.Add(new BotCommand
            {
                Command = "/turnOff",
                CountArgsCommand = 2,
                ExampleCommand = "/turnOff [tMin: int] [tSec: int]",
                NameOfLevel = LevelCommand.ControlPC,
                VisibleForUsers = false,
                Execute = async (model, message) =>
                {
                    await RepBotActions.ControlPC.TurnOff(message, RepBotActions.ControlUsers.StrToInt(model.Args.FirstOrDefault()), RepBotActions.ControlUsers.StrToInt(model.Args.LastOrDefault()));
                }
            });

            GlobalUnit.botCommands.Add(new BotCommand
            {
                Command = "/cancelOff",
                ExampleCommand = "/cancelOff",
                NameOfLevel = LevelCommand.ControlPC,
                VisibleForUsers = false,
                Execute = (model, message) =>
                {
                    if (RepBotActions.ControlPC.ExecuteCmdCommand(@"C:\Windows\System32\shutdown.exe", "/a"))
                        GlobalUnit.Api.SendTextMessageAsync(message.From.Id, "Успешно выполнено снятие таймера на выключение");
                    else
                        GlobalUnit.Api.SendTextMessageAsync(message.From.Id, "При снятии таймера, произошла системная ошибка");
                }
            });

            GlobalUnit.botCommands.Add(new BotCommand
            {
                Command = "/lockSystem",
                ExampleCommand = "/lockSystem",
                NameOfLevel = LevelCommand.ControlPC,
                Execute = (model, message) =>
                {
                    if (RepBotActions.ControlPC.ExecuteCmdCommand(@"C:\Windows\System32\rundll32.exe", "USER32.DLL LockWorkStation"))
                        GlobalUnit.Api.SendTextMessageAsync(message.From.Id, "Успешно заблокирована система");
                    else
                        GlobalUnit.Api.SendTextMessageAsync(message.From.Id, "При блокировке системы, произошла системная ошибка");
                }
            });

            GlobalUnit.botCommands.Add(new BotCommand
            {
                Command = "/getScreen",
                ExampleCommand = "/getScreen",
                NameOfLevel = LevelCommand.ControlPC,
                Execute = (model, message) =>
                {
                    RepBotActions.ControlPC.GetScreenshot(message);
                }
            });

            GlobalUnit.botCommands.Add(new BotCommand
            {
                Command = "/hiberSystem",
                ExampleCommand = "/hiberSystem",
                NameOfLevel = LevelCommand.ControlPC,
                Execute = (model, message) =>
                {
                    if (RepBotActions.ControlPC.ExecuteCmdCommand(@"C:\Windows\System32\shutdown.exe", "/h"))
                        GlobalUnit.Api.SendTextMessageAsync(message.From.Id, "Успешно выполнен перевод в гибернацию");
                    else
                        GlobalUnit.Api.SendTextMessageAsync(message.From.Id, "При переводе в гибернацию, произошла системная ошибка");
                }
            });

            GlobalUnit.botCommands.Add(new BotCommand
            {
                Command = "/volume",
                CountArgsCommand = 1,
                ExampleCommand = "/volume [int [-100..100]; mute]",
                NameOfLevel = LevelCommand.ControlPC,
                Execute = (model, message) =>
                {
                    RepBotActions.ControlPC.ContolVolume.changeVolume(message, model.Args.FirstOrDefault());
                }
            });

            GlobalUnit.botCommands.Add(new BotCommand
            {
                Command = "/keyboard",
                CountArgsCommand = 1,
                ExampleCommand = "/keyboard [true; false; all]",
                Execute = (model, message) =>
                {
                    RepBotActions.ShowScreenButtons(message, model.Args.FirstOrDefault());
                }
            });

            Sort();
        }
    }
}
