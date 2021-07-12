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
using System.Threading;
using System.Windows.Threading;

namespace TBotHamsti.LogicRepository
{
    /// <summary>
    /// Implementation of all bot functionality
    /// </summary>
    public static class RepBotActions
    {
        public static string GetHelp(PatternUser user) => string.Join("\n", BotLevelCommand.GetBotLevelCommand(user).CommandsOfLevel.Where(w => (w.StatusUser <= user.Status)).Select(s => s.ExampleCommand));

        public static Task HelpBot(PatternUser user) => user.SendMessageAsync("Список команд у бота:\n" + GetHelp(user));

        public static Exception ExceptionWrongNumberOfArgs => new ArgumentOutOfRangeException("Не верное кол-во агрументов\nСписок комманд: /help");

        private static string GetOriginalArgs(ITCommand model, Message message, int skipArgs = 0)
        {
            if (model.Args.Length > skipArgs)
                return message.Text.Substring(message.Text.LastIndexOf(model.Args[skipArgs]));
            else
                throw ExceptionWrongNumberOfArgs;
        }

        public static async Task ImageUploader(PatternUser user, Message message)
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

        public static async Task DocumentUploader(PatternUser user, Message message)
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

        public static async Task ComStopApp(PatternUser user)
        {
            await user.SendMessageAsync($"Принудительное завершение работы приложения пользователем: " + user.IdUser_Nickname);
            await ExecuteLaunchBot.StopBotAsync();
            App.Current.Dispatcher.BeginInvokeShutdown(System.Windows.Threading.DispatcherPriority.Background);
        }

        public static Task ShowScreenButtons(PatternUser user, string arg)
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

            public static async Task UserSentToAdmin(ITCommand model, PatternUser user, Message message)
            {
                await StatusUser.Admin.SendMessageAsync($"Сообщение от пользователя \n[{user.IdUser_Nickname} | blocked: {user.IsBlocked}):\n\"{GetOriginalArgs(model, message)}\"");
                await user.SendMessageAsync("Сообщение успешно отправлено админу бота " + App.Api.GetMeAsync().Result);
            }

            public static async Task AdminSentToUser(PatternUser userSource, PatternUser userDestination, string[] args)
            {
                if (userDestination is null)
                {
                    throw new ArgumentNullException(nameof(userDestination));
                }

                await userDestination.SendMessageAsync($"Сообщение от администратора бота {App.Api.GetMeAsync().Result}: \n\"{string.Join(" ", args.Skip(1))}\"" +
                    $"\nВы можете написать администратору бота используя команду \"/sentToAdmin YourMessage\"");
                await userSource.SendMessageAsync($"Сообщение успешно отправлено пользователю \"{userDestination.IdUser_Nickname}\"");
            }

            public static async Task UserSpamFromAdmin(PatternUser userSource, PatternUser userDestination, int countMessages)
            {
                if (userDestination is null)
                {
                    throw new ArgumentNullException(nameof(userDestination));
                }

                await userDestination.SendMessageAsync(
                    $"Вы были выбраны жертвой для спама от администратора бота {await App.Api.GetMeAsync()}:\n" +
                    $"\nВы можете написать администратору бота используя команду \"/sentToAdmin YourMessage\"");
                for (int i = 0; i < countMessages; i++)
                {
                    await userDestination.SendMessageAsync(RandomString(new Random().Next(5, 40)));
                }
                await userDestination.SendMessageAsync("Спам успешно завершён. Хорошего дня ;>");
                await userSource.SendMessageAsync("Спам пользователя \"" + userDestination.IdUser_Nickname + "\" успешно завершён.");
            }
        }

