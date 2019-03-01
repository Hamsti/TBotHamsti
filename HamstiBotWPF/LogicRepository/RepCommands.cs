using System.Linq;
using System.Threading.Tasks;

namespace HamstiBotWPF.LogicRepository
{
    /// <summary>
    /// To work with a list of commands
    /// </summary>
    public static class RepCommands
    {
        /// <summary>
        /// Add all commands to the command list
        /// </summary>
        public static void AddAllCommands()
        {
            GlobalUnit.botCommands.Add(new Core.BotCommand
            {
                Command = "/start",
                CountArgsCommand = 0,
                ExampleCommand = "/start",
                VisibleCommand = false,
                Execute = async (model, message) =>
                {
                    await RepBotActions.helpBot(message);
                }
            });

            GlobalUnit.botCommands.Add(new Core.BotCommand
            {
                Command = "/help",
                CountArgsCommand = 0,
                ExampleCommand = "/help",
                Execute = async (model, message) =>
                {
                    await RepBotActions.helpBot(message);
                }
            });

            GlobalUnit.botCommands.Add(new Core.BotCommand
            {
                Command = "/helpAdmin",
                CountArgsCommand = 0,
                ExampleCommand = "/helpAdmin",
                VisibleCommand = false,
                Execute = async(model,message) =>
                {
                    await RepBotActions.helpBotAdmin(message);
                }
            });

            GlobalUnit.botCommands.Add(new Core.BotCommand
            {
                Command = "/stopBot",
                CountArgsCommand = 0,
                ExampleCommand = "/stopBot",
                VisibleCommand = false,
                Execute = (model, message) =>
                {
                    RepBotActions.comStopBot(message);
                }
            });

            GlobalUnit.botCommands.Add(new Core.BotCommand
            {
                Command = "/stopApp",
                CountArgsCommand = 0,
                ExampleCommand = "/stopApp",
                VisibleCommand = false,
                Execute = (model, message) =>
                {
                    RepBotActions.comStopApp(message);
                }
            });

            GlobalUnit.botCommands.Add(new Core.BotCommand
            {
                Command = "/reloadBot",
                CountArgsCommand = 0,
                ExampleCommand = "/reloadBot",
                VisibleCommand = false,
                Execute = (model, message) =>
                {
                    Task.Run(() => ExecuteLaunchBot.reloadBot());
                }
            });

            GlobalUnit.botCommands.Add(new Core.BotCommand
            {
                Command = "/url",
                CountArgsCommand = 1,
                ExampleCommand = "/url [url:]",
                Execute = (model, message) =>
                {
                    RepBotActions.ControlPC.executeUrl(message, model.Command, model.Args.FirstOrDefault());
                }
            });

            GlobalUnit.botCommands.Add(new Core.BotCommand
            {
                Command = "/turnOff",
                CountArgsCommand = 2,
                ExampleCommand = "/turnOff [tMin: int, tSec: int]",
                VisibleCommand = false,
                Execute = (model, message) =>
                {
                    RepBotActions.ControlPC.turnOff(message, int.Parse(model.Args.FirstOrDefault()), int.Parse(model.Args.LastOrDefault()));
                }
            });

            GlobalUnit.botCommands.Add(new Core.BotCommand
            {
                Command = "/cancelOff",
                CountArgsCommand = 0,
                ExampleCommand = "/cancelOff",
                VisibleCommand = false,
                Execute = (model, message) =>
                {
                    if (RepBotActions.ControlPC.cmdCommands(message, @"C:\Windows\System32\shutdown.exe", "/a"))
                        GlobalUnit.myBot.Api.SendTextMessageAsync(message.From.Id, "Успешно выполнено снятие таймера на выключение");
                    else
                        GlobalUnit.myBot.Api.SendTextMessageAsync(message.From.Id, "При снятии таймера, произошла системная ошибка");
                }
            });

            GlobalUnit.botCommands.Add(new Core.BotCommand
            {
                Command = "/lockSystem",
                CountArgsCommand = 0,
                ExampleCommand = "/lockSystem",
                Execute = (model, message) =>
                {
                    if (RepBotActions.ControlPC.cmdCommands(message, @"C:\Windows\System32\rundll32.exe", "USER32.DLL LockWorkStation"))
                        GlobalUnit.myBot.Api.SendTextMessageAsync(message.From.Id, "Успешно заблокирована система");
                    else
                        GlobalUnit.myBot.Api.SendTextMessageAsync(message.From.Id, "При блокировке системы, произошла системная ошибка");
                }
            });

            GlobalUnit.botCommands.Add(new Core.BotCommand
            {
                Command = "/getScreen",
                CountArgsCommand = 0,
                ExampleCommand = "/getScreen",
                Execute = (model, message) =>
                {
                    RepBotActions.ControlPC.getScreenshot(message);
                }
            });

            GlobalUnit.botCommands.Add(new Core.BotCommand
            {
                Command = "/hiberSystem",
                CountArgsCommand = 0,
                ExampleCommand = "/hiberSystem",
                Execute = (model, message) =>
                {
                    if (RepBotActions.ControlPC.cmdCommands(message, @"C:\Windows\System32\shutdown.exe", "/h"))
                        GlobalUnit.myBot.Api.SendTextMessageAsync(message.From.Id, "Успешно выполнен перевод в гибернацию");
                    else
                        GlobalUnit.myBot.Api.SendTextMessageAsync(message.From.Id, "При переводе в гибернацию, произошла системная ошибка");
                }
            });

            GlobalUnit.botCommands.Add(new Core.BotCommand
            {
                Command = "/volume",
                CountArgsCommand = 1,
                ExampleCommand = "/volume [int value[-100..100, mute]]",
                Execute = (model, message) =>
                {
                    RepBotActions.ControlPC.ContolVolume.changeVolume(message, model.Args.FirstOrDefault());
                }
            });

            GlobalUnit.botCommands.Add(new Core.BotCommand
            {
                Command = "/keyboard",
                CountArgsCommand = 1,
                ExampleCommand = "/keyboard [true;false;all]",
                Execute = (model, message) =>
                {
                    RepBotActions.showScreenButtons(message, model.Args.FirstOrDefault()); 
                }
            });
        }
    }
}
