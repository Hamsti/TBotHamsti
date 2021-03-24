using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TBotHamsti.Core;
using LevelCommand = TBotHamsti.Core.BotLevelCommand.LevelCommand;
using StatusUser = TBotHamsti.LogicRepository.RepUsers.StatusUser;

namespace TBotHamsti.LogicRepository
{
    /// <summary>
    /// Implementation of all bot functionality
    /// </summary>
    public static class RepBotActions
    {
        public static string GetHelp(int idUser) => (RepCommands.currentLevelCommand != LevelCommand.Root ? BotLevelCommand.TOPREVLEVEL.ToUpper() + "\n" : string.Empty) +
            string.Join("\n", RepCommands.botCommands.Where(w => (w.StatusUser <= RepUsers.GetStatusUser(idUser) && w.Command != BotLevelCommand.TOPREVLEVEL) &&
            RepCommands.currentLevelCommand == (w.GetType().Equals(typeof(BotLevelCommand)) ? ((BotLevelCommand)w).ParrentLevel : w.NameOfLevel)).
            Select(s => s.ExampleCommand ?? ((BotLevelCommand)s).ExampleCommand));

        public static Task HelpBot(Message message) => RepUsers.SendMessage(message.From.Id, "Список команд у бота:\n" + GetHelp(message.From.Id));

        public static Task SendMessageWrongNumberOfArgs(Message message) => RepUsers.SendMessage(message.From.Id, "Не верное кол-во агрументов\nСписок комманд: /help");

