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

namespace HamstiBotWPF.LogicRepository
{
    /// <summary>
    /// Implementation of all bot functionality
    /// </summary>
    public static class RepBotActions
    {
        public static string helpForUsers { get { return (GlobalUnit.currentLevelCommand != Core.BotLevelCommand.LevelCommand.Root ? "/..\n" : string.Empty) + 
                    string.Join("\n", GlobalUnit.botCommands.Where(w => (w.VisibleForUsers == true && w.Command != "/" + Core.BotLevelCommand.LevelCommand.Root.ToString().ToUpper()) && 
                    GlobalUnit.currentLevelCommand == (w.GetType().Equals(typeof(Core.BotCommand)) ? w.NameOfLevel : ((Core.BotLevelCommand)w).ParrentLevel)).
                    Select(s => s.ExampleCommand ?? ((Core.BotLevelCommand)s).ExampleCommand)); } }
        public static string helpForAdmin { get { return (GlobalUnit.currentLevelCommand != Core.BotLevelCommand.LevelCommand.Root ? "/..\n" : string.Empty) + 
                    string.Join("\n", GlobalUnit.botCommands.Where(w => w.Command != "/" + Core.BotLevelCommand.LevelCommand.Root.ToString().ToUpper() && 
                    GlobalUnit.currentLevelCommand == (w.GetType().Equals(typeof(Core.BotCommand)) ? w.NameOfLevel : ((Core.BotLevelCommand)w).ParrentLevel)).
                    Select(s => s.ExampleCommand ?? ((Core.BotLevelCommand)s).ExampleCommand)); } }
        public static Task helpBot(Message message) => GlobalUnit.Api.SendTextMessageAsync(message.From.Id, "Список команд у бота:\n" + helpForUsers);
        public static Task helpBotAdmin(Message message) => GlobalUnit.Api.SendTextMessageAsync(message.From.Id, "Список всех реализованных команд у бота:\n" + helpForAdmin);

        public static async void UserSendMessageForAdmin(Message message)
        {
            if (message.Type == Telegram.Bot.Types.Enums.MessageType.Text)
            {
                await GlobalUnit.Api.SendTextMessageAsync(Properties.Settings.Default.AdminId,
                    $"Сообщение от пользователя \n(id: {message.From.Id} || IsBlocked: {!RepUsers.IsAuthNotIsBlockedUser(message.From.Id)}):\n{String.Join(" ", Core.BotCommand.ParserCommand(message.Text).Args)}");
                await GlobalUnit.Api.SendTextMessageAsync(message.From.Id, "Сообщение успешно отправлено админу бота " + GlobalUnit.Api.GetMeAsync().Result);
            }
            else
            {
                await GlobalUnit.Api.SendTextMessageAsync(message.From.Id, "Разрешается использовать только текстовые сообщения. Сообщение не было отправлено.");
            }
        }

