using DevExpress.Mvvm;
using System;
using System.Windows;
using System.Windows.Input;
using TBotHamsti.Services;
using TBotHamsti.Models.Messages;

namespace TBotHamsti.ViewModels
{
    public class SettingsViewModel : BindableBase
    {
        private readonly MessageBus messageBus;

        static SettingsViewModel()
        {
            if (Properties.Settings.Default.UsedDarkTheme)
                ChangeTheInterfaceForDarkTheme();
            else
                ChangeTheInterfaceForLightTheme();
        }

        public SettingsViewModel(MessageBus messageBus)
        {
            this.messageBus = messageBus;
        }

        public ICommand ChangeForDarkTheme => new AsyncCommand(async () =>
        {
            ChangeTheInterfaceForDarkTheme();
            await messageBus.SendTo<LogsViewModel>(new TextMessage("Changed theme on \"Dark\"", HorizontalAlignment.Center));
        }, () => !Properties.Settings.Default.UsedDarkTheme);

        public ICommand ChangeForLightTheme => new AsyncCommand(async () =>
        {
            ChangeTheInterfaceForLightTheme();
            await messageBus.SendTo<LogsViewModel>(new TextMessage("Changed theme on \"Light\"", HorizontalAlignment.Center));
        }, () => Properties.Settings.Default.UsedDarkTheme);

        public ICommand SaveSettingsBot => new DelegateCommand(() =>
        {
            Properties.Settings.Default.Save();
        });

        public ICommand DefaultSettingBot => new DelegateCommand(() =>
        {
            
        });

        private static void ChangeTheInterfaceForDarkTheme()
        {
            Uri uri = new Uri("pack://application:,,,/TBotHamsti;component/Views/Themes/Dark.xaml");
            Application.Current.Resources.MergedDictionaries.RemoveAt(1);
            Application.Current.Resources.MergedDictionaries.Insert(1, new ResourceDictionary() { Source = uri });
            Properties.Settings.Default.UsedDarkTheme = true;
            Properties.Settings.Default.Save();
        }

        private static void ChangeTheInterfaceForLightTheme()
        {
            Uri uri = new Uri($"pack://application:,,,/TBotHamsti;component/Views/Themes/Light.xaml");
            Application.Current.Resources.MergedDictionaries.RemoveAt(1);
            Application.Current.Resources.MergedDictionaries.Insert(1, new ResourceDictionary() { Source = uri });
            Properties.Settings.Default.UsedDarkTheme = false;
            Properties.Settings.Default.Save();
        }
    }
}
