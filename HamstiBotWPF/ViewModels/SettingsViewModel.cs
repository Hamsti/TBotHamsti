using DevExpress.Mvvm;
using System;
using System.Linq;
using HamstiBotWPF.Services;
using System.Windows.Input;
using HamstiBotWPF.Core;
using HamstiBotWPF.Events;

namespace HamstiBotWPF.ViewModels
{
    public class SettingsViewModel : BindableBase
    {
        private readonly MessageBus messageBus;
        //private int IdAdminForStartApp { get; set; }

        public SettingsViewModel(MessageBus messageBus)
        {
            this.messageBus = messageBus;
            if (Properties.Settings.Default.UsedDarkTheme)
                ChangeTheInterfaceForDarkTheme();
            else
                ChangeTheInterfaceForLightTheme();

            //IdAdminForStartApp = Properties.Settings.Default.AdminId;
        }

        public ICommand ChangeForDarkTheme => new DelegateCommand((obj) =>
        {
            ChangeTheInterfaceForDarkTheme();
        }, (obj) => !Properties.Settings.Default.UsedDarkTheme);

        public ICommand ChangeForLightTheme => new DelegateCommand((obj) =>
        {
            ChangeTheInterfaceForLightTheme();
        }, (obj) => Properties.Settings.Default.UsedDarkTheme);

        //public ICommand SaveSettingsBot => new AsyncCommand(async () =>
        //{
        //    Properties.Settings.Default.Save();
        //    PatternUser newAdminUser = GlobalUnit.AuthUsers.FirstOrDefault(f => f.IdUser == Properties.Settings.Default.AdminId);
        //    await messageBus.SendTo<LogsViewModel>(new Messages.TextMessage($"New bot administrator: {(newAdminUser != null ? newAdminUser.IdUser_Nickname : "Unauthorized user")}"));
        //    IdAdminForStartApp = Properties.Settings.Default.AdminId;
        //    //await eventBus.Publish(new RefreshUsersListEvent());
        //});
        ////() => IdAdminForStartApp != Properties.Settings.Default.AdminId);

        //public ICommand DefaultSettingBot => new AsyncCommand(async () =>
        //{
        //    Properties.Settings.Default.AdminId = Properties.Settings.Default.RecoverIdAdmin;
        //    Properties.Settings.Default.Save();
        //    PatternUser newAdminUser = GlobalUnit.AuthUsers.FirstOrDefault(f => f.IdUser == Properties.Settings.Default.AdminId);
        //    await messageBus.SendTo<LogsViewModel>(new Messages.TextMessage($"Restored default bot admin: {(newAdminUser != null ? newAdminUser.IdUser_Nickname : "Unauthorized user")}"));
        //    IdAdminForStartApp = Properties.Settings.Default.RecoverIdAdmin;
        //    //await eventBus.Publish(new RefreshUsersListEvent());
        //});
        ////() => Properties.Settings.Default.AdminId != Properties.Settings.Default.RecoverIdAdmin);

        private void ChangeTheInterfaceForDarkTheme()
        {
            Uri uri = new Uri($"pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Dark.xaml");
            System.Windows.Application.Current.Resources.MergedDictionaries.RemoveAt(1);
            System.Windows.Application.Current.Resources.MergedDictionaries.Insert(1, new System.Windows.ResourceDictionary() { Source = uri });
            uri = new Uri("pack://application:,,,/HamstiBotWPF;component/Themes/Dark.xaml");
            System.Windows.Application.Current.Resources.MergedDictionaries.RemoveAt(3);
            System.Windows.Application.Current.Resources.MergedDictionaries.Insert(3, new System.Windows.ResourceDictionary() { Source = uri });
            Properties.Settings.Default.UsedDarkTheme = true;
            Properties.Settings.Default.Save();
        }

        private void ChangeTheInterfaceForLightTheme()
        {
            Uri uri = new Uri($"pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Light.xaml");
            System.Windows.Application.Current.Resources.MergedDictionaries.RemoveAt(1);
            System.Windows.Application.Current.Resources.MergedDictionaries.Insert(1, new System.Windows.ResourceDictionary() { Source = uri });
            uri = new Uri($"pack://application:,,,/HamstiBotWPF;component/Themes/Light.xaml");
            System.Windows.Application.Current.Resources.MergedDictionaries.RemoveAt(3);
            System.Windows.Application.Current.Resources.MergedDictionaries.Insert(3, new System.Windows.ResourceDictionary() { Source = uri });
            Properties.Settings.Default.UsedDarkTheme = false;
            Properties.Settings.Default.Save();
        }
    }
}