        public static async void ImageUploader(Message message)
        {
            try
            {
                var file = await App.Api.GetFileAsync(message.Photo.LastOrDefault()?.FileId);
                var filename = file.FileId + "." + file.FilePath.Split('.').Last();
                using (var saveImageStream = System.IO.File.Open(Properties.Settings.Default.SavePath + @"photos\" + filename, FileMode.Create))
                {
                    await App.Api.DownloadFileAsync(file.FilePath, saveImageStream);
                }
                await RepUsers.SendMessage(message.From.Id, "Загрузка изображения успешно завершена");
            }
            catch (Exception ex)
            {
                await RepUsers.SendMessage(message.From.Id, $"При загрузке изображения произошла ошибка: {ex.Message}");
            }
        }

        public static async void DocumentUploader(Message message)
        {
            try
            {
                var file = await App.Api.GetFileAsync(message.Document.FileId);
                using (var saveImageStream = System.IO.File.Open(Properties.Settings.Default.SavePath + @"documents\" + message.Document.FileName, FileMode.Create))
                {
                    await App.Api.DownloadFileAsync(file.FilePath, saveImageStream);
                }
                await RepUsers.SendMessage(message.From.Id, "Загрузка документа успешно завершена");
            }
            catch (Exception ex)
            {
                await RepUsers.SendMessage(message.From.Id, $"При загрузке документа произошла ошибка: {ex.Message}");
            }
        }

        public static async void ComStopApp()
        {
            await RepUsers.SendMessage($"Принудительное завершение работы приложения пользователем: {Environment.UserDomainName}");
            await ExecuteLaunchBot.StopBotAsync();
            App.Current.Dispatcher.BeginInvokeShutdown(System.Windows.Threading.DispatcherPriority.Background);
        }

        public static Task ShowScreenButtons(Message message, string arg)
        {
            string[] keys;
            ParserKeys(out keys, arg, message.From.Id);

            if (keys == null)
                return RepUsers.SendMessage(message.From.Id, "Не найден аргумент для выполнения команды");

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

            if (keys.Count() == 0)
                return App.Api.SendTextMessageAsync(message.Chat.Id, "Экранная клавиатура успешно удалена.", replyMarkup: new ReplyKeyboardRemove());
            else
                return App.Api.SendTextMessageAsync(message.Chat.Id, $"Количество добавленных кнопок: {keys.Count()}", replyMarkup: rkm);
        }

        private static void ParserKeys(out string[] keys, string arg, int idUser)
        {
            StatusUser statusUser = RepUsers.GetStatusUser(idUser);
            if (bool.TryParse(arg, out bool isShowKeys))
                keys = isShowKeys ? RepCommands.botCommands.Where(x => x.CountArgsCommand == 0 && x.StatusUser <= statusUser).Select(s => s.Command).ToArray() : new string[0];
            else
                keys = arg.ToLower() == "all" ? RepCommands.botCommands.Where(w => w.StatusUser <= statusUser).Select(s => s.Command).ToArray() : null;
        }

        public static class Messages
        {
            private static string RandomString(int length)
            {
                Random random = new Random();
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
            }

            public static async Task UserSentToAdmin(Message message, string[] args)
            {
                try
                {
                    if (message.Type == Telegram.Bot.Types.Enums.MessageType.Text)
                    {
                        await RepUsers.SendMessage($"Сообщение от пользователя \n(id: {message.From.Id} || IsBlocked: {!RepUsers.IsAuthNotIsBlockedUser(message.From.Id)}):\n\"{string.Join(" ", args)}\"");
                        await RepUsers.SendMessage(message.From.Id, "Сообщение успешно отправлено админу бота " + App.Api.GetMeAsync().Result);
                    }
                    else
                    {
                        await RepUsers.SendMessage(message.From.Id, "Разрешается использовать только текстовые сообщения. Сообщение не было отправлено.");
                    }
                }
                catch
                {
                    await RepUsers.SendMessage(message.From.Id, $"При отправке сообщения администратору произошла системная ошибка...\nПовторите попытку позже.");
                }
            }

            public static async Task AdminSentToUser(Message message, int idUser, string[] args)
            {
                if (RepUsers.IsAuthUser(idUser) && message.Type == Telegram.Bot.Types.Enums.MessageType.Text)
                {
                    try
                    {
                        await RepUsers.SendMessage(idUser,
                            $"Сообщение от администратора бота {App.Api.GetMeAsync().Result}: \n\"{string.Join(" ", args.Skip(1))}\"" +
                            $"\nВы можете написать администратору бота используя команду \"/messageToAdmin YourMessage\"");
                        await RepUsers.SendMessage(message.From.Id, $"Сообщение успешно отправлено пользователю \"{idUser}\"");
                    }
                    catch (Telegram.Bot.Exceptions.ApiRequestException ex)
                    {
                        await RepUsers.SendMessage($"При отправке сообщения пользователю с id: \"{idUser}\" произошла ошибка Telegram.Bot.Exceptions.ApiRequestException:\n\n{ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        await RepUsers.SendMessage($"При отправке сообщения пользователю с id: \"{idUser}\" произошла системная ошибка:\n\n{ex.Message}");
                    }
                }
                else
                {
                    await RepUsers.SendMessage(message.From.Id, "Разрешается использовать только текстовые сообщения. Сообщение не было отправлено.");
                }
            }

            public static async Task UserSpamFromAdmin(Message message, int idUser, int countMessages)
            {
                if (RepUsers.IsAuthUser(idUser) && message.Type == Telegram.Bot.Types.Enums.MessageType.Text)
                {
                    try
                    {
                        await RepUsers.SendMessage(idUser,
                            $"Вы были выбраны жертвой для спама от администратора бота {App.Api.GetMeAsync().Result}:\n" +
                            $"\nВы можете написать администратору бота используя команду \"/messageToAdmin YourMessage\"");
                        for (int i = 0; i < countMessages; i++)
                        {
                            await RepUsers.SendMessage(idUser, RandomString(new Random().Next(5, 40)));
                        }
                        await RepUsers.SendMessage(idUser, "Спам успешно завершён. Хорошего дня ;)");
                        await RepUsers.SendMessage("Спам пользователя \"" + idUser + "\" успешно завершён.");
                    }
                    catch (Telegram.Bot.Exceptions.ApiRequestException ex)
                    {
                        await RepUsers.SendMessage($"При отправке сообщения пользователю с id: \"{idUser}\" произошла ошибка Telegram.Bot.Exceptions.ApiRequestException:\n\n{ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        await RepUsers.SendMessage($"При отправке сообщения пользователю с id: \"{idUser}\" произошла системная ошибка:\n\n{ex.Message}");
                    }
                }
                else
                {
                    await RepUsers.SendMessage(message.From.Id, "Разрешается использовать только текстовые сообщения. Сообщение не было отправлено.");
                }
            }
        }

        public static class ControlPC
        {
            public static Task ExecuteUrl(Message message, string url)
            {
                if (!url.StartsWith("https://") || !url.StartsWith("http://"))
                {
                    Process.Start(url.Insert(0, "https://"));
                    return RepUsers.SendMessage(message.From.Id, $"Message: {message.Text.Split(' ').FirstOrDefault()} || Arg: {url}");
                }
                return Task.CompletedTask;
            }

            public static Task TurnOff(Message message, int tMin, int tSec)
            {
                while (tSec >= 60)
                {
                    tSec -= 60;
                    tMin++;
                }
                Process.Start(@"C:\Windows\System32\shutdown.exe", "/s /t " + (tMin * 60 + tSec));
                return RepUsers.SendMessage(message.From.Id, $"Таймер успешно установлен.\nЧерез {tMin} мин. {tSec} сек. ваш ПК будет выключен.");
            }

            public static bool ExecuteCmdCommand(string programmPath, string cmdArgs)
            {
                try
                {
                    Process.Start(programmPath, cmdArgs);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            public static class ContolVolume
            {
                private const int APPCOMMAND_VOLUME_MUTE = 0x80000;
                private const int APPCOMMAND_VOLUME_UP = 0xA0000;
                private const int APPCOMMAND_VOLUME_DOWN = 0x90000;
                private const int WM_APPCOMMAND = 0x319;

                [DllImport("user32.dll")]
                private static extern IntPtr SendMessageW(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);


                public static Task changeVolume(Message message, string strValue)
                {
                    if (int.TryParse(strValue, out int value))
                    {
                        if (value == 0) return RepUsers.SendMessage(message.From.Id, $"Изменение громкости на: {value} не является допустимым");
                        if (value >= -100 && value <= 100)
                        {
                            if (value >= 0)
                                for (double i = 0; i < Math.Ceiling((double)value / 2); i++)
                                    SendMessageW(Process.GetCurrentProcess().MainWindowHandle, WM_APPCOMMAND, Process.GetCurrentProcess().MainWindowHandle, (IntPtr)APPCOMMAND_VOLUME_UP);
                            if (value <= 0)
                                for (double i = 0; i > Math.Floor((double)value / 2); i--)
                                    SendMessageW(Process.GetCurrentProcess().MainWindowHandle, WM_APPCOMMAND, Process.GetCurrentProcess().MainWindowHandle, (IntPtr)APPCOMMAND_VOLUME_DOWN);
                            return RepUsers.SendMessage(message.From.Id, $"Громкость изменена на: {value}");
                        }
                        return RepUsers.SendMessage(message.From.Id, "Выход за пределы: [-100;100] при изменении громкости");
                    }
                    if (strValue.ToLower() == "mute")
                    {
                        SendMessageW(Process.GetCurrentProcess().MainWindowHandle, WM_APPCOMMAND, Process.GetCurrentProcess().MainWindowHandle, (IntPtr)APPCOMMAND_VOLUME_MUTE);
                        return RepUsers.SendMessage(message.From.Id, $"Громкость изменена на: {strValue}");
                    }
                    return Task.CompletedTask;
                }
            }

            public async static Task GetScreenshot(Message message)
            {
                try
                {
                    string filename = "ScreenCapture-" + DateTime.Now.ToString("ddMMyyyy-hhmmss") + ".png";
                    using (Bitmap bmp = new Bitmap((int)SystemParameters.VirtualScreenWidth, (int)SystemParameters.VirtualScreenHeight))
                    {
                        using Graphics graph = Graphics.FromImage(bmp);
                        graph.CopyFromScreen((int)SystemParameters.VirtualScreenLeft, (int)SystemParameters.VirtualScreenTop, 0, 0, bmp.Size);
                        bmp.Save(Properties.Settings.Default.SavePath + "\\" + filename);
                    }

                    using var stream = System.IO.File.OpenRead(Properties.Settings.Default.SavePath + "\\" + filename);
                    await App.Api.SendDocumentAsync(message.From.Id, new Telegram.Bot.Types.InputFiles.InputOnlineFile(stream, stream.Name));
                }
                catch (Exception ex)
                {
                    await RepUsers.SendMessage(message.From.Id, $"При работе со скриншотом, произошла ошибка: {ex.Message}");
                }
            }
        }

        public static class ControlUsers
        {
            private static ObservableCollection<PatternUser> ListUsers => RepUsers.AuthUsers;
            public static int StrToInt(string idUserString)
            {
                int.TryParse(idUserString, out int idNewUser);
                return idNewUser;
            }

            private static string ListOfUsersParseString()
            {
                int indexUser = 0;
                System.Text.StringBuilder messageText = new System.Text.StringBuilder($"Список пользователей бота {App.Api.GetMeAsync().Result}:\n\n");
                RepUsers.RefreshAndSort();
                foreach (var user in ListUsers)
                    messageText.Append($"{++indexUser}) {user.IdUser_Nickname} | Is blocked: {user.IsBlocked} | Status: {user.Status}\n");
                return messageText.ToString();
            }

            public static Task SendListOfUsers(Message message) => RepUsers.SendMessage(message.From.Id, ListOfUsersParseString());
            public static Task SaveChanges(Message message) => RepUsers.SendMessage(message.From.Id, RepUsers.Save() ? "Успешное сохранение изменений" : "При сохранении изменений произошла ошибка");
            public static Task CancelChanges(Message message) => RepUsers.SendMessage(message.From.Id, App.Current.Dispatcher.Invoke(() => RepUsers.Upload()) ? "Изменения успешно отменены" : "При отмене изменений произошла ошибка");

            public static async Task<Task> AuthNewUser(Message message, int idUser, string nickname = null)
            {
                try
                {
                    if (!RepUsers.IsAuthUser(idUser))
                    {
                        App.Current.Dispatcher.Invoke(() => ListUsers.Add(new PatternUser() { IdUser = idUser, LocalNickname = nickname }));
                        await RepUsers.SendMessage($"Пользователь c id: \"{idUser}{(!string.IsNullOrEmpty(nickname) ? " | " + nickname : string.Empty)}\" был успешно добавлен в список пользователей.");
                    }
                    else
                        await RepUsers.SendMessage($"Пользователь c id: \"{idUser}\" уже существует!");
                    await RepUsers.SendMessage(idUser, $"Вы были успешно добавлены в список пользователей бота.\nЗапросите у администратора бота {App.Api.GetMeAsync().Result} вас добавить в список разрешённых пользователей.\n\nВы можете написать администратору бота используя команду \"/messageToAdmin YourMessage\"");
                }
                catch (Exception ex)
                {
                    await RepUsers.SendMessage($"Error: При попытке использования команды \"{message.Text}\" пользователем {message.From.Id} произошла ошибка: \n{ex.Message}");
                }
                return SendListOfUsers(message);
            }

            public static Task DeauthUser(Message message, int idUser)
            {
                if (App.Current.Dispatcher.Invoke(() => ListUsers.Remove(ListUsers.Where(f => f.IdUser == idUser).DefaultIfEmpty(new PatternUser()).FirstOrDefault())))
                    RepUsers.SendMessage(message.From.Id, $"Пользователь c id: \"{idUser}\" был успешно успешно удалён из списка пользователей.").Wait();
                else
                    return RepUsers.SendMessage(message.From.Id, $"Пользователя c id: \"{idUser}\" не существует...");
                return SendListOfUsers(message);
            }

            public static Task DeauthUser(Message message, string[] args)
            {
                string LocalNickname = string.Join(" ", args);
                if (App.Current.Dispatcher.Invoke(() => ListUsers.Remove(ListUsers.Where(f => f.LocalNickname.ToLower() == LocalNickname.ToLower() && !StrToInt(f.LocalNickname).Equals(f.IdUser)).DefaultIfEmpty(new PatternUser()).FirstOrDefault())))
                    RepUsers.SendMessage(message.From.Id, $"Пользователь c Nickname: \"{LocalNickname}\" был успешно успешно удалён из списка пользователей.").Wait();
                else
                    return RepUsers.SendMessage(message.From.Id, $"Пользователя c Nickname: \"{LocalNickname}\" не существует...");
                return SendListOfUsers(message);
            }

            public static Task ChangeLocalName(Message message, int idUser, string[] args)
            {
                var findedUser = ListUsers.Where(f => f.IdUser == idUser).FirstOrDefault();
                if (findedUser is null)
                    return RepUsers.SendMessage(message.From.Id, $"Пользователь c id: \"{idUser}\" не найден...");

                string beforeChangingNickname = findedUser.LocalNickname;
                findedUser.LocalNickname = string.Join(" ", args.Skip(1));
                RepUsers.SendMessage(message.From.Id, $"Пользователю c id: \"{idUser}\" измененён Nickname: \"{beforeChangingNickname}\" => \"{findedUser.LocalNickname}\".").Wait();
                return SendListOfUsers(message);
            }

            public static Task LockUser(Message message, int idUser)
            {
                var findedUser = ListUsers.Where(f => f.IdUser == idUser).FirstOrDefault();
                if (findedUser is null)
                    return RepUsers.SendMessage(message.From.Id, $"Пользователь c id: \"{idUser}\" не найден...");

                findedUser.IsBlocked = findedUser.IsBlocked ? false : true;
                RepUsers.SendMessage(message.From.Id, $"Пользователь c id: \"{idUser}\" успешно {(findedUser.IsBlocked ? "заблокирован" : "разблокирован")}.").Wait();
                return SendListOfUsers(message);
            }

            public static Task LockUser(Message message, string[] args)
            {
                string LocalNickname = string.Join(" ", args);
                var findedUser = ListUsers.Where(f => f.LocalNickname.ToLower() == LocalNickname.ToLower()).FirstOrDefault();
                if (findedUser is null || StrToInt(findedUser.LocalNickname).Equals(findedUser.IdUser))
                    return RepUsers.SendMessage(message.From.Id, $"Пользователь c Nickname: \"{LocalNickname}\" не найден...");

                findedUser.IsBlocked = findedUser.IsBlocked ? false : true;
                RepUsers.SendMessage(message.From.Id, $"Пользователь c Nickname: \"{LocalNickname}\" успешно {(findedUser.IsBlocked ? "заблокирован" : "разблокирован")}.").Wait();
                return SendListOfUsers(message);
            }
        }
    }
}