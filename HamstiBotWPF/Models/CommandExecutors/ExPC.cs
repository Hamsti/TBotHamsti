using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using TBotHamsti.Models.Commands;
using TBotHamsti.Models.Users;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using File = System.IO.File;
using User = TBotHamsti.Models.Users.User;

namespace TBotHamsti.Models.CommandExecutors
{
    public static class ExPC
    {
        public static Task ExecuteUrl(ICommand model, User user, Message message)
        {
            string url = model.Args.FirstOrDefault();
            Process.Start(url = !url.StartsWith("https://") || !url.StartsWith("http://") ? "https://" + url : url);
            return user.SendMessageAsync($"Opening a site by link: " + url);
        }

        public static Task TurnOff(User user, int tMin, int tSec)
        {
            while (tSec >= 60)
            {
                tSec -= 60;
                tMin++;
            }

            ExecuteCmdCommand(@"C:\Windows\System32\shutdown.exe", "/s /t " + (tMin * 60 + tSec));
            return user.SendMessageAsync($"Timer set successfully.\nIn {tMin} min. {tSec} sec. the workstation will be turned off.");
        }

        public static void ExecuteCmdCommand(string programmPath, string cmdArgs)
        {
            try
            {
                Process.Start(programmPath, cmdArgs);
            }
            catch (Exception ex)
            {
                ex.AppendExceptionMessage("An error occurred while executing the console command");
                throw;
            }
        }

        public static async Task ImageUploaderAsync(User user, Message message, StatusUser requiredStatusUser = StatusUser.User)
        {
            CheckDirectoryExists();
            if (user.Status < requiredStatusUser)
            {
                throw new ArgumentException($"To execute, the user status is required {requiredStatusUser} and higher", nameof(user.Status));
            }

            try
            {
                var file = await App.Api.GetFileAsync(message.Photo.LastOrDefault()?.FileId);
                var filename = file.FileId + "." + file.FilePath.Split('.').Last();
                using (var saveImageStream = File.Open(Properties.Settings.Default.SavePath + @"photos\" + filename, FileMode.Create))
                {
                    await App.Api.DownloadFileAsync(file.FilePath, saveImageStream);
                }
                await user.SendMessageAsync("Image upload completed successfully");
            }
            catch (Exception ex)
            {
                ex.AppendExceptionMessage("An error occurred while loading the image");
                throw;
            }
        }

        public static async Task DocumentUploaderAsync(User user, Message message, StatusUser requiredStatusUser = StatusUser.User)
        {
            CheckDirectoryExists();
            if (user.Status < requiredStatusUser)
            {
                throw new ArgumentException($"To execute, the user status is required {requiredStatusUser} and higher", nameof(user.Status));
            }

            try
            {
                var file = await App.Api.GetFileAsync(message.Document.FileId);
                using (var saveImageStream = File.Open(Properties.Settings.Default.SavePath + @"documents\" + message.Document.FileName, FileMode.Create))
                {
                    await App.Api.DownloadFileAsync(file.FilePath, saveImageStream);
                }
                await user.SendMessageAsync("Document upload completed successfully");
            }
            catch (Exception ex)
            {
                ex.AppendExceptionMessage($"An error occurred while loading the document");
                throw;
            }
        }

        public static Task GetScreenshot(User user)
        {
            CheckDirectoryExists();
            try
            {
                string fileName = "screen-" + DateTime.Now.ToString("ddMMyyyy-hhmmss") + ".png";
                string fullFilePath = Properties.Settings.Default.SavePath + "\\" + fileName;
                using (Bitmap bmp = new Bitmap((int)SystemParameters.VirtualScreenWidth, (int)SystemParameters.VirtualScreenHeight))
                {
                    using Graphics graph = Graphics.FromImage(bmp);
                    graph.CopyFromScreen((int)SystemParameters.VirtualScreenLeft, (int)SystemParameters.VirtualScreenTop, 0, 0, bmp.Size);
                    bmp.Save(fullFilePath);
                }

                App.Api.SendDocumentAsync(user.Id, new InputOnlineFile(File.OpenRead(fullFilePath), fileName)).Wait();
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                ex.AppendExceptionMessage("An error occurred while working with the screenshot");
                throw;
            }
        }

        private static void CheckDirectoryExists()
        {
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.SavePath) || !Directory.Exists(Properties.Settings.Default.SavePath))
            {
                throw new ArgumentException("The path for saving files is incorrectly configured", nameof(Properties.Settings.Default.SavePath));
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


            public static Task ChangeVolume(User user, string arg)
            {
                static void ChangeVolume(int appCommand) =>
                    SendMessageW(Process.GetCurrentProcess().MainWindowHandle, WM_APPCOMMAND, Process.GetCurrentProcess().MainWindowHandle, (IntPtr)appCommand);

                switch (arg)
                {
                    case "0": throw new ArgumentOutOfRangeException($"Volume change to \"{arg}\" is not valid");
                    case "mute":
                        ChangeVolume(APPCOMMAND_VOLUME_MUTE);
                        break;
                    case var _ when int.TryParse(arg, out int vol):
                        if (vol < -100 || vol > 100)
                        {
                            throw new ArgumentOutOfRangeException("Out of bounds: [-100; 100]");
                        }

                        bool isVolumeUp = vol > 0;
                        vol = (int)Math.Abs(Math.Round((double)vol / 2, 0, MidpointRounding.AwayFromZero));

                        for (int i = 0; i < vol; i++)
                        {
                            ChangeVolume(isVolumeUp ? APPCOMMAND_VOLUME_UP : APPCOMMAND_VOLUME_DOWN);
                        }
                        break;
                    default: throw new ArgumentException("Invalid argument", nameof(arg));
                }

                return user.SendMessageAsync($"Volume changed to: {arg}");
            }
        }
    }
}