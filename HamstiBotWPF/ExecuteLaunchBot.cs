using System;
using System.Threading.Tasks;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using HamstiBotWPF.Core;
using HamstiBotWPF.LogicRepository;

namespace HamstiBotWPF
{
    /// <summary>
    /// To control the bot (reading messages, start, stop, reload)
    /// </summary>
    public static class ExecuteLaunchBot
    {
        /// <summary>
        /// Listening to all incoming messages
        /// </summary>
        public static async void CheckMessageBot(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            //Check message for null
            if (message == null) return;

            //User authorization check
            if (RepUsers.IsAuthNotIsBlockedUser(message.From.Id))
            {
                switch (message.Type)
                {
                    //Text message received from user
                    case MessageType.Text:
                        //Parsing and executing a command or errors
                        var model = BotCommand.ParserCommand(message.Text);

                        if (model == null || !ExecCommand(model, message)) //Execute if command not found
                        {
                            await GlobalUnit.Api.SendTextMessageAsync(message.From.Id, $"Команда \"{message.Text}\" не была найдена\nДля просмотра списка команд введите /help");
                            return;
                        }
                        break;
                    //Image received from user
                    case MessageType.Photo:
                        RepBotActions.ImageUploader(message); break;
                    //Document received from user
                    case MessageType.Document:
                        RepBotActions.DocumentUploader(message); break;
                    default:
                        await GlobalUnit.Api.SendTextMessageAsync(message.From.Id, $"На данный момент, \"{message.Type.ToString()}\" - неизвестный тип сообщения."); break;
                }
            }
            //User was not found or IsBlocked in the list of authorized users
            else
            {
                if (RepUsers.IsAuthUser(message.From.Id))
                    await GlobalUnit.Api.SendTextMessageAsync(message.From.Id, $"На данный момент вы заблокированы. Запросите у администратора бота {GlobalUnit.Api.GetMeAsync().Result} вас добавить в список разрешённых пользователей.\n\nВы можете написать администратору бота используя команду \"/messageToAdmin YourMessage\"");
                else
                    await RepBotActions.ControlUsers.AuthNewUser(message, message.From.Id);
            }
        }

        private static bool ExecCommand(BotCommandStructure model, Telegram.Bot.Types.Message message)
        {
            bool IsBotLevelCommand(BotCommand levelCommand) => levelCommand.GetType().Equals(typeof(BotLevelCommand));
            bool isCommand = false;
            int CountCurrentCommand = GlobalUnit.botCommands.FindAll(m => m.Command.Equals(model.Command)).Count;
            int CountCurrentCommand2 = CountCurrentCommand;

            foreach (var command in GlobalUnit.botCommands)
            {
                if (command.Command == model.Command)
                {
                    isCommand = true;

                    if (IsBotLevelCommand(command) && ((BotLevelCommand)command).ParrentLevel == GlobalUnit.currentLevelCommand ||
                        command.NameOfLevel == GlobalUnit.currentLevelCommand || !command.LevelDependent)
                    {
                        if (command.CountArgsCommand == model.Args.Length ||
                            command.CountArgsCommand == -1 && model.Args.Length > 0)
                        {
                            if (command.VisibleForUsers || !command.VisibleForUsers && RepUsers.IsHaveAccessAdmin(message.From.Id))
                            {
                                if (IsBotLevelCommand(command))
                                    ((BotLevelCommand)command).Execute?.Invoke(model, message);
                                else 
                                    command.Execute?.Invoke(model, message);
                            }
                            else
                                GlobalUnit.Api.SendTextMessageAsync(message.From.Id, $"Для выполнения команды \"{model.Command}\", необходимы права администратора.");
                        }
                        else if (--CountCurrentCommand < 1)
                        {
                            if (IsBotLevelCommand(command))
                                ((BotLevelCommand)command).OnError.Invoke(model, message);
                            else
                                command.OnError?.Invoke(model, message);
                        }
                    }
                    else if (--CountCurrentCommand2 < 1)
                        GlobalUnit.Api.SendTextMessageAsync(message.From.Id, $"Текущий уровень \"{GlobalUnit.currentLevelCommand}\". \nЗапрашиваемая комманда находится на уровне \"{command.NameOfLevel}\"");
                }
            }
            return isCommand;
        }

        /// <summary>
        /// To launch this bot
        /// </summary>
        /// <param name="Attempt">The number of attempts to launch the bot</param>
        public async static Task RunBotAsync(int numberAttempt = 1)
        {
            if (numberAttempt <= 0)
            {
                await GlobalUnit.Api.SendTextMessageAsync(Properties.Settings.Default.AdminId, $"Бот {GlobalUnit.Api.GetMeAsync().Result} не удалось запустить...");
                return;
            }

            if (!GlobalUnit.Api.IsReceiving)
            {
                GlobalUnit.Api.StartReceiving(Array.Empty<UpdateType>());
                await GlobalUnit.Api.SendTextMessageAsync(Properties.Settings.Default.AdminId, $"Запущен бот {GlobalUnit.Api.GetMeAsync().Result} пользователем: {Environment.UserDomainName}");
            }
            else
            {
                await GlobalUnit.Api.SendTextMessageAsync(Properties.Settings.Default.AdminId, $"Бот {GlobalUnit.Api.GetMeAsync().Result} уже запущен.");
                await RunBotAsync(--numberAttempt);
            }
        }

        /// <summary>
        /// To stop this bot
        /// </summary>
        /// <param name="Attempt">The number of attempts to stop the bot</param>
        public async static Task StopBotAsync(int numberAttempt = 1)
        {
            if (numberAttempt <= 0)
            {
                await GlobalUnit.Api.SendTextMessageAsync(Properties.Settings.Default.AdminId, $"Бот {GlobalUnit.Api.GetMeAsync().Result} не удалось остановить...");
                return;
            }
            if (GlobalUnit.Api.IsReceiving)
            {
                await GlobalUnit.Api.SendTextMessageAsync(Properties.Settings.Default.AdminId, $"Бот {GlobalUnit.Api.GetMeAsync().Result} успешно остановлен пользователем: {Environment.UserDomainName}");
                GlobalUnit.Api.StopReceiving();
            }
            else
            {
                await GlobalUnit.Api.SendTextMessageAsync(Properties.Settings.Default.AdminId, $"Бот {GlobalUnit.Api.GetMeAsync().Result} уже остановлен");
                await StopBotAsync(--numberAttempt);
            }
        }

        /// <summary>
        /// To reload this bot
        /// </summary>
        /// <param name="numberAttempt">The number of attempts to reload the bot</param>
        public async static Task RestartBotAsync(int numberAttempt = 2)
        {
            await GlobalUnit.Api.SendTextMessageAsync(Properties.Settings.Default.AdminId, $"Бот {GlobalUnit.Api.GetMeAsync().Result} выполняет перезагрузку...");

            if (GlobalUnit.Api.IsReceiving)
            {
                await StopBotAsync(numberAttempt);
                await RunBotAsync(numberAttempt);
            }
            else
                await RunBotAsync(numberAttempt);
        }
    }
}