        public static async void AdminSendMessageToUser(Message message, int IdUser)
        {
            if (RepUsers.IsAuthUser(IdUser) && message.Type == Telegram.Bot.Types.Enums.MessageType.Text)
            {
                try
                {
                    await GlobalUnit.Api.SendTextMessageAsync(IdUser,
                        $"Сообщение от администратора бота {GlobalUnit.Api.GetMeAsync().Result}: \n{String.Join(" ", Core.BotCommand.ParserCommand(message.Text).Args.Skip(1))}" +
                        $"\nВы можете написать администратору бота используя команду \"/messageToAdmin YourMessage\"");
                    await GlobalUnit.Api.SendTextMessageAsync(message.From.Id, "Сообщение успешно отправлено пользователю " + IdUser);
                }
                catch (Telegram.Bot.Exceptions.ApiRequestException ex)
                {
                    await GlobalUnit.Api.SendTextMessageAsync(Properties.Settings.Default.AdminId, $"При отправке сообщения пользователю с id:{IdUser} произошла ошибка Telegram.Bot.Exceptions.ApiRequestException:\n\n{ex.Message}");
                }
                catch (Exception ex)
                {
                    await GlobalUnit.Api.SendTextMessageAsync(Properties.Settings.Default.AdminId, $"При отправке сообщения пользователю с id:{IdUser} произошла системная ошибка:\n\n{ex.Message}");
                }
            }
            else
            {
                await GlobalUnit.Api.SendTextMessageAsync(message.From.Id, "Разрешается использовать только текстовые сообщения. Сообщение не было отправлено.");
            }
        }
        public static async void AdminSpamMessageToUser(Message message, int IdUser, int CountMessages)
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
                        await GlobalUnit.Api.SendTextMessageAsync(IdUser, RepBotActions.RandomString(new Random().Next(5, 40)));
                    }
                    await GlobalUnit.Api.SendTextMessageAsync(IdUser, "Спам успешно завершён. Хорошего дня ;)");
                    await GlobalUnit.Api.SendTextMessageAsync(Properties.Settings.Default.AdminId, "Спам пользователя " + IdUser + " успешно завершён.");
                }
                catch (Telegram.Bot.Exceptions.ApiRequestException ex)
                {
                    await GlobalUnit.Api.SendTextMessageAsync(Properties.Settings.Default.AdminId, $"При отправке сообщения пользователю с id:{IdUser} произошла ошибка Telegram.Bot.Exceptions.ApiRequestException:\n\n{ex.Message}");
                }
                catch (Exception ex)
                {
                    await GlobalUnit.Api.SendTextMessageAsync(Properties.Settings.Default.AdminId, $"При отправке сообщения пользователю с id:{IdUser} произошла системная ошибка:\n\n{ex.Message}");
                }
            }
            else
            {
                await GlobalUnit.Api.SendTextMessageAsync(message.From.Id, "Разрешается использовать только текстовые сообщения. Сообщение не было отправлено.");
            }
        }

        public static string RandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static async void imageUploader(Message message)
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

        public static async void documentUploader(Message message)
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

        public static async void comStopBot(Message message)
        {
            await ExecuteLaunchBot.StopBotAsync();
        }

        public static async void comStopApp(Message message)
        {
            await GlobalUnit.Api.SendTextMessageAsync(message.From.Id, $"Принудительное завершение работы приложения пользователем: {Environment.UserDomainName}");
            await ExecuteLaunchBot.StopBotAsync();
            Environment.Exit(0);
        }

        public static Task showScreenButtons(Message message, string Show)
        {
            string[] keys;
            parserKeys(out keys, Show, message.From.Id);

            if (keys == null)
                return GlobalUnit.Api.SendTextMessageAsync(message.Chat.Id, "Не найден аргумент для изменения аргумента");

            int countColsKeys = 3;
            var rkm = new ReplyKeyboardMarkup();
            var rows = new List<KeyboardButton[]>();
            var cols = new List<KeyboardButton>();

            for (var i = 0; i < keys.Count(); i++)
            {
                cols.Add(new KeyboardButton(keys[i]));
                if ((i+1) % countColsKeys != 0 && i+1!=keys.Count()) continue;
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

        private static void parserKeys(out string[] keys, string Show, int IdUser)
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

        public static class ControlPC
        {
            public static void executeUrl(Message message, string command, string Url)
            {
                if (!Url.StartsWith("https://") || !Url.StartsWith("http://"))
                {
                    Process.Start(Url.Insert(0, "https://"));
                    GlobalUnit.Api.SendTextMessageAsync(message.From.Id, $"Message: {message.Text.Split(' ').FirstOrDefault()} || Arg: {Url}");
                }
            }

            public static void turnOff(Message message, int tMin, int tSec)
            {
                while (tSec >= 60)
                {
                    tSec -= 60;
                    tMin++;
                }
                Process.Start(@"C:\Windows\System32\shutdown.exe", "/s /t " + (tMin * 60 + tSec));
                GlobalUnit.Api.SendTextMessageAsync(message.From.Id, $"Таймер успешно установлен.\nЧерез {tMin} мин. {tSec} сек. ваш ПК будет выключен.");
            }

            public static bool cmdCommands(Message message, string programmPath, string cmdArgs)
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
            
            public async static void getScreenshot(Message message, string nameOfPicture = "Screenshot.png")
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
            private static ObservableCollection<Core.PatternUser> ListUsers => GlobalUnit.authUsers;
            public static int StrToInt(string IdUserString)
            {
                int.TryParse(IdUserString, out int idNewUser);
                return idNewUser;
            }

            public static string ListOfUsersParseString
            {
                get
                {
                    System.Text.StringBuilder messageText = new System.Text.StringBuilder();
                    RepUsers.Sort();
                    foreach (var user in ListUsers)
                        messageText.Append(user.IdUser_Nickname + " | IsBlocked: " + user.IsBlocked + "\n");
                    return messageText.ToString();
                }
            }

            public static async void authNewUser(Message message, int IdUser)
            {
                try
                {
                    App.Current.Dispatcher.Invoke(() => ListUsers.Add(new Core.PatternUser() { IdUser = IdUser }));
                    await GlobalUnit.Api.SendTextMessageAsync(Properties.Settings.Default.AdminId, $"Пользователь c id:{IdUser} был успешно добавлен в список пользователей.\n\nСписок пользоватей бота:\n" + ListOfUsersParseString);
                    await GlobalUnit.Api.SendTextMessageAsync(message.From.Id, $"Вы были успешно добавлены в список пользователей бота.\nЗапросите у администратора бота {GlobalUnit.Api.GetMeAsync().Result} вас добавить в список разрешённых пользователей.\n\nВы можете написать администратору бота используя команду \"/messageToAdmin YourMessage\"");
                }
                catch (Exception ex)
                {
                    await GlobalUnit.Api.SendTextMessageAsync(Properties.Settings.Default.AdminId, $"Error: При попытке использования команды \"{message.Text}\" пользователем {message.From.Id} произошла ошибка: \n{ex.Message}");
                }
                //GlobalUnit.authUsers.Ad
            }

            public static void deauthUser(Message message, int IdUser) => ListUsers.Remove(ListUsers.Where(f => f.IdUser == IdUser).FirstOrDefault());
            public static void deauthUser(Message message, string LocalNickname) => ListUsers.Remove(ListUsers.Where(f => f.LocalNickname == LocalNickname).FirstOrDefault() );
            public static void changeLocalName(Message message, int IdUser, string LocalNickname) => ListUsers.Where(f => f.IdUser == IdUser).Select(s => s.LocalNickname = LocalNickname);
            public static void lockUser(Message message, int IdUser) => ListUsers.Where(f => f.IdUser == IdUser).Select(s => s.IsBlocked = true);
            public static void lockUser(Message message, string LocalNickname) => ListUsers.Where(f => f.LocalNickname == LocalNickname).Select(s => s.IsBlocked = true);
        }
    }
}
