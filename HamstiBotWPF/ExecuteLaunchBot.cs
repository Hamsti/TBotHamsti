using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using HamstiBotWPF.Core;
using HamstiBotWPF.LogicRepository;
using StatusUser = HamstiBotWPF.LogicRepository.RepUsers.StatusUser;

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

                        if (model == null || !ExecCommand(model, message).Result) //Execute if command not found
                        {
                            await RepUsers.SendMessage(message.From.Id, $"Команда \"{message.Text}\" не была найдена\nДля просмотра списка команд введите /help");
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
                        await RepUsers.SendMessage(message.From.Id, $"На данный момент, \"{message.Type}\" - неизвестный тип сообщения."); break;
                }
            }
            //User was not found or IsBlocked in the list of authorized users
            else
            {
                if (RepUsers.IsAuthUser(message.From.Id))
                    await RepUsers.SendMessage(message.From.Id, $"На данный момент вы заблокированы. Запросите у администратора бота {App.Api.GetMeAsync().Result} вас добавить в список разрешённых пользователей.\n\nВы можете написать администратору бота используя команду \"/messageToAdmin YourMessage\"");
                else
                    await RepBotActions.ControlUsers.AuthNewUser(message, message.From.Id);
            }
        }

        private static async Task<bool> ExecCommand(BotCommandStructure model, Telegram.Bot.Types.Message message)
        {
            bool IsBotLevelCommand(BotCommand levelCommand) => levelCommand.GetType().Equals(typeof(BotLevelCommand));
            bool isCommand = false;
            int countCurrentCommand = RepCommands.botCommands.FindAll(m => m.Command.Equals(model.Command)).Count;
            int countCurrentCommand2 = countCurrentCommand;

            foreach (var command in RepCommands.botCommands)
            {
                if (command.Command == model.Command)
                {
                    isCommand = true;

                    if (IsBotLevelCommand(command) && ((BotLevelCommand)command).ParrentLevel == RepCommands.currentLevelCommand ||
                        command.NameOfLevel == RepCommands.currentLevelCommand || !command.LevelDependent)
                    {
                        if (command.CountArgsCommand == model.Args.Length ||
                            command.CountArgsCommand == -1 && model.Args.Length > 0)
                        {
                            if (command.StatusUser <= RepUsers.GetStatusUser(message.From.Id))
                            {
                                if (IsBotLevelCommand(command))
                                    ((BotLevelCommand)command).Execute?.Invoke(model, message);
                                else 
                                    command.Execute?.Invoke(model, message);
                            }
                            else
                                await RepUsers.SendMessage(message.From.Id, $"Для выполнения команды \"{model.Command}\", необходим статус \"{command.StatusUser}\"{(Enum.GetValues(typeof(StatusUser)).Cast<int>().Max() != (int)command.StatusUser ? " и выше" : string.Empty)}.");
                        }
                        else if (--countCurrentCommand < 1)
                        {
                            if (IsBotLevelCommand(command))
                                ((BotLevelCommand)command).OnError.Invoke(model, message);
                            else
                                command.OnError?.Invoke(model, message);
                        }
                    }
                    else if (--countCurrentCommand2 < 1)
                        await RepUsers.SendMessage(message.From.Id, $"Текущий уровень \"{RepCommands.currentLevelCommand}\". \nЗапрашиваемая комманда находится на уровне \"{command.NameOfLevel}\"");
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
                await RepUsers.SendMessage($"Бот {App.Api.GetMeAsync().Result} не удалось запустить...");
                return;
            }

            if (!App.Api.IsReceiving)
            {
                App.Api.StartReceiving(Array.Empty<UpdateType>());
                await RepUsers.SendMessage($"Запущен бот {App.Api.GetMeAsync().Result} пользователем: {Environment.UserDomainName}");
            }
            else
            {
                await RepUsers.SendMessage($"Бот {App.Api.GetMeAsync().Result} уже запущен.");
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
                await RepUsers.SendMessage($"Бот {App.Api.GetMeAsync().Result} не удалось остановить...");
                return;
            }
            if (App.Api.IsReceiving)
            {
                await RepUsers.SendMessage($"Бот {App.Api.GetMeAsync().Result} успешно остановлен пользователем: {Environment.UserDomainName}");
                App.Api.StopReceiving();
            }
            else
            {
                await RepUsers.SendMessage($"Бот {App.Api.GetMeAsync().Result} уже остановлен");
                await StopBotAsync(--numberAttempt);
            }
        }

        /// <summary>
        /// To reload this bot
        /// </summary>
        /// <param name="numberAttempt">The number of attempts to reload the bot</param>
        public static async Task RestartBotAsync(int numberAttempt = 1)
        {
            await RepUsers.SendMessage($"Бот {App.Api.GetMeAsync().Result} выполняет перезагрузку...");

            if (App.Api.IsReceiving)
            {
                await StopBotAsync(numberAttempt);
                await RunBotAsync(numberAttempt);
            }
            else
                await RunBotAsync(numberAttempt);
        }
    }
}
