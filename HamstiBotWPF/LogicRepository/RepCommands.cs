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
            GlobalUnit.botCommands.Add(new BotLevelCommand(LevelCommand.Root));
            GlobalUnit.botCommands.Add(new BotLevelCommand(LevelCommand.Messages));
            GlobalUnit.botCommands.Add(new BotLevelCommand(LevelCommand.ControlUsers));
            GlobalUnit.botCommands.Add(new BotLevelCommand(LevelCommand.ControlPC));

            GlobalUnit.botCommands.Add(new BotCommand
            {
                Command = "/help",
                ExampleCommand = "/help",
                Execute = async (model, message) =>
                {
                    await RepBotActions.helpBot(message);
                }
            });

            GlobalUnit.botCommands.Add(new BotCommand
            {
                Command = "/helpAdmin",
                ExampleCommand = "/helpAdmin",
                VisibleForUsers = false,
                Execute = async (model,message) =>
                {
                    await RepBotActions.helpBotAdmin(message);
                }
            });

            GlobalUnit.botCommands.Add(new BotCommand
            {
                Command = "/messageToAdmin",
                CountArgsCommand = -1,
                ExampleCommand = "/messageToAdmin YourMessage",
                NameOfLevel = LevelCommand.Messages,
                Execute = (model, message) =>
                {
                    RepBotActions.UserSendMessageForAdmin(message);
                }
            });

            GlobalUnit.botCommands.Add(new BotCommand
            {
                Command = "/messageToUser",
                CountArgsCommand = -1,
                ExampleCommand = "/messageToUser IdUser YourMessage",
                NameOfLevel = LevelCommand.Messages,
                VisibleForUsers = false,
                Execute = (model, message) =>
                {
                    RepBotActions.AdminSendMessageToUser(message, int.Parse(model.Args.FirstOrDefault()));
                }
            });

            GlobalUnit.botCommands.Add(new BotCommand
            {
                Command = "/messageSpamToUser",
                CountArgsCommand = 2,
                ExampleCommand = "/messageSpamToUser IdUser CountMessages",
                NameOfLevel = LevelCommand.Messages,
                VisibleForUsers = false,
                Execute = (model, message) =>
                {
                    RepBotActions.AdminSpamMessageToUser(message, int.Parse(model.Args.FirstOrDefault()), int.Parse(model.Args.LastOrDefault()));
                }
            });

            GlobalUnit.botCommands.Add(new BotCommand
            {
                Command = "/start",
                ExampleCommand = "/start",
                Execute = (model, message) =>
                {
                    RepBotActions.ControlUsers.authNewUser(message, message.From.Id);
                    //await GlobalUnit.Api.SetChatDescriptionAsync(new Telegram.Bot.Types.ChatId(Properties.Settings.Default.AdminId));
                }
            });

            GlobalUnit.botCommands.Add(new BotCommand
            {
                Command = "/ListOfUsers",
                ExampleCommand = "/ListOfUsers",
                NameOfLevel = LevelCommand.ControlUsers,
                VisibleForUsers = false,
                Execute = async (model, message) =>
                {
                    await GlobalUnit.Api.SendTextMessageAsync(Properties.Settings.Default.AdminId, $"Список пользователей бота {GlobalUnit.Api.GetMeAsync().Result}:\n\n" + RepBotActions.ControlUsers.ListOfUsersParseString);
                }
            });

            GlobalUnit.botCommands.Add(new BotCommand
            {
                Command = "/addUser",
                CountArgsCommand = 1,
                ExampleCommand = "/addUser [IdUser]",
                NameOfLevel = LevelCommand.ControlUsers,
                VisibleForUsers = false,
                Execute = (model, message) =>
                { 
                    RepBotActions.ControlUsers.authNewUser(message, RepBotActions.ControlUsers.StrToInt(model.Args.FirstOrDefault()));
                }
            });

            GlobalUnit.botCommands.Add(new BotCommand
            {
                Command = "/lockUser",
                CountArgsCommand = 1,
                ExampleCommand = "/lockUser [IdUser]",
                NameOfLevel = LevelCommand.ControlUsers,
                VisibleForUsers = false,
                Execute = (model, message) =>
                {
                    RepBotActions.ControlUsers.lockUser(message, RepBotActions.ControlUsers.StrToInt(model.Args.FirstOrDefault()));
                }
            });

            GlobalUnit.botCommands.Add(new BotCommand
            {
                Command = "/stopBot",
                ExampleCommand = "/stopBot",
                VisibleForUsers = false,
                Execute = (model, message) =>
                {
                    RepBotActions.comStopBot(message);
                }
            });

            GlobalUnit.botCommands.Add(new BotCommand
            {
                Command = "/stopApp",
                ExampleCommand = "/stopApp",
                VisibleForUsers = false,
                Execute = (model, message) =>
                {
                    RepBotActions.comStopApp(message);
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
                    RepBotActions.ControlPC.executeUrl(message, model.Command, model.Args.FirstOrDefault());
                }
            });

            GlobalUnit.botCommands.Add(new BotCommand
            {
                Command = "/turnOff",
                CountArgsCommand = 2,
                ExampleCommand = "/turnOff [tMin: int, tSec: int]",
                NameOfLevel = LevelCommand.ControlPC,
                VisibleForUsers = false,
                Execute = (model, message) =>
                {
                    RepBotActions.ControlPC.turnOff(message, int.Parse(model.Args.FirstOrDefault()), int.Parse(model.Args.LastOrDefault()));
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
                    if (RepBotActions.ControlPC.cmdCommands(message, @"C:\Windows\System32\shutdown.exe", "/a"))
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
                    if (RepBotActions.ControlPC.cmdCommands(message, @"C:\Windows\System32\rundll32.exe", "USER32.DLL LockWorkStation"))
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
                    RepBotActions.ControlPC.getScreenshot(message);
                }
            });

            GlobalUnit.botCommands.Add(new BotCommand
            {
                Command = "/hiberSystem",
                ExampleCommand = "/hiberSystem",
                NameOfLevel = LevelCommand.ControlPC,
                Execute = (model, message) =>
                {
                    if (RepBotActions.ControlPC.cmdCommands(message, @"C:\Windows\System32\shutdown.exe", "/h"))
                        GlobalUnit.Api.SendTextMessageAsync(message.From.Id, "Успешно выполнен перевод в гибернацию");
                    else
                        GlobalUnit.Api.SendTextMessageAsync(message.From.Id, "При переводе в гибернацию, произошла системная ошибка");
                }
            });

            GlobalUnit.botCommands.Add(new BotCommand
            {
                Command = "/volume",
                CountArgsCommand = 1,
                ExampleCommand = "/volume [int value[-100..100, mute]]",
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
                ExampleCommand = "/keyboard [true;false;all]",
                Execute = (model, message) =>
                {
                    RepBotActions.showScreenButtons(message, model.Args.FirstOrDefault()); 
                }
            });

            Sort();
        }
    }
}
