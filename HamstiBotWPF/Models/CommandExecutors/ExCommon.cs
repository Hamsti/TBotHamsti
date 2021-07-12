using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TBotHamsti.Models.Commands;
using TBotHamsti.Models.Users;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using User = TBotHamsti.Models.Users.User;

namespace TBotHamsti.Models.CommandExecutors
{
    /// <summary>
    /// Implementation of all bot functionality
    /// </summary>
    public static class ExCommon
    {
        public static string GetHelp(User user) => string.Join("\n", BotLevelCommand.GetBotLevelCommand(user).CommandsOfLevel.Where(w => w.StatusUser <= user.Status).Select(s => s.ExampleCommand));

        public static Task HelpBot(User user) => user.SendMessageAsync("Список команд у бота:\n" + GetHelp(user));

        private static Exception ExceptionWrongNumberOfArgs => new ArgumentOutOfRangeException("Не верное кол-во агрументов\nСписок комманд: /help");
        public static string GetOriginalArgs(ICommand model, Message message, int skipArgs = 0)
        {
            if (model.Args.Length > skipArgs)
                return message.Text.Substring(message.Text.LastIndexOf(model.Args[skipArgs]));
            else
                throw ExceptionWrongNumberOfArgs;
        }

        public static async Task ImageUploader(User user, Message message)
        {
            try
            {
                var file = await App.Api.GetFileAsync(message.Photo.LastOrDefault()?.FileId);
                var filename = file.FileId + "." + file.FilePath.Split('.').Last();
                using (var saveImageStream = System.IO.File.Open(Properties.Settings.Default.SavePath + @"photos\" + filename, FileMode.Create))
                {
                    await App.Api.DownloadFileAsync(file.FilePath, saveImageStream);
                }
                await user.SendMessageAsync("Загрузка изображения успешно завершена");
            }
            catch (Exception ex)
            {
                await user.SendMessageAsync($"При загрузке изображения произошла ошибка: {ex.Message}");
            }
        }

        public static async Task DocumentUploader(User user, Message message)
        {
            try
            {
                var file = await App.Api.GetFileAsync(message.Document.FileId);
                using (var saveImageStream = System.IO.File.Open(Properties.Settings.Default.SavePath + @"documents\" + message.Document.FileName, FileMode.Create))
                {
                    await App.Api.DownloadFileAsync(file.FilePath, saveImageStream);
                }
                await user.SendMessageAsync("Загрузка документа успешно завершена");
            }
            catch (Exception ex)
            {
                await user.SendMessageAsync($"При загрузке документа произошла ошибка: {ex.Message}");
            }
        }

        public static async Task ComStopApp(User user)
        {
            await user.SendMessageAsync($"Принудительное завершение работы приложения пользователем: " + user.IdUser_Nickname);
            await ExecutionBot.StopBotAsync();
            Application.Current.Dispatcher.BeginInvokeShutdown(System.Windows.Threading.DispatcherPriority.Background);
        }

        public static Task ShowScreenButtons(User user, string arg)
        {
            ParserKeys(out string[] keys, arg, user);

            if (keys == null)
                return user.SendMessageAsync("Не найден аргумент для выполнения команды");

            int countColsKeys = 3;
            var rkm = new ReplyKeyboardMarkup();
            var rows = new List<KeyboardButton[]>();
            var cols = new List<KeyboardButton>();

            for (var i = 0; i < keys.Count(); i++)
            {
                cols.Add(new KeyboardButton(keys[i]));
                if ((i + 1) % countColsKeys != 0 && i + 1 != keys.Count()) continue;
                rows.Add(cols.ToArray());
                cols = new List<KeyboardButton>();
            }
            rkm.Keyboard = rows.ToArray();

            rkm.OneTimeKeyboard = true;

            //if (keys.Count() == 0)
            //    return App.Api.SendTextMessageAsync(message.Chat.Id, "Экранная клавиатура успешно удалена.", replyMarkup: new ReplyKeyboardRemove());
            //else
            //    return App.Api.SendTextMessageAsync(message.Chat.Id, $"Количество добавленных кнопок: {keys.Count()}", replyMarkup: rkm);
            if (keys.Count() == 0)
                return App.Api.SendTextMessageAsync(user.Id, "Экранная клавиатура успешно удалена.", replyMarkup: new ReplyKeyboardRemove());
            else
                return App.Api.SendTextMessageAsync(user.Id, $"Количество добавленных кнопок: {keys.Count()}", replyMarkup: rkm);
        }

        private static void ParserKeys(out string[] keys, string arg, User user)
        {
            if (bool.TryParse(arg, out bool isShowKeys))
                keys = isShowKeys ? CollectionCommands.RootLevel.CommandsOfLevel.Where(x => x.CountArgsCommand == 0 && x.StatusUser <= user.Status).Select(s => s.Command).ToArray() : new string[0];
            else
                keys = arg.ToLower() == "all" ? CollectionCommands.RootLevel.CommandsOfLevel.Where(w => w.StatusUser <= user.Status).Select(s => s.Command).ToArray() : null;
        }
    }
}