        public static class ControlPC
        {
            public static Task ExecuteUrl(ITCommand model, PatternUser user, Message message)
            {
                string url = model.Args.FirstOrDefault();
                if (!url.StartsWith("https://") || !url.StartsWith("http://"))
                {
                    Process.Start(url.Insert(0, "https://"));
                    return user.SendMessageAsync($"Message: {message.Text.Split(' ').FirstOrDefault()} || Arg: {url}");
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
                return user.SendMessageAsync($"Таймер успешно установлен.\nЧерез {tMin} мин. {tSec} сек. ваш ПК будет выключен.");
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
                        if (value == 0) return user.SendMessageAsync($"Изменение громкости на: {value} не является допустимым");
                        if (value >= -100 && value <= 100)
                        {
                            if (value >= 0)
                                for (double i = 0; i < Math.Ceiling((double)value / 2); i++)
                                    SendMessageW(Process.GetCurrentProcess().MainWindowHandle, WM_APPCOMMAND, Process.GetCurrentProcess().MainWindowHandle, (IntPtr)APPCOMMAND_VOLUME_UP);
                            if (value <= 0)
                                for (double i = 0; i > Math.Floor((double)value / 2); i--)
                                    SendMessageW(Process.GetCurrentProcess().MainWindowHandle, WM_APPCOMMAND, Process.GetCurrentProcess().MainWindowHandle, (IntPtr)APPCOMMAND_VOLUME_DOWN);
                            return user.SendMessageAsync($"Громкость изменена на: {value}");
                        }
                        return user.SendMessageAsync("Выход за пределы: [-100;100] при изменении громкости");
                    }
                    if (strValue.ToLower() == "mute")
                    {
                        SendMessageW(Process.GetCurrentProcess().MainWindowHandle, WM_APPCOMMAND, Process.GetCurrentProcess().MainWindowHandle, (IntPtr)APPCOMMAND_VOLUME_MUTE);
                        return user.SendMessageAsync($"Громкость изменена на: {strValue}");
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
                    await user.SendMessageAsync($"При работе со скриншотом, произошла ошибка: {ex.Message}");
                }
            }
        }

        public static class ControlUsers
        {
            private static ObservableCollection<PatternUser> ListUsers => RepUsers.AuthUsers;
            public static int StrToInt(string idUserString) => int.TryParse(idUserString, out int idNewUser) ? idNewUser : -1;

            private static string ListOfUsersParseString()
            {
                int indexUser = 0;
                System.Text.StringBuilder messageText = new System.Text.StringBuilder($"Список пользователей бота {App.Api.GetMeAsync().Result}:\n\n");
                RepUsers.Refresh();
                foreach (var user in ListUsers)
                    messageText.Append($"{++indexUser}) {user.IdUser_Nickname} | Is blocked: {user.IsBlocked} | Status: {user.Status}\n");
                return messageText.ToString();
            }

            public static Task SendListOfUsers(PatternUser user) => user.SendMessageAsync(ListOfUsersParseString());
            public static Task SaveChanges(PatternUser user) => user.SendMessageAsync(RepUsers.SaveRefresh() ? "Успешное сохранение изменений" : "При сохранении изменений произошла ошибка");
            public static Task CancelChanges(PatternUser user) => user.SendMessageAsync(App.Current.Dispatcher.Invoke(() => RepUsers.Upload()) ? "Изменения успешно отменены" : "При отмене изменений произошла ошибка");

            public static async Task<PatternUser> AuthNewUser(ITCommand model, Message message)
            {
                int id;
                if (model is null)
                {
                    throw new ArgumentNullException(nameof(model));
                }

                if (model.Args is null)
                {
                    throw new ArgumentNullException(nameof(model.Args));
                }

                if (model.CountArgsCommand == 0)
                {
                    id = message.From.Id;
                }
                else if ((id = StrToInt(model.Args[0])) < 0)
                {
                    throw new ArgumentException("id isn't in the correct format", nameof(id));
                }

                return AddUser(id, null);
            }

            public static PatternUser AddUser(int id, string userName)
            {
                PatternUser newUser = new PatternUser() { Id = id, LocalNickname = userName };
                if (RepUsers.GetUser(id) is null)
                {

                    App.UiContext.Send(x => ListUsers.Add(newUser), null);

                    //newUser.SendMessageAsync($"Вы были успешно добавлены в список пользователей бота.\n" +
                    //    $"Запросите у администратора бота {App.Api.GetMeAsync().Result} вас добавить в список разрешённых пользователей.\n\n" +
                    //    $"Вы можете написать администратору бота используя команду \"{CollectionCommands.SendMessageToAdminCommand.ExampleCommand}\"");
                    return newUser;

                }

                throw new ArgumentException($"Пользователь [{newUser.IdUser_Nickname}] уже существует!", nameof(id));
            }

            public static async Task AuthNewUser(ITCommand model, PatternUser userSource, Message message)
            {
                string nickname = null;
                if (userSource is null)
                {
                    throw new ArgumentNullException(nameof(userSource));
                }

                if (model.CountArgsCommand > 1)
                {
                    nickname = GetOriginalArgs(model, message, 1);
                }

                PatternUser newUser = await AuthNewUser(model, message);
                newUser.LocalNickname = nickname;
                await userSource.SendMessageAsync($"Пользователь [{newUser.IdUser_Nickname}] успешно добавлен в список пользователей.");
            }

