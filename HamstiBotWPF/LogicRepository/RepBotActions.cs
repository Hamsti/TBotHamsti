using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using Screen = System.Windows.Forms.Screen;
using System.Runtime.InteropServices;
using Telegram.Bot.Types.ReplyMarkups;
using System.Collections.ObjectModel;
using HamstiBotWPF.Core;
using LevelCommand = HamstiBotWPF.Core.BotLevelCommand.LevelCommand;

namespace HamstiBotWPF.LogicRepository
{
    /// <summary>
    /// Implementation of all bot functionality
    /// </summary>
    public static class RepBotActions
    {
        public static string HelpForUsers => (GlobalUnit.currentLevelCommand != LevelCommand.Root ? BotLevelCommand.TOPREVLEVEL + "\n" : string.Empty) +
            string.Join("\n", GlobalUnit.botCommands.Where(w => (w.VisibleForUsers == true && w.Command != "/" + LevelCommand.Root.ToString().ToUpper()) &&
            GlobalUnit.currentLevelCommand == (w.GetType().Equals(typeof(BotCommand)) ? w.NameOfLevel : ((BotLevelCommand)w).ParrentLevel)).
            Select(s => s.ExampleCommand ?? ((BotLevelCommand)s).ExampleCommand));
        public static string HelpForAdmin => (GlobalUnit.currentLevelCommand != LevelCommand.Root ? BotLevelCommand.TOPREVLEVEL + "\n" : string.Empty) +
            string.Join("\n", GlobalUnit.botCommands.Where(w => w.Command != "/" + LevelCommand.Root.ToString().ToUpper() &&
            GlobalUnit.currentLevelCommand == (w.GetType().Equals(typeof(BotCommand)) ? w.NameOfLevel : ((BotLevelCommand)w).ParrentLevel)).
            Select(s => s.ExampleCommand ?? ((BotLevelCommand)s).ExampleCommand));
        public static Task HelpBot(Message message) => GlobalUnit.Api.SendTextMessageAsync(message.From.Id, "Список команд у бота:\n" + HelpForUsers);
        public static Task HelpBotAdmin(Message message) => GlobalUnit.Api.SendTextMessageAsync(message.From.Id, "Список всех реализованных команд у бота:\n" + HelpForAdmin);
        public static Task SendMessageWrongNumberOfArgs(Message message) => GlobalUnit.Api.SendTextMessageAsync(message.From.Id, "Не верное кол-во агрументов\nСписок комманд: /help");

