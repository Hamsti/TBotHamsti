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

namespace HamstiBotWPF.LogicRepository
{
    /// <summary>
    /// Implementation of all bot functionality
    /// </summary>
    public static class RepBotActions
    {
        public static Task helpBot(Message message) => GlobalUnit.myBot.Api.SendTextMessageAsync(message.From.Id, "Список команд у бота:\n" + string.Join("\n", GlobalUnit.botCommands.Where(v => v.VisibleCommand == true).Select(s => s.ExampleCommand)));
        public static Task helpBotAdmin(Message message)
        {
            if (RepUsers.isHaveAccessAdmin(message.From.Id))
                return GlobalUnit.myBot.Api.SendTextMessageAsync(message.From.Id, "Список всех реализованных команд у бота:\n" + string.Join("\n", GlobalUnit.botCommands.Select(s => s.ExampleCommand)));
            else
                return GlobalUnit.myBot.Api.SendTextMessageAsync(message.From.Id, "Вы не администратор...");
        }

        public static async void photoUploader(Message message)
        {
            try
            {
                var file = await GlobalUnit.myBot.Api.GetFileAsync(message.Photo.LastOrDefault()?.FileId);
                var filename = file.FileId + "." + file.FilePath.Split('.').Last();
                using (var saveImageStream = System.IO.File.Open(Properties.Settings.Default.SavePathImages + filename, FileMode.Create))
                {
                    await GlobalUnit.myBot.Api.DownloadFileAsync(file.FilePath, saveImageStream);
                }
                await GlobalUnit.myBot.Api.SendTextMessageAsync(message.From.Id, "Загрузка изображения успешно завершена");
            }
            catch (Exception ex)
            {
                await GlobalUnit.myBot.Api.SendTextMessageAsync(message.From.Id, $"При загрузке изображения произошла ошибка: {ex.Message}");
            }
        }

        public static async void comStopBot(Message message)
        {
            if (RepUsers.isHaveAccessAdmin(message.From.Id))
            {
                await ExecuteLaunchBot.stopBot();
            }
            else
                await GlobalUnit.myBot.Api.SendTextMessageAsync(message.From.Id, "Вы не администратор...");
        }

        public static async void comStopApp(Message message)
        {
            if (RepUsers.isHaveAccessAdmin(message.From.Id))
            {
                await GlobalUnit.myBot.Api.SendTextMessageAsync(message.From.Id, $"Принудительное завершение работы приложения пользователем: {Environment.UserDomainName}");
                Environment.Exit(0);
            }
            else
                await GlobalUnit.myBot.Api.SendTextMessageAsync(message.From.Id, "Вы не администратор...");
        }

        public static Task showScreenButtons(Message message, string Show)
        {
            string[] keys;
            parserKeys(out keys, Show, message.From.Id);

            if (keys == null)
                return GlobalUnit.myBot.Api.SendTextMessageAsync(message.Chat.Id, "Не найден аргумент для изменения аргумента");

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
                return GlobalUnit.myBot.Api.SendTextMessageAsync(message.Chat.Id, "Экранная клавиатура успешно удалена.", replyMarkup: new ReplyKeyboardRemove());
            else
                return GlobalUnit.myBot.Api.SendTextMessageAsync(message.Chat.Id, $"Количество добавленных кнопок: {keys.Count()}", replyMarkup: rkm);
        }

        private static void parserKeys(out string[] keys, string Show, int idUser)
        {
            bool isShow;
            if (bool.TryParse(Show, out isShow))
            {
                if (isShow)
                {
                    keys = new string[GlobalUnit.botCommands.Where(x => x.VisibleCommand).Count()];
                    if (RepUsers.isHaveAccessAdmin(idUser))
                        keys = GlobalUnit.botCommands.Where(x => x.CountArgsCommand == 0).Select(s => s.Command).ToArray();
                    else
                        keys = GlobalUnit.botCommands.Where(x => x.CountArgsCommand == 0 && x.VisibleCommand).Select(s => s.Command).ToArray();
                }
                else
                    keys = new string[0];
            }
            else
            {
                if (Show.ToLower() == "all")
                {
                    keys = new string[GlobalUnit.botCommands.Count()];
                    if (RepUsers.isHaveAccessAdmin(idUser))
                        keys = GlobalUnit.botCommands.Where(w => w.Command != "/helpAdmin").Select(s => s.Command).ToArray();
                    else
                        keys = GlobalUnit.botCommands.Where(w => w.VisibleCommand).Select(s => s.Command).ToArray();
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
                    GlobalUnit.myBot.Api.SendTextMessageAsync(message.From.Id, $"Message: {message.Text.Split(' ').FirstOrDefault()} || Arg: {Url}");
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
                GlobalUnit.myBot.Api.SendTextMessageAsync(message.From.Id, $"Таймер успешно установлен.\nЧерез {tMin} мин. {tSec} сек. ваш ПК будет выключен.");
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
                            GlobalUnit.myBot.Api.SendTextMessageAsync(message.From.Id, $"Изменение громкости на: {Value} не является допустимым");
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
                            GlobalUnit.myBot.Api.SendTextMessageAsync(message.From.Id, $"Громкость изменена на: {Value}");
                        }
                        else
                        {
                            GlobalUnit.myBot.Api.SendTextMessageAsync(message.From.Id, "Выход за пределы: [-100;100] при изменении громкости");
                        }
                    }
                    else
                    {
                        if (strValue.ToLower() == "mute")
                        {
                            SendMessageW(Process.GetCurrentProcess().MainWindowHandle, WM_APPCOMMAND, Process.GetCurrentProcess().MainWindowHandle, (IntPtr)APPCOMMAND_VOLUME_MUTE);
                            GlobalUnit.myBot.Api.SendTextMessageAsync(message.From.Id, $"Громкость изменена на: {strValue}");
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
                        await GlobalUnit.myBot.Api.SendPhotoAsync(message.From.Id, new Telegram.Bot.Types.InputFiles.InputOnlineFile(stream, stream.Name));
                    }
                }
                catch (Exception ex)
                {
                    await GlobalUnit.myBot.Api.SendTextMessageAsync(message.From.Id, $"При работе со скриншотом, произошла ошибка: {ex.Message}");
                }
            }
        }
    
        private static class ControlUsers
        {
            public static Task authNewUser(Message message, int idUser) => null;
            public static Task deauthUser(Message message, int idUser) => null;
            public static Task changeLocalName(Message message, int idUser, string newLocalName) => null;
            public static Task lockedUser(Message message, int idUser) => null;
        }
    }
}