            public static async Task<PatternUser> StartCommandUser(Message message)
            {
                PatternUser user = AddUser(message.From.Id, message.From.Username);
                await user.SendMessageAsync($"Вы были успешно добавлены в список пользователей бота.\n" +
                    $"Запросите у администратора бота {App.Api.GetMeAsync().Result} вас добавить в список разрешённых пользователей.\n\n" +
                    $"Вы можете написать администратору бота используя команду \"{CollectionCommands.SendMessageToAdminCommand.ExampleCommand}\"");
                return user;
            }

            public static Task DeauthUser(PatternUser user, int IdSelectedUser)
            {
                if (App.Current.Dispatcher.Invoke(() => ListUsers.Remove(ListUsers.Where(f => f.Id == IdSelectedUser).DefaultIfEmpty(new PatternUser()).FirstOrDefault())))
                    user.SendMessageAsync($"Пользователь c id: \"{IdSelectedUser}\" был успешно успешно удалён из списка пользователей.").Wait();
                else
                    return user.SendMessageAsync($"Пользователя c id: \"{IdSelectedUser}\" не существует...");
                return SendListOfUsers(user);
            }

            public static Task DeauthUser(PatternUser user, string[] args)
            {
                string LocalNickname = string.Join(" ", args);
                if (App.Current.Dispatcher.Invoke(() => ListUsers.Remove(ListUsers.Where(f => f.LocalNickname != null && f.LocalNickname.ToLower() == LocalNickname.ToLower() && !StrToInt(f.LocalNickname).Equals(f.Id)).DefaultIfEmpty(new PatternUser()).FirstOrDefault())))
                    user.SendMessageAsync($"Пользователь c Nickname: \"{LocalNickname}\" был успешно успешно удалён из списка пользователей.").Wait();
                else
                    return user.SendMessageAsync($"Пользователя c Nickname: \"{LocalNickname}\" не существует...");
                return SendListOfUsers(user);
            }

            public static Task ChangeLocalName(ITCommand model, PatternUser user, Message message)
            {
                var selectedUser = ListUsers.Where(f => f.Id == StrToInt(model.Args[0])).DefaultIfEmpty(null)?.FirstOrDefault()
                    ?? throw new ArgumentException($"Пользователь c id: \"{model.Args[0]}\" не найден...");

                string beforeChangingNickname = selectedUser.LocalNickname;
                selectedUser.LocalNickname = GetOriginalArgs(model, message, 1);
                user.SendMessageAsync($"Пользователю c id: \"{model.Args[0]}\" измененён Nickname: \"{beforeChangingNickname}\" => \"{selectedUser.LocalNickname}\".").Wait();
                return SendListOfUsers(user);
            }

            public static Task LockUser(PatternUser user, int IdSelectedUser)
            {
                var selectedUser = ListUsers.Where(f => f.Id == IdSelectedUser).FirstOrDefault();
                if (selectedUser is null)
                    return user.SendMessageAsync($"Пользователь c id: \"{IdSelectedUser}\" не найден...");

                selectedUser.IsBlocked = !selectedUser.IsBlocked;
                user.SendMessageAsync($"Пользователь c id: \"{IdSelectedUser}\" успешно {(selectedUser.IsBlocked ? "заблокирован" : "разблокирован")}.").Wait();
                return SendListOfUsers(user);
            }

            public static Task LockUser(PatternUser user, string[] args)
            {
                string LocalNickname = string.Join(" ", args);
                var selectedUser = ListUsers.Where(f => f.LocalNickname.ToLower() == LocalNickname.ToLower()).FirstOrDefault();
                if (selectedUser is null || StrToInt(selectedUser.LocalNickname).Equals(selectedUser.Id))
                    return user.SendMessageAsync($"Пользователь c Nickname: \"{LocalNickname}\" не найден...");

                selectedUser.IsBlocked = !selectedUser.IsBlocked;
                user.SendMessageAsync($"Пользователь c Nickname: \"{LocalNickname}\" успешно {(selectedUser.IsBlocked ? "заблокирован" : "разблокирован")}.").Wait();
                return SendListOfUsers(user);
            }
        }
    }
}