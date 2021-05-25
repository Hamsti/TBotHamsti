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
using Telegram.Bot.Exceptions;
using TBotHamsti.Core;
using LevelCommand = TBotHamsti.Core.BotLevelCommand.LevelCommand;
using StatusUser = TBotHamsti.LogicRepository.RepUsers.StatusUser;
using TEnum = Telegram.Bot.Types.Enums;

namespace TBotHamsti.LogicRepository
{
    /// <summary>
    /// Implementation of all bot functionality
    /// </summary>
    public static class RepBotActions
    {
        public static string GetHelp(PatternUser user) => string.Join("\n", BotLevelCommand.GetBotLevelCommand(user).Result.CommandsOfLevel.Where(w => (w.StatusUser <= user.Status)).Select(s => s.ExampleCommand));

        public static Task HelpBot(PatternUser user) => RepUsers.SendMessage(user.Id, "Список команд у бота:\n" + GetHelp(user));

        public static Task SendMessageWrongNumberOfArgs(PatternUser user) => RepUsers.SendMessage(user.Id, "Не верное кол-во агрументов\nСписок комманд: /help");

        public static async void ImageUploader(PatternUser user, Message message)
        {
            try
            {
                var file = await App.Api.GetFileAsync(message.Photo.LastOrDefault()?.FileId);
                var filename = file.FileId + "." + file.FilePath.Split('.').Last();
                using (var saveImageStream = System.IO.File.Open(Properties.Settings.Default.SavePath + @"photos\" + filename, FileMode.Create))
                {
                    await App.Api.DownloadFileAsync(file.FilePath, saveImageStream);
                }
                await RepUsers.SendMessage(user.Id, "Загрузка изображения успешно завершена");
            }
            catch (Exception ex)
            {
                await RepUsers.SendMessage(user.Id, $"При загрузке изображения произошла ошибка: {ex.Message}");
            }
        }

        public static async void DocumentUploader(PatternUser user, Message message)
        {
            try
            {
                var file = await App.Api.GetFileAsync(message.Document.FileId);
                using (var saveImageStream = System.IO.File.Open(Properties.Settings.Default.SavePath + @"documents\" + message.Document.FileName, FileMode.Create))
                {
                    await App.Api.DownloadFileAsync(file.FilePath, saveImageStream);
                }
                await RepUsers.SendMessage(user.Id, "Загрузка документа успешно завершена");
            }
            catch (Exception ex)
            {
                await RepUsers.SendMessage(user.Id, $"При загрузке документа произошла ошибка: {ex.Message}");
            }
        }

        public static async void ComStopApp()
        {
            await RepUsers.SendMessage($"Принудительное завершение работы приложения пользователем: {Environment.UserDomainName}");
            await ExecuteLaunchBot.StopBotAsync();
            App.Current.Dispatcher.BeginInvokeShutdown(System.Windows.Threading.DispatcherPriority.Background);
        }

        public static Task ShowScreenButtons(PatternUser user, string arg)
        {
            ParserKeys(out string[] keys, arg, user);

            if (keys == null)
                return RepUsers.SendMessage(user.Id, "Не найден аргумент для выполнения команды");

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

        private static void ParserKeys(out string[] keys, string arg, PatternUser user)
        {
            if (bool.TryParse(arg, out bool isShowKeys))
                keys = isShowKeys ? CollectionCommands.RootLevel.CommandsOfLevel.Where(x => x.CountArgsCommand == 0 && x.StatusUser <= user.Status).Select(s => s.Command).ToArray() : new string[0];
            else
                keys = arg.ToLower() == "all" ? CollectionCommands.RootLevel.CommandsOfLevel.Where(w => w.StatusUser <= user.Status).Select(s => s.Command).ToArray() : null;
        }

        public static class Messages
        {
            private static string RandomString(int length)
            {
                Random random = new Random();
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
            }

            public static async Task UserSentToAdmin(PatternUser user, Message message, string[] args)
            {
                try
                {
                    if (message.Type == TEnum.MessageType.Text)
                    {
                        await RepUsers.SendMessage($"Сообщение от пользователя \n(id: {user.Id} || IsBlocked: {!RepUsers.IsAuthNotIsBlockedUser(user.Id)}):\n\"{string.Join(" ", args)}\"");
                        await RepUsers.SendMessage(user.Id, "Сообщение успешно отправлено админу бота " + App.Api.GetMeAsync().Result);
                    }
                    else
                    {
                        await RepUsers.SendMessage(user.Id, "Разрешается использовать только текстовые сообщения. Сообщение не было отправлено.");
                    }
                }
                catch
                {
                    await RepUsers.SendMessage(user.Id, $"При отправке сообщения администратору произошла системная ошибка...\nПовторите попытку позже.");
                }
            }

            public static async Task AdminSentToUser(PatternUser user, Message message, int Id, string[] args)
            {
                if (RepUsers.IsAuthUser(Id) && message.Type == TEnum.MessageType.Text)
                {
                    try
                    {
                        await RepUsers.SendMessage(Id,
                            $"Сообщение от администратора бота {App.Api.GetMeAsync().Result}: \n\"{string.Join(" ", args.Skip(1))}\"" +
                            $"\nВы можете написать администратору бота используя команду \"/sentToAdmin YourMessage\"");
                        await RepUsers.SendMessage(user.Id, $"Сообщение успешно отправлено пользователю \"{Id}\"");
                    }
                    catch (ApiRequestException ex)
                    {
                        await RepUsers.SendMessage($"При отправке сообщения пользователю с id: \"{Id}\" произошла ошибка Telegram.Bot.Exceptions.ApiRequestException:\n\n{ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        await RepUsers.SendMessage($"При отправке сообщения пользователю с id: \"{Id}\" произошла системная ошибка:\n\n{ex.Message}");
                    }
                }
                else
                {
                    await RepUsers.SendMessage(Id, "Разрешается использовать только текстовые сообщения. Сообщение не было отправлено.");
                }
            }

            public static async Task UserSpamFromAdmin(PatternUser user, Message message, int IdUserForSpam, int countMessages)
            {
                if (RepUsers.IsAuthUser(IdUserForSpam) && message.Type == TEnum.MessageType.Text)
                {
                    try
                    {
                        await RepUsers.SendMessage(IdUserForSpam,
                            $"Вы были выбраны жертвой для спама от администратора бота {App.Api.GetMeAsync().Result}:\n" +
                            $"\nВы можете написать администратору бота используя команду \"/sentToAdmin YourMessage\"");
                        for (int i = 0; i < countMessages; i++)
                        {
                            await RepUsers.SendMessage(IdUserForSpam, RandomString(new Random().Next(5, 40)));
                        }
                        await RepUsers.SendMessage(IdUserForSpam, "Спам успешно завершён. Хорошего дня ;)");
                        await RepUsers.SendMessage(user.Id, "Спам пользователя \"" + IdUserForSpam + "\" успешно завершён.");
                    }
                    catch (ApiRequestException ex)
                    {
                        await RepUsers.SendMessage(user.Id, $"При отправке сообщения пользователю с id: \"{IdUserForSpam}\" произошла ошибка Telegram.Bot.Exceptions.ApiRequestException:\n\n{ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        await RepUsers.SendMessage(user.Id, $"При отправке сообщения пользователю с id: \"{IdUserForSpam}\" произошла системная ошибка:\n\n{ex.Message}");
                    }
                }
                else
                {
                    await RepUsers.SendMessage(user.Id, "Разрешается использовать только текстовые сообщения. Сообщение не было отправлено.");
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

            public static Task TurnOff(PatternUser user, int tMin, int tSec)
            {
                while (tSec >= 60)
                {
                    tSec -= 60;
                    tMin++;
                }
                Process.Start(@"C:\Windows\System32\shutdown.exe", "/s /t " + (tMin * 60 + tSec));
                return RepUsers.SendMessage(user.Id, $"Таймер успешно установлен.\nЧерез {tMin} мин. {tSec} сек. ваш ПК будет выключен.");
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


                public static Task ChangeVolume(PatternUser user, string strValue)
                {
                    if (int.TryParse(strValue, out int value))
                    {
                        if (value == 0) return RepUsers.SendMessage(user.Id, $"Изменение громкости на: {value} не является допустимым");
                        if (value >= -100 && value <= 100)
                        {
                            if (value >= 0)
                                for (double i = 0; i < Math.Ceiling((double)value / 2); i++)
                                    SendMessageW(Process.GetCurrentProcess().MainWindowHandle, WM_APPCOMMAND, Process.GetCurrentProcess().MainWindowHandle, (IntPtr)APPCOMMAND_VOLUME_UP);
                            if (value <= 0)
                                for (double i = 0; i > Math.Floor((double)value / 2); i--)
                                    SendMessageW(Process.GetCurrentProcess().MainWindowHandle, WM_APPCOMMAND, Process.GetCurrentProcess().MainWindowHandle, (IntPtr)APPCOMMAND_VOLUME_DOWN);
                            return RepUsers.SendMessage(user.Id, $"Громкость изменена на: {value}");
                        }
                        return RepUsers.SendMessage(user.Id, "Выход за пределы: [-100;100] при изменении громкости");
                    }
                    if (strValue.ToLower() == "mute")
                    {
                        SendMessageW(Process.GetCurrentProcess().MainWindowHandle, WM_APPCOMMAND, Process.GetCurrentProcess().MainWindowHandle, (IntPtr)APPCOMMAND_VOLUME_MUTE);
                        return RepUsers.SendMessage(user.Id, $"Громкость изменена на: {strValue}");
                    }
                    return Task.CompletedTask;
                }
            }

            public async static Task GetScreenshot(PatternUser user)
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
                    await App.Api.SendDocumentAsync(user.Id, new Telegram.Bot.Types.InputFiles.InputOnlineFile(stream, stream.Name));
                }
                catch (Exception ex)
                {
                    await RepUsers.SendMessage(user.Id, $"При работе со скриншотом, произошла ошибка: {ex.Message}");
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
                RepUsers.Refresh();
                foreach (var user in ListUsers)
                    messageText.Append($"{++indexUser}) {user.IdUser_Nickname} | Is blocked: {user.IsBlocked} | Status: {user.Status}\n");
                return messageText.ToString();
            }

            public static Task SendListOfUsers(PatternUser user) => RepUsers.SendMessage(user.Id, ListOfUsersParseString());
            public static Task SaveChanges(PatternUser user) => RepUsers.SendMessage(user.Id, RepUsers.SaveRefresh() ? "Успешное сохранение изменений" : "При сохранении изменений произошла ошибка");
            public static Task CancelChanges(PatternUser user) => RepUsers.SendMessage(user.Id, App.Current.Dispatcher.Invoke(() => RepUsers.Upload()) ? "Изменения успешно отменены" : "При отмене изменений произошла ошибка");

            public static async Task<Task> AuthNewUser(PatternUser user, Message message, int IdSelectedUser, string nickname = null)
            {
                try
                {
                    if (!RepUsers.IsAuthUser(IdSelectedUser))
                    {
                        App.Current.Dispatcher.Invoke(() => ListUsers.Add(new PatternUser() { Id = IdSelectedUser, LocalNickname = nickname }));
                        await RepUsers.SendMessage($"Пользователь c id: \"{IdSelectedUser}{(!string.IsNullOrEmpty(nickname) ? " | " + nickname : string.Empty)}\" был успешно добавлен в список пользователей.");
                    }
                    else
                        await RepUsers.SendMessage($"Пользователь c id: \"{IdSelectedUser}\" уже существует!");
                    await RepUsers.SendMessage(IdSelectedUser, $"Вы были успешно добавлены в список пользователей бота.\nЗапросите у администратора бота {App.Api.GetMeAsync().Result} вас добавить в список разрешённых пользователей.\n\nВы можете написать администратору бота используя команду \"/sentToAdmin YourMessage\"");
                }
                catch (Exception ex)
                {
                    await RepUsers.SendMessage($"Error: При попытке использования команды \"{message.Text}\" пользователем {user.Id} произошла ошибка: \n{ex.Message}");
                }
                return SendListOfUsers(user);
            }

            public static Task DeauthUser(PatternUser user, int IdSelectedUser)
            {
                if (App.Current.Dispatcher.Invoke(() => ListUsers.Remove(ListUsers.Where(f => f.Id == IdSelectedUser).DefaultIfEmpty(new PatternUser()).FirstOrDefault())))
                    RepUsers.SendMessage(user.Id, $"Пользователь c id: \"{IdSelectedUser}\" был успешно успешно удалён из списка пользователей.").Wait();
                else
                    return RepUsers.SendMessage(user.Id, $"Пользователя c id: \"{IdSelectedUser}\" не существует...");
                return SendListOfUsers(user);
            }

            public static Task DeauthUser(PatternUser user, string[] args)
            {
                string LocalNickname = string.Join(" ", args);
                if (App.Current.Dispatcher.Invoke(() => ListUsers.Remove(ListUsers.Where(f => f.LocalNickname.ToLower() == LocalNickname.ToLower() && !StrToInt(f.LocalNickname).Equals(f.Id)).DefaultIfEmpty(new PatternUser()).FirstOrDefault())))
                    RepUsers.SendMessage(user.Id, $"Пользователь c Nickname: \"{LocalNickname}\" был успешно успешно удалён из списка пользователей.").Wait();
                else
                    return RepUsers.SendMessage(user.Id, $"Пользователя c Nickname: \"{LocalNickname}\" не существует...");
                return SendListOfUsers(user);
            }

            public static Task ChangeLocalName(PatternUser user, int IdSelectedUser, string[] args)
            {
                var selectedUser = ListUsers.Where(f => f.Id == IdSelectedUser).FirstOrDefault();
                if (selectedUser is null)
                    return RepUsers.SendMessage(user.Id, $"Пользователь c id: \"{IdSelectedUser}\" не найден...");

                string beforeChangingNickname = selectedUser.LocalNickname;
                selectedUser.LocalNickname = string.Join(" ", args.Skip(1));
                RepUsers.SendMessage(user.Id, $"Пользователю c id: \"{IdSelectedUser}\" измененён Nickname: \"{beforeChangingNickname}\" => \"{selectedUser.LocalNickname}\".").Wait();
                return SendListOfUsers(user);
            }

            public static Task LockUser(PatternUser user, int IdSelectedUser)
            {
                var selectedUser = ListUsers.Where(f => f.Id == IdSelectedUser).FirstOrDefault();
                if (selectedUser is null)
                    return RepUsers.SendMessage(user.Id, $"Пользователь c id: \"{IdSelectedUser}\" не найден...");

                selectedUser.IsBlocked = !selectedUser.IsBlocked;
                RepUsers.SendMessage(user.Id, $"Пользователь c id: \"{IdSelectedUser}\" успешно {(selectedUser.IsBlocked ? "заблокирован" : "разблокирован")}.").Wait();
                return SendListOfUsers(user);
            }

            public static Task LockUser(PatternUser user, string[] args)
            {
                string LocalNickname = string.Join(" ", args);
                var selectedUser = ListUsers.Where(f => f.LocalNickname.ToLower() == LocalNickname.ToLower()).FirstOrDefault();
                if (selectedUser is null || StrToInt(selectedUser.LocalNickname).Equals(selectedUser.Id))
                    return RepUsers.SendMessage(user.Id, $"Пользователь c Nickname: \"{LocalNickname}\" не найден...");

                selectedUser.IsBlocked = !selectedUser.IsBlocked;
                RepUsers.SendMessage(user.Id, $"Пользователь c Nickname: \"{LocalNickname}\" успешно {(selectedUser.IsBlocked ? "заблокирован" : "разблокирован")}.").Wait();
                return SendListOfUsers(user);
            }
        }
    }
}