        public static async void ImageUploader(Message message)
        {
            try
            {
                var file = await GlobalUnit.Api.GetFileAsync(message.Photo.LastOrDefault()?.FileId);
                var filename = file.FileId + "." + file.FilePath.Split('.').Last();
                using (var saveImageStream = System.IO.File.Open(Properties.Settings.Default.SavePath + @"photos\" + filename, FileMode.Create))
                {
                    await GlobalUnit.Api.DownloadFileAsync(file.FilePath, saveImageStream);
                }
                await GlobalUnit.Api.SendTextMessageAsync(message.From.Id, "Загрузка изображения успешно завершена");
            }
            catch (Exception ex)
            {
                await GlobalUnit.Api.SendTextMessageAsync(message.From.Id, $"При загрузке изображения произошла ошибка: {ex.Message}");
            }
        }

        public static async void DocumentUploader(Message message)
        {
            try
            {
                var file = await GlobalUnit.Api.GetFileAsync(message.Document.FileId);
                using (var saveImageStream = System.IO.File.Open(Properties.Settings.Default.SavePath + @"documents\" + message.Document.FileName, FileMode.Create))
                {
                    await GlobalUnit.Api.DownloadFileAsync(file.FilePath, saveImageStream);
                }
                await GlobalUnit.Api.SendTextMessageAsync(message.From.Id, "Загрузка документа успешно завершена");
            }
            catch (Exception ex)
            {
                await GlobalUnit.Api.SendTextMessageAsync(message.From.Id, $"При загрузке документа произошла ошибка: {ex.Message}");
            }
        }

        public static async void ComStopBot() => await ExecuteLaunchBot.StopBotAsync();

        public static async void ComStopApp(Message message)
        {
            await GlobalUnit.Api.SendTextMessageAsync(message.From.Id, $"Принудительное завершение работы приложения пользователем: {Environment.UserDomainName}");
            await ExecuteLaunchBot.StopBotAsync();
            Environment.Exit(0);
        }

        public static Task ShowScreenButtons(Message message, string Show)
        {
            string[] keys;
            ParserKeys(out keys, Show, message.From.Id);

            if (keys == null)
                return GlobalUnit.Api.SendTextMessageAsync(message.Chat.Id, "Не найден аргумент для изменения аргумента");

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
                return GlobalUnit.Api.SendTextMessageAsync(message.Chat.Id, "Экранная клавиатура успешно удалена.", replyMarkup: new ReplyKeyboardRemove());
            else
                return GlobalUnit.Api.SendTextMessageAsync(message.Chat.Id, $"Количество добавленных кнопок: {keys.Count()}", replyMarkup: rkm);
        }

        private static void ParserKeys(out string[] keys, string Show, int IdUser)
        {
            bool isShow;
            if (bool.TryParse(Show, out isShow))
            {
                if (isShow)
                {
                    keys = new string[GlobalUnit.botCommands.Where(x => x.VisibleForUsers).Count()];
                    if (RepUsers.IsHaveAccessAdmin(IdUser))
                        keys = GlobalUnit.botCommands.Where(x => x.CountArgsCommand == 0).Select(s => s.Command).ToArray();
                    else
                        keys = GlobalUnit.botCommands.Where(x => x.CountArgsCommand == 0 && x.VisibleForUsers).Select(s => s.Command).ToArray();
                }
                else
                    keys = new string[0];
            }
            else
            {
                if (Show.ToLower() == "all")
                {
                    keys = new string[GlobalUnit.botCommands.Count()];
                    if (RepUsers.IsHaveAccessAdmin(IdUser))
                        keys = GlobalUnit.botCommands.Where(w => w.Command != "/helpAdmin").Select(s => s.Command).ToArray();
                    else
                        keys = GlobalUnit.botCommands.Where(w => w.VisibleForUsers).Select(s => s.Command).ToArray();
                }
                else
                    keys = null;
            }
        }

        public static class Messages
        {
            private static string RandomString(int length)
            {
                Random random = new Random();
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                return new string(Enumerable.Repeat(chars, length)
                  .Select(s => s[random.Next(s.Length)]).ToArray());
            }

            public static async void UserSentToAdmin(Message message, string[] Args)
            {
                try
                {
                    if (message.Type == Telegram.Bot.Types.Enums.MessageType.Text)
                    {
                        await GlobalUnit.Api.SendTextMessageAsync(Properties.Settings.Default.AdminId,
                            $"Сообщение от пользователя \n(id: {message.From.Id} || IsBlocked: {!RepUsers.IsAuthNotIsBlockedUser(message.From.Id)}):\n{string.Join(" ", Args)}");
                        await GlobalUnit.Api.SendTextMessageAsync(message.From.Id, "Сообщение успешно отправлено админу бота " + GlobalUnit.Api.GetMeAsync().Result);
                    }
                    else
                    {
                        await GlobalUnit.Api.SendTextMessageAsync(message.From.Id, "Разрешается использовать только текстовые сообщения. Сообщение не было отправлено.");
                    }
                }
                catch
                {
                    await GlobalUnit.Api.SendTextMessageAsync(message.From.Id, $"При отправке сообщения администратору произошла системная ошибка...\nПовторите попытку позже.");
                }
            }

            public static async void AdminSentToUser(Message message, int IdUser, string[] Args)
            {
                if (RepUsers.IsAuthUser(IdUser) && message.Type == Telegram.Bot.Types.Enums.MessageType.Text)
                {
                    try
                    {
                        await GlobalUnit.Api.SendTextMessageAsync(IdUser,
                            $"Сообщение от администратора бота {GlobalUnit.Api.GetMeAsync().Result}: \n{string.Join(" ", Args.Skip(1))}" +
                            $"\nВы можете написать администратору бота используя команду \"/messageToAdmin YourMessage\"");
                        await GlobalUnit.Api.SendTextMessageAsync(message.From.Id, "Сообщение успешно отправлено пользователю " + IdUser);
                    }
                    catch (Telegram.Bot.Exceptions.ApiRequestException ex)
                    {
                        await GlobalUnit.Api.SendTextMessageAsync(Properties.Settings.Default.AdminId, $"При отправке сообщения пользователю с id: \"{IdUser}\" произошла ошибка Telegram.Bot.Exceptions.ApiRequestException:\n\n{ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        await GlobalUnit.Api.SendTextMessageAsync(Properties.Settings.Default.AdminId, $"При отправке сообщения пользователю с id: \"{IdUser}\" произошла системная ошибка:\n\n{ex.Message}");
                    }
                }
                else
                {
                    await GlobalUnit.Api.SendTextMessageAsync(message.From.Id, "Разрешается использовать только текстовые сообщения. Сообщение не было отправлено.");
                }
            }

            public static async void UserSpamFromAdmin(Message message, int IdUser, int CountMessages)
            {
                if (RepUsers.IsAuthUser(IdUser) && message.Type == Telegram.Bot.Types.Enums.MessageType.Text)
                {
                    try
                    {
                        await GlobalUnit.Api.SendTextMessageAsync(IdUser,
                            $"Вы были выбраны жертвой для спама от администратора бота {GlobalUnit.Api.GetMeAsync().Result}:\n" +
                            $"\nВы можете написать администратору бота используя команду \"/messageToAdmin YourMessage\"");
                        for (int i = 0; i < CountMessages; i++)
                        {
                            await GlobalUnit.Api.SendTextMessageAsync(IdUser, RandomString(new Random().Next(5, 40)));
                        }
                        await GlobalUnit.Api.SendTextMessageAsync(IdUser, "Спам успешно завершён. Хорошего дня ;)");
                        await GlobalUnit.Api.SendTextMessageAsync(Properties.Settings.Default.AdminId, "Спам пользователя " + IdUser + " успешно завершён.");
                    }
                    catch (Telegram.Bot.Exceptions.ApiRequestException ex)
                    {
                        await GlobalUnit.Api.SendTextMessageAsync(Properties.Settings.Default.AdminId, $"При отправке сообщения пользователю с id: \"{IdUser}\" произошла ошибка Telegram.Bot.Exceptions.ApiRequestException:\n\n{ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        await GlobalUnit.Api.SendTextMessageAsync(Properties.Settings.Default.AdminId, $"При отправке сообщения пользователю с id: \"{IdUser}\" произошла системная ошибка:\n\n{ex.Message}");
                    }
                }
                else
                {
                    await GlobalUnit.Api.SendTextMessageAsync(message.From.Id, "Разрешается использовать только текстовые сообщения. Сообщение не было отправлено.");
                }
            }
        }

        public static class ControlPC
        {
            public static void ExecuteUrl(Message message, string Url)
            {
                if (!Url.StartsWith("https://") || !Url.StartsWith("http://"))
                {
                    Process.Start(Url.Insert(0, "https://"));
                    GlobalUnit.Api.SendTextMessageAsync(message.From.Id, $"Message: {message.Text.Split(' ').FirstOrDefault()} || Arg: {Url}");
                }
            }

            public static Task TurnOff(Message message, int tMin, int tSec)
            {
                while (tSec >= 60)
                {
                    tSec -= 60;
                    tMin++;
                }
                Process.Start(@"C:\Windows\System32\shutdown.exe", "/s /t " + (tMin * 60 + tSec));
                return GlobalUnit.Api.SendTextMessageAsync(message.From.Id, $"Таймер успешно установлен.\nЧерез {tMin} мин. {tSec} сек. ваш ПК будет выключен.");
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


                public static void changeVolume(Message message, string strValue)
                {
                    int Value;
                    if (int.TryParse(strValue, out Value))
                    {
                        if (Value == 0)
                        {
                            GlobalUnit.Api.SendTextMessageAsync(message.From.Id, $"Изменение громкости на: {Value} не является допустимым");
                            return;
                        }
                        if (Value >= -100 && Value <= 100)
                        {
                            if (Value >= 0)
                                for (double i = 0; i < Math.Ceiling((double)Value / 2); i++)
                                    SendMessageW(Process.GetCurrentProcess().MainWindowHandle, WM_APPCOMMAND, Process.GetCurrentProcess().MainWindowHandle, (IntPtr)APPCOMMAND_VOLUME_UP);
                            if (Value <= 0)
                                for (double i = 0; i > Math.Floor((double)Value / 2); i--)
                                    SendMessageW(Process.GetCurrentProcess().MainWindowHandle, WM_APPCOMMAND, Process.GetCurrentProcess().MainWindowHandle, (IntPtr)APPCOMMAND_VOLUME_DOWN);
                            GlobalUnit.Api.SendTextMessageAsync(message.From.Id, $"Громкость изменена на: {Value}");
                        }
                        else
                        {
                            GlobalUnit.Api.SendTextMessageAsync(message.From.Id, "Выход за пределы: [-100;100] при изменении громкости");
                        }
                    }
                    else
                    {
                        if (strValue.ToLower() == "mute")
                        {
                            SendMessageW(Process.GetCurrentProcess().MainWindowHandle, WM_APPCOMMAND, Process.GetCurrentProcess().MainWindowHandle, (IntPtr)APPCOMMAND_VOLUME_MUTE);
                            GlobalUnit.Api.SendTextMessageAsync(message.From.Id, $"Громкость изменена на: {strValue}");
                        }
                    }
                }
            }

            public async static void GetScreenshot(Message message, string nameOfPicture = "Screenshot.png")
            {
                try
                {
                    Graphics graph;
                    var bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                    graph = Graphics.FromImage(bmp);
                    graph.CopyFromScreen(0, 0, 0, 0, bmp.Size);
                    bmp.Save(nameOfPicture);
                    using (var stream = System.IO.File.OpenRead(nameOfPicture))
                    {
                        await GlobalUnit.Api.SendDocumentAsync(message.From.Id, new Telegram.Bot.Types.InputFiles.InputOnlineFile(stream, stream.Name));
                    }
                }
                catch (Exception ex)
                {
                    await GlobalUnit.Api.SendTextMessageAsync(message.From.Id, $"При работе со скриншотом, произошла ошибка: {ex.Message}");
                }
            }
        }

        public static class ControlUsers
        {
            private static ObservableCollection<PatternUser> ListUsers => GlobalUnit.AuthUsers;
            public static int StrToInt(string IdUserString)
            {
                int.TryParse(IdUserString, out int idNewUser);
                return idNewUser;
            }

            private static string ListOfUsersParseString()
            {
                System.Text.StringBuilder messageText = new System.Text.StringBuilder($"Список пользователей бота {GlobalUnit.Api.GetMeAsync().Result}:\n\n");
                RepUsers.RefreshAndSort();
                foreach (var user in ListUsers)
                    messageText.Append(user.IdUser_Nickname + " | IsBlocked: " + user.IsBlocked + "\n");
                return messageText.ToString();
            }

            public static Task SendListOfUsers(Message message) => GlobalUnit.Api.SendTextMessageAsync(message.From.Id, ListOfUsersParseString());
            public static Task SaveChanges(Message message) => GlobalUnit.Api.SendTextMessageAsync(message.From.Id, RepUsers.Save() ? "Успешное сохранение изменений" : "При сохранении изменений произошла ошибка");
            public static Task CancelChanges(Message message) => GlobalUnit.Api.SendTextMessageAsync(message.From.Id, App.Current.Dispatcher.Invoke(() => RepUsers.Upload()) ? "Изменения успешно отменены" : "При отмене изменений произошла ошибка");

            public static async Task<Task> AuthNewUser(Message message, int IdUser, string Nickname = null)
            {
                try
                {
                    if (!RepUsers.IsAuthUser(IdUser))
                    {
                        App.Current.Dispatcher.Invoke(() => ListUsers.Add(new PatternUser() { IdUser = IdUser, LocalNickname = Nickname }));
                        await GlobalUnit.Api.SendTextMessageAsync(Properties.Settings.Default.AdminId, $"Пользователь c id: \"{IdUser}\" был успешно добавлен в список пользователей.");
                    }
                    else
                        await GlobalUnit.Api.SendTextMessageAsync(Properties.Settings.Default.AdminId, $"Пользователь c id: \"{IdUser}\" уже существует!");
                    await GlobalUnit.Api.SendTextMessageAsync(IdUser, $"Вы были успешно добавлены в список пользователей бота.\nЗапросите у администратора бота {GlobalUnit.Api.GetMeAsync().Result} вас добавить в список разрешённых пользователей.\n\nВы можете написать администратору бота используя команду \"/messageToAdmin YourMessage\"");
                }
                catch (Exception ex)
                {
                    await GlobalUnit.Api.SendTextMessageAsync(Properties.Settings.Default.AdminId, $"Error: При попытке использования команды \"{message.Text}\" пользователем {message.From.Id} произошла ошибка: \n{ex.Message}");
                }
                return SendListOfUsers(message);
            }

            public static Task DeauthUser(Message message, int IdUser)
            {
                if (App.Current.Dispatcher.Invoke(() => ListUsers.Remove(ListUsers.Where(f => f.IdUser == IdUser).DefaultIfEmpty(new PatternUser()).FirstOrDefault())))
                    GlobalUnit.Api.SendTextMessageAsync(message.From.Id, $"Пользователь c id: \"{IdUser}\" был успешно успешно удалён из списка пользователей.");
                else
                    return GlobalUnit.Api.SendTextMessageAsync(message.From.Id, $"Пользователя c id: \"{IdUser}\" не существует...");
                return SendListOfUsers(message);
            }

            public static Task DeauthUser(Message message, string[] Args)
            {
                string LocalNickname = string.Join(" ", Args);
                if (App.Current.Dispatcher.Invoke(() => ListUsers.Remove(ListUsers.Where(f => f.LocalNickname == LocalNickname && !StrToInt(f.LocalNickname).Equals(f.IdUser)).DefaultIfEmpty(new PatternUser()).FirstOrDefault())))
                    GlobalUnit.Api.SendTextMessageAsync(message.From.Id, $"Пользователь c Nickname: \"{LocalNickname}\" был успешно успешно удалён из списка пользователей.");
                else
                    return GlobalUnit.Api.SendTextMessageAsync(message.From.Id, $"Пользователя c Nickname: \"{LocalNickname}\" не существует...");
                return SendListOfUsers(message);
            }

            public static Task ChangeLocalName(Message message, int IdUser, string[] Args)
            {
                var findedUser = ListUsers.Where(f => f.IdUser == IdUser).FirstOrDefault();
                if (findedUser is null)
                    return GlobalUnit.Api.SendTextMessageAsync(message.From.Id, $"Пользователь c id: \"{IdUser}\" не найден...");

                string beforeChangingNickname = findedUser.LocalNickname;
                findedUser.LocalNickname = string.Join(" ", Args.Skip(1));
                GlobalUnit.Api.SendTextMessageAsync(message.From.Id, $"Пользователю c id: \"{IdUser}\" измененён Nickname: \"{beforeChangingNickname}\" => \"{findedUser.LocalNickname}\".");
                return SendListOfUsers(message);
            }

            public static Task LockUser(Message message, int IdUser)
            {
                var findedUser = ListUsers.Where(f => f.IdUser == IdUser).FirstOrDefault();
                if (findedUser is null)
                    return GlobalUnit.Api.SendTextMessageAsync(message.From.Id, $"Пользователь c id: \"{IdUser}\" не найден...");

                findedUser.IsBlocked = findedUser.IsBlocked ? false : true;
                GlobalUnit.Api.SendTextMessageAsync(message.From.Id, $"Пользователь c id: \"{IdUser}\" успешно {(findedUser.IsBlocked ? "заблокирован" : "разблокирован")}.");
                return SendListOfUsers(message);
            }

            public static Task LockUser(Message message, string[] Args)
            {
                string LocalNickname = string.Join(" ", Args);
                var findedUser = ListUsers.Where(f => f.LocalNickname == LocalNickname).FirstOrDefault();
                if (findedUser is null || StrToInt(findedUser.LocalNickname).Equals(findedUser.IdUser))
                    return GlobalUnit.Api.SendTextMessageAsync(message.From.Id, $"Пользователь c Nickname: \"{LocalNickname}\" не найден...");

                findedUser.IsBlocked = findedUser.IsBlocked ? false : true;
                GlobalUnit.Api.SendTextMessageAsync(message.From.Id, $"Пользователь c Nickname: \"{LocalNickname}\" успешно {(findedUser.IsBlocked ? "заблокирован" : "разблокирован")}.");
                return SendListOfUsers(message);
            }
        }
    }
}
