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
            bool isCommand = false;
            //Check message for null
            if (message == null) return;

            //User authorization check
            if (LogicRepository.RepUsers.isAuthUser(message.From.Id))
            {
                switch (message.Type)
                {
                    //Text message received from user
                    case MessageType.Text:
                        //Parsing and executing a command or errors
                        var model = Core.BotCommand.ParserCommand(message.Text);

                        if (model != null)
                        {
                            foreach (var commandList in GlobalUnit.botCommands)
                            {
                                if (commandList.Command == model.Command)
                                {
                                    isCommand = true;
                                    if (commandList.CountArgsCommand == model.Args.Length)
                                    {
                                        commandList.Execute?.Invoke(model, message);
                                    }
                                    else
                                    {
                                        commandList.OnError?.Invoke(model, message);
                                    }
                                }
                            }
                        }
                        if (model == null || !isCommand) //Execute if command not found
                        {
                            await GlobalUnit.myBot.Api.SendTextMessageAsync(message.From.Id, $"Команда \"{message.Text}\" не была найдена\nДля просмотра списка команд введите /help");
                            return;
                        }
                        break;
                    //Image received from user
                    case MessageType.Photo:
                        LogicRepository.RepBotActions.imageUploader(message); break;
                    //Document received from user
                    case MessageType.Document:
                        LogicRepository.RepBotActions.documentUploader(message); break;
                }
            }
            //User was not found or locked in the list of authorized users
            else
            {
                await GlobalUnit.myBot.Api.SendTextMessageAsync(message.From.Id, $"Запросите у администратора бота {GlobalUnit.myBot.Api.GetMeAsync().Result} вас добавить в список разрешённых пользователей");
            }
        }

        /// <summary>
        /// To launch this bot
        /// </summary>
        /// <param name="Attempt">The number of attempts to launch the bot</param>
        public async static Task runBot(int numberAttempt = 1)
        {
            if (numberAttempt <= 0)
            {
                await GlobalUnit.myBot.Api.SendTextMessageAsync(Properties.Settings.Default.AdminId, $"Бот {GlobalUnit.myBot.Api.GetMeAsync().Result} не удалось запустить...");
                return;
            }

            if (!GlobalUnit.myBot.Api.IsReceiving)
            {
                await GlobalUnit.myBot.Api.SendTextMessageAsync(Properties.Settings.Default.AdminId, $"Запущен бот {GlobalUnit.myBot.Api.GetMeAsync().Result} пользователем: {Environment.UserDomainName}");
                GlobalUnit.myBot.Api.StartReceiving(Array.Empty<UpdateType>());
            }
            else
            {
                await GlobalUnit.myBot.Api.SendTextMessageAsync(Properties.Settings.Default.AdminId, $"Бот {GlobalUnit.myBot.Api.GetMeAsync().Result} уже запущен.");
                numberAttempt--;
                await runBot(numberAttempt);
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
                await GlobalUnit.myBot.Api.SendTextMessageAsync(Properties.Settings.Default.AdminId, $"Бот {GlobalUnit.myBot.Api.GetMeAsync().Result} не удалось остановить...");
                return;
            }
            if (GlobalUnit.myBot.Api.IsReceiving)
            {
                await GlobalUnit.myBot.Api.SendTextMessageAsync(Properties.Settings.Default.AdminId, $"Бот {GlobalUnit.myBot.Api.GetMeAsync().Result} успешно остановлен пользователем: {Environment.UserDomainName}");
                GlobalUnit.myBot.Api.StopReceiving();
            }
            else
            {
                await GlobalUnit.myBot.Api.SendTextMessageAsync(Properties.Settings.Default.AdminId, $"Бот {GlobalUnit.myBot.Api.GetMeAsync().Result} уже остановлен");
                numberAttempt--;
                await stopBot(numberAttempt);
            }
        }

        /// <summary>
        /// To reload this bot
        /// </summary>
        /// <param name="numberAttempt">The number of attempts to reload the bot</param>
        /// <returns></returns>
        public async static Task reloadBot(int numberAttempt = 2)
        {
            await GlobalUnit.myBot.Api.SendTextMessageAsync(Properties.Settings.Default.AdminId, $"Бот {GlobalUnit.myBot.Api.GetMeAsync().Result} выполняет перезагрузку...");

            if (GlobalUnit.myBot.Api.IsReceiving)
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
