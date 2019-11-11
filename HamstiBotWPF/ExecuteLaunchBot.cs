using System;
using System.Threading.Tasks;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

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
        public static async void checkMessageBot(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            //Check message for null
            if (message == null) return;

            //User authorization check
            if (LogicRepository.RepUsers.isAuthNotBlockedUser(message.From.Id))
            {
                switch (message.Type)
                {
                    //Text message received from user
                    case MessageType.Text:
                        //Parsing and executing a command or errors
                        var model = Core.BotCommand.ParserCommand(message.Text);

                        if (model == null || !ExecCommand(model, message)) //Execute if command not found
                        {
                            await GlobalUnit.Api.SendTextMessageAsync(message.From.Id, $"Команда \"{message.Text}\" не была найдена\nДля просмотра списка команд введите /help");
                            return;
                        }
                        break;
                    //Image received from user
                    case MessageType.Photo:
                        LogicRepository.RepBotActions.imageUploader(message); break;
                    //Document received from user
                    case MessageType.Document:
                        LogicRepository.RepBotActions.documentUploader(message); break;
                    default:
                        await GlobalUnit.Api.SendTextMessageAsync(message.From.Id, "На данный момент, неизвестный тип сообщения."); break;
                }
            }
            //User was not found or blocked in the list of authorized users
            else
            {
                if (LogicRepository.RepUsers.isAuthUser(message.From.Id))
                {
                    await GlobalUnit.Api.SendTextMessageAsync(message.From.Id, $"На данный момент вы заблокированы. Запросите у администратора бота {GlobalUnit.Api.GetMeAsync().Result} вас добавить в список разрешённых пользователей.\n\nВы можете написать администратору бота используя команду \"/messageToAdmin YourMessage\"");
                }
                else
                {
                    await GlobalUnit.Api.SendTextMessageAsync(message.From.Id, $"Вы были успешно добавлены в список пользователей бота.\nЗапросите у администратора бота {GlobalUnit.Api.GetMeAsync().Result} вас добавить в список разрешённых пользователей.\n\nВы можете написать администратору бота используя команду \"/messageToAdmin YourMessage\"");
                    LogicRepository.RepBotActions.ControlUsers.authNewUser(message, message.From.Id);
                }
            }
        }

        private static bool ExecCommand(Core.BotCommandStructure model, Telegram.Bot.Types.Message message)
        {
            bool isCommand = false;
            int CountCurrentCommand = GlobalUnit.botCommands.FindAll(m => m.Command.ToLower().Equals(model.Command.ToLower())).Count;

            foreach (var command in GlobalUnit.botCommands)
            {
                if (command.Command == model.Command || command.Command.ToLower() == model.Command.ToLower() || 
                    command.GetType().Equals(typeof(Core.BotLevelCommand)) && model.Command == Core.BotLevelCommand.TOPREVLEVEL && GlobalUnit.currentLevelCommand == command.NameOfLevel)
                {
                    isCommand = true;

                    if (command.GetType().Equals(typeof(Core.BotLevelCommand)) && ((Core.BotLevelCommand)command).ParrentLevel == GlobalUnit.currentLevelCommand ||
                        command.NameOfLevel == GlobalUnit.currentLevelCommand)
                    {
                        if (command.CountArgsCommand == model.Args.Length ||
                            command.CountArgsCommand == -1 && model.Args.Length > 0)
                        {
                            if (command.VisibleForUsers || !command.VisibleForUsers && LogicRepository.RepUsers.isHaveAccessAdmin(message.From.Id))
                            {
                                if (command.GetType().Equals(typeof(Core.BotCommand)))
                                    command.Execute?.Invoke(model, message);
                                else
                                    ((Core.BotLevelCommand)command).Execute?.Invoke(model, message);
                            }
                            else
                                GlobalUnit.Api.SendTextMessageAsync(message.From.Id, $"Для выполнения команды {model.Command}, необходимы права администратора.");
                        }
                        else if (--CountCurrentCommand < 1)
                        {
                            if (command.GetType().Equals(typeof(Core.BotCommand)))
                                command.OnError?.Invoke(model, message);
                            else
                                ((Core.BotLevelCommand)command).OnError.Invoke(model, message);
                        }
                    }
                    else
                        GlobalUnit.Api.SendTextMessageAsync(message.From.Id, "Вы находитесь не на том уровне");
                }
            }
            return isCommand;
        }

        /// <summary>
        /// To launch this bot
        /// </summary>
        /// <param name="Attempt">The number of attempts to launch the bot</param>
        public async static Task runBot(int numberAttempt = 1)
        {
            if (numberAttempt <= 0)
            {
                await GlobalUnit.Api.SendTextMessageAsync(Properties.Settings.Default.AdminId, $"Бот {GlobalUnit.Api.GetMeAsync().Result} не удалось запустить...");
                return;
            }

            if (!GlobalUnit.Api.IsReceiving)
            {
                await GlobalUnit.Api.SendTextMessageAsync(Properties.Settings.Default.AdminId, $"Запущен бот {GlobalUnit.Api.GetMeAsync().Result} пользователем: {Environment.UserDomainName}");
                GlobalUnit.Api.StartReceiving(Array.Empty<UpdateType>());
            }
            else
            {
                await GlobalUnit.Api.SendTextMessageAsync(Properties.Settings.Default.AdminId, $"Бот {GlobalUnit.Api.GetMeAsync().Result} уже запущен.");
                await runBot(--numberAttempt);
            }
        }

        /// <summary>
        /// To stop this bot
        /// </summary>
        /// <param name="Attempt">The number of attempts to stop the bot</param>
        /// <returns></returns>
        public async static Task stopBot(int numberAttempt = 1)
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
                await stopBot(--numberAttempt);
            }
        }

        /// <summary>
        /// To reload this bot
        /// </summary>
        /// <param name="numberAttempt">The number of attempts to reload the bot</param>
        /// <returns></returns>
        public async static Task reloadBot(int numberAttempt = 2)
        {
            await GlobalUnit.Api.SendTextMessageAsync(Properties.Settings.Default.AdminId, $"Бот {GlobalUnit.Api.GetMeAsync().Result} выполняет перезагрузку...");

            if (GlobalUnit.Api.IsReceiving)
            {
                await stopBot(numberAttempt);
                await runBot(numberAttempt);
            }
            else
            {
                await runBot(numberAttempt);
            }

        }
    }
}
