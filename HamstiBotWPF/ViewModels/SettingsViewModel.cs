using DevExpress.Mvvm;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TBotHamsti.Models.Messages;
using TBotHamsti.Services;
using Settings = TBotHamsti.Properties.Settings;

namespace TBotHamsti.ViewModels
{
    public class SettingsViewModel : BindableBase
    {
        private readonly MessageBus messageBus;
        private static readonly Regex regex;
        private static bool isEnabledAutoStartBotDefault;
        private static bool isShowLogsTimeDefault;
        private static double secondsDelayOnReceiveError;
        private static string savePathDefault;
        private bool IsDefaultSettings =>
            isEnabledAutoStartBotDefault.Equals(Settings.Default.IsEnabledAutoStartBot)
            && isShowLogsTimeDefault.Equals(Settings.Default.IsShowLogsTime)
            && secondsDelayOnReceiveError.Equals(Settings.Default.SecondsDelayOnReceiveError)
            && savePathDefault.Equals(Settings.Default.SavePath);


        static SettingsViewModel()
        {
            isShowLogsTimeDefault = Settings.Default.IsShowLogsTime;
            isEnabledAutoStartBotDefault = Settings.Default.IsEnabledAutoStartBot;
            secondsDelayOnReceiveError = Settings.Default.SecondsDelayOnReceiveError;
            savePathDefault = Settings.Default.SavePath;
            regex = new Regex("[^1-9]");

            Settings.Default.UsedDarkTheme = !Settings.Default.UsedDarkTheme;
            ChangeTheInterfaceTheme();
        }

        public SettingsViewModel(MessageBus messageBus)
        {
            this.messageBus = messageBus;
        }

        public ICommand ChangeForDarkTheme => new AsyncCommand(ChangeThemeCommandAction, () => !Settings.Default.UsedDarkTheme);
        public ICommand ChangeForLightTheme => new AsyncCommand(ChangeThemeCommandAction, () => Settings.Default.UsedDarkTheme);

        public ICommand SaveSettingsBot => new AsyncCommand(() =>
        {
            isShowLogsTimeDefault = Settings.Default.IsShowLogsTime;
            isEnabledAutoStartBotDefault = Settings.Default.IsEnabledAutoStartBot;
            secondsDelayOnReceiveError = Settings.Default.SecondsDelayOnReceiveError;
            savePathDefault = Settings.Default.SavePath;
            Settings.Default.Save();
            return messageBus.SendTo<LogsViewModel>(new TextMessage($"Settings saved successfully", HorizontalAlignment.Center));
        }, () => !IsDefaultSettings);

        public ICommand DefaultSettingBot => new AsyncCommand(() =>
        {
            Settings.Default.IsShowLogsTime = isShowLogsTimeDefault;
            Settings.Default.IsEnabledAutoStartBot = isEnabledAutoStartBotDefault;
            Settings.Default.SecondsDelayOnReceiveError = secondsDelayOnReceiveError;
            Settings.Default.SavePath = savePathDefault;
            Settings.Default.Save();
            return messageBus.SendTo<LogsViewModel>(new TextMessage($"Settings restored successfully", HorizontalAlignment.Center));
        }, () => !IsDefaultSettings);

        public ICommand ChangeSavePath => new AsyncCommand(() =>
        {
            using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = fbd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    Settings.Default.SavePath = fbd.SelectedPath;
                    return messageBus.SendTo<LogsViewModel>(new TextMessage($"Save path changed successfully", HorizontalAlignment.Center));
                }

                return messageBus.SendTo<LogsViewModel>(new TextMessage($"Save path changing canceled", HorizontalAlignment.Center));
            }
        });

        public void NumberValidationTextBox(object sender, TextCompositionEventArgs e) => e.Handled = regex.IsMatch(e.Text);
        private static void ChangeTheInterfaceTheme()
        {
            Settings.Default.UsedDarkTheme = !Settings.Default.UsedDarkTheme;
            Uri uri = new Uri("pack://application:,,,/TBotHamsti;component/Views/Themes/" + (Settings.Default.UsedDarkTheme ? "Dark" : "Light") + ".xaml");
            Application.Current.Resources.MergedDictionaries.RemoveAt(1);
            Application.Current.Resources.MergedDictionaries.Insert(1, new ResourceDictionary() { Source = uri });
            Settings.Default.Save();
        }

        private Task ChangeThemeCommandAction()
        {
            ChangeTheInterfaceTheme();
            return messageBus.SendTo<LogsViewModel>(new TextMessage($"Changed theme on \"{(Settings.Default.UsedDarkTheme ? "Dark" : "Light")}\"", HorizontalAlignment.Center));
        }
    }
}
