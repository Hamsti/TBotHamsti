using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using TBotHamsti.Models.Commands;
using TBotHamsti.Models.Users;
using Telegram.Bot.Types;
using User = TBotHamsti.Models.Users.User;

namespace TBotHamsti.Models.CommandExecutors
{
    public static class ExPC
    {
        public static Task ExecuteUrl(ICommand model, User user, Message message)
        {
            string url = model.Args.FirstOrDefault();
            if (!url.StartsWith("https://") || !url.StartsWith("http://"))
            {
                Process.Start(url.Insert(0, "https://"));
                return user.SendMessageAsync($"Message: {message.Text.Split(' ').FirstOrDefault()} || Arg: {url}");
            }
            return Task.CompletedTask;
        }

        public static Task TurnOff(User user, int tMin, int tSec)
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


            public static Task ChangeVolume(User user, string strValue)
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

        public async static Task GetScreenshot(User user)
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
}