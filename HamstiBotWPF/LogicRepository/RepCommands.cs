using System.Linq;
using System.Collections.Generic;
using HamstiBotWPF.Core;
using LevelCommand = HamstiBotWPF.Core.BotLevelCommand.LevelCommand;
using StatusUser = HamstiBotWPF.LogicRepository.RepUsers.StatusUser;

namespace HamstiBotWPF.LogicRepository
{
    /// <summary>
    /// To work with a list of commands
    /// </summary>
    public static class RepCommands
    {
        /// <summary>
        /// List of all commands for working with the bot
        /// </summary>
        public static List<BotCommand> botCommands = new List<BotCommand>();

        /// <summary>
        /// Current level for commands
        /// </summary>
        public static LevelCommand currentLevelCommand = LevelCommand.Root;

        private static void Sort() => botCommands = new List<BotCommand>(botCommands.OrderBy(o => o is BotLevelCommand ? -1 : 1).ThenBy(t => t.NameOfLevel).ThenBy(t => t.CountArgsCommand).ThenBy(t => t.Command));
        /// <summary>
        /// Add all commands to the command list
        /// </summary>
        public static void Refresh()
        {
            botCommands.Add(new BotLevelCommand(LevelCommand.Root) { Command = BotLevelCommand.TOPREVLEVEL, LevelDependent = false });
            botCommands.Add(new BotLevelCommand(LevelCommand.Messages));
            botCommands.Add(new BotLevelCommand(LevelCommand.ControlUsers) { StatusUser = StatusUser.Admin });
            botCommands.Add(new BotLevelCommand(LevelCommand.ControlPC));
            botCommands.Add(new BotLevelCommand(LevelCommand.ControlBot) { StatusUser = StatusUser.Admin });

            botCommands.Add(new BotCommand
            {
                Command = "/help",
                ExampleCommand = "/help",
                LevelDependent = false,
                Execute = async (model, message) => await RepBotActions.HelpBot(message)
            });

            botCommands.Add(new BotCommand
            {
                Command = "/sentToAdmin",
                CountArgsCommand = -1,
                ExampleCommand = "/sentToAdmin [Message text]",
                NameOfLevel = LevelCommand.Messages,
                LevelDependent = false,
                Execute = async (model, message) => await RepBotActions.Messages.UserSentToAdmin(message, model.Args)
            });

            botCommands.Add(new BotCommand
            {
                Command = "/sentToUser",
                CountArgsCommand = -1,
                ExampleCommand = "/sentToUser [Id user] [Message text]",
                NameOfLevel = LevelCommand.Messages,
                StatusUser = StatusUser.Admin,
                Execute = async (model, message) => await RepBotActions.Messages.AdminSentToUser(message, RepBotActions.ControlUsers.StrToInt(model.Args.FirstOrDefault()), model.Args)
            });

            botCommands.Add(new BotCommand
            {
                Command = "/userSpam",
                CountArgsCommand = 2,
                ExampleCommand = "/userSpam [Id user] [Count of messages]",
                NameOfLevel = LevelCommand.Messages,
                StatusUser = StatusUser.Admin,
                Execute = async (model, message) =>
                    await RepBotActions.Messages.UserSpamFromAdmin(message, RepBotActions.ControlUsers.StrToInt(model.Args.FirstOrDefault()), RepBotActions.ControlUsers.StrToInt(model.Args.LastOrDefault()))
            });

            botCommands.Add(new BotCommand
            {
                Command = "/start",
                ExampleCommand = "/start",
                LevelDependent = false,
                Execute = async (model, message) => await RepBotActions.ControlUsers.AuthNewUser(message, message.From.Id)
            });

            botCommands.Add(new BotCommand
            {
                Command = "/listOfUsers",
                ExampleCommand = "/listOfUsers",
                NameOfLevel = LevelCommand.ControlUsers,
                StatusUser = StatusUser.Admin,
                Execute = async (model, message) => await RepBotActions.ControlUsers.SendListOfUsers(message)
            });
            
            botCommands.Add(new BotCommand
            {
                Command = "/listOfUsers",
                ExampleCommand = "/listOfUsers",
                NameOfLevel = LevelCommand.Messages,
                StatusUser = StatusUser.Admin,
                Execute = async (model, message) => await RepBotActions.ControlUsers.SendListOfUsers(message)
            });

            botCommands.Add(new BotCommand
            {
                Command = "/add",
                CountArgsCommand = 1,
                ExampleCommand = "/add [Id user]",
                NameOfLevel = LevelCommand.ControlUsers,
                StatusUser = StatusUser.Admin,
                Execute = async (model, message) => await RepBotActions.ControlUsers.AuthNewUser(message, RepBotActions.ControlUsers.StrToInt(model.Args.FirstOrDefault()))
            });

            botCommands.Add(new BotCommand
            {
                Command = "/add",
                CountArgsCommand = 2,
                ExampleCommand = "/add [Id user] [Nickname]",
                NameOfLevel = LevelCommand.ControlUsers,
                StatusUser = StatusUser.Admin,
                Execute = async (model, message) => await RepBotActions.ControlUsers.AuthNewUser(message, RepBotActions.ControlUsers.StrToInt(model.Args.FirstOrDefault()), model.Args.LastOrDefault())
            });

            botCommands.Add(new BotCommand
            {
                Command = "/lock",
                CountArgsCommand = 1,
                ExampleCommand = "/lock [Id user]",
                NameOfLevel = LevelCommand.ControlUsers,
                StatusUser = StatusUser.Admin,
                Execute = async (model, message) =>
                {
                    if (int.TryParse(model.Args.FirstOrDefault(), out int IdUser))
                        await RepBotActions.ControlUsers.LockUser(message, IdUser);
                }
            });

            botCommands.Add(new BotCommand
            {
                Command = "/lock",
                CountArgsCommand = -1,
                ExampleCommand = "/lock [Nickname]",
                NameOfLevel = LevelCommand.ControlUsers,
                StatusUser = StatusUser.Admin,
                Execute = async (model, message) => await RepBotActions.ControlUsers.LockUser(message, model.Args)
            });

            botCommands.Add(new BotCommand
            {
                Command = "/deauth",
                CountArgsCommand = 1,
                ExampleCommand = "/deauth [Id user]",
                NameOfLevel = LevelCommand.ControlUsers,
                StatusUser = StatusUser.Admin,
                Execute = async (model, message) =>
                {
                    if (int.TryParse(model.Args.FirstOrDefault(), out int IdUser))
                        await RepBotActions.ControlUsers.DeauthUser(message, IdUser);
                }
            });

            botCommands.Add(new BotCommand
            {
                Command = "/deauth",
                CountArgsCommand = -1,
                ExampleCommand = "/deauth [Nickname]",
                NameOfLevel = LevelCommand.ControlUsers,
                StatusUser = StatusUser.Admin,
                Execute = async (model, message) => await RepBotActions.ControlUsers.DeauthUser(message, model.Args)
            });

            botCommands.Add(new BotCommand
            {
                Command = "/nickname",
                CountArgsCommand = -1,
                ExampleCommand = "/nickname [Id user] [New nickname]",
                NameOfLevel = LevelCommand.ControlUsers,
                StatusUser = StatusUser.Moderator,
                Execute = async (model, message) =>
                {
                    if (model.Args.Length >= 2)
                        await RepBotActions.ControlUsers.ChangeLocalName(message, RepBotActions.ControlUsers.StrToInt(model.Args.FirstOrDefault()), model.Args);
                    else
                        await RepBotActions.SendMessageWrongNumberOfArgs(message);
                }
            });

            botCommands.Add(new BotCommand
            {
                Command = "/saveChanges",
                ExampleCommand = "/saveChanges",
                NameOfLevel = LevelCommand.ControlUsers,
                StatusUser = StatusUser.Admin,
                Execute = async (model, message) => await RepBotActions.ControlUsers.SaveChanges(message)
            });

            botCommands.Add(new BotCommand
            {
                Command = "/cancelChanges",
                ExampleCommand = "/cancelChanges",
                NameOfLevel = LevelCommand.ControlUsers,
                StatusUser = StatusUser.Admin,
                Execute = async (model, message) => await RepBotActions.ControlUsers.CancelChanges(message)
            });

            botCommands.Add(new BotCommand
            {
                Command = "/stopBot",
                ExampleCommand = "/stopBot",
                NameOfLevel = LevelCommand.ControlBot,
                StatusUser = StatusUser.Admin,
                Execute = async (model, message) => await ExecuteLaunchBot.StopBotAsync()
            });

            botCommands.Add(new BotCommand
            {
                Command = "/stopApp",
                ExampleCommand = "/stopApp",
                NameOfLevel = LevelCommand.ControlBot,
                StatusUser = StatusUser.Admin,
                Execute = (model, message) => RepBotActions.ComStopApp()
            });

            botCommands.Add(new BotCommand
            {
                Command = "/reloadBot",
                ExampleCommand = "/reloadBot",
                NameOfLevel = LevelCommand.ControlBot,
                StatusUser = StatusUser.Admin,
                Execute = async (model, message) => await ExecuteLaunchBot.RestartBotAsync()
            });

            botCommands.Add(new BotCommand
            {
                Command = "/url",
                CountArgsCommand = 1,
                ExampleCommand = "/url [url:]",
                NameOfLevel = LevelCommand.ControlPC,
                Execute = async (model, message) => await RepBotActions.ControlPC.ExecuteUrl(message, model.Args.FirstOrDefault())
            });

            botCommands.Add(new BotCommand
            {
                Command = "/turnOff",
                CountArgsCommand = 2,
                ExampleCommand = "/turnOff [tMin: int] [tSec: int]",
                NameOfLevel = LevelCommand.ControlPC,
                StatusUser = StatusUser.Admin,
                Execute = async (model, message) =>
                    await RepBotActions.ControlPC.TurnOff(message, RepBotActions.ControlUsers.StrToInt(model.Args.FirstOrDefault()), RepBotActions.ControlUsers.StrToInt(model.Args.LastOrDefault()))
            });

            botCommands.Add(new BotCommand
            {
                Command = "/cancelOff",
                ExampleCommand = "/cancelOff",
                NameOfLevel = LevelCommand.ControlPC,
                StatusUser = StatusUser.Admin,
                Execute = async (model, message) =>
                {
                    if (RepBotActions.ControlPC.ExecuteCmdCommand(@"C:\Windows\System32\shutdown.exe", "/a"))
                        await App.Api.SendTextMessageAsync(message.From.Id, "Успешно выполнено снятие таймера на выключение");
                    else
                        await RepUsers.SendMessage(message.From.Id, "При снятии таймера, произошла системная ошибка");
                }
            });

            botCommands.Add(new BotCommand
            {
                Command = "/lockSys",
                ExampleCommand = "/lockSys",
                NameOfLevel = LevelCommand.ControlPC,
                Execute = async (model, message) =>
                {
                    if (RepBotActions.ControlPC.ExecuteCmdCommand(@"C:\Windows\System32\rundll32.exe", "USER32.DLL LockWorkStation"))
                        await RepUsers.SendMessage(message.From.Id, "Успешно заблокирована система");
                    else
                        await RepUsers.SendMessage(message.From.Id, "При блокировке системы, произошла системная ошибка");
                }
            });

            botCommands.Add(new BotCommand
            {
                Command = "/getScreen",
                ExampleCommand = "/getScreen",
                NameOfLevel = LevelCommand.ControlPC,
                Execute = async (model, message) => await RepBotActions.ControlPC.GetScreenshot(message)
            });

            botCommands.Add(new BotCommand
            {
                Command = "/hiberSys",
                ExampleCommand = "/hiberSys",
                NameOfLevel = LevelCommand.ControlPC,
                Execute = async (model, message) =>
                {
                    if (RepBotActions.ControlPC.ExecuteCmdCommand(@"C:\Windows\System32\shutdown.exe", "/h"))
                        await RepUsers.SendMessage(message.From.Id, "Успешно выполнен перевод в гибернацию");
                    else
                        await RepUsers.SendMessage(message.From.Id, "При переводе в гибернацию, произошла системная ошибка");
                }
            });

            botCommands.Add(new BotCommand
            {
                Command = "/volume",
                CountArgsCommand = 1,
                ExampleCommand = "/volume [int [-100..100]; mute]",
                NameOfLevel = LevelCommand.ControlPC,
                Execute = async (model, message) => await RepBotActions.ControlPC.ContolVolume.changeVolume(message, model.Args.FirstOrDefault())
            });

            botCommands.Add(new BotCommand
            {
                Command = "/keyboard",
                CountArgsCommand = 1,
                ExampleCommand = "/keyboard [true; false; all]",
                Execute = async (model, message) => await RepBotActions.ShowScreenButtons(message, model.Args.FirstOrDefault())
            });

            Sort();
        }
    }
}
