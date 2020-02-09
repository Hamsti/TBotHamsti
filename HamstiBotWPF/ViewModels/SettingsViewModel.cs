using DevExpress.Mvvm;
using System;
using System.Linq;
using HamstiBotWPF.Services;
using System.Windows.Input;
using HamstiBotWPF.Core;
using HamstiBotWPF.Events;

namespace HamstiBotWPF.ViewModels
{
    public class SettingsViewModel 
    {
        private readonly MessageBus messageBus;
        private readonly EventBus eventBus;
        private int IdAdminForStartApp { get; set; }

        public SettingsViewModel(MessageBus messageBus, EventBus eventBus)
        {
            this.messageBus = messageBus;
            this.eventBus = eventBus;
            IdAdminForStartApp = Properties.Settings.Default.AdminId;
        }

        public ICommand ChangeTheInterfaceForDarkTheme => new DelegateCommand((obj) =>
        {
            Uri uri = new Uri($"pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Dark.xaml");
            System.Windows.Application.Current.Resources.MergedDictionaries.RemoveAt(0);
            System.Windows.Application.Current.Resources.MergedDictionaries.Insert(0, new System.Windows.ResourceDictionary() { Source = uri });
        });

        public ICommand ChangeTheInterfaceForLightTheme => new DelegateCommand((obj) =>
        {
            Uri uri = new Uri($"pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Light.xaml");
            System.Windows.Application.Current.Resources.MergedDictionaries.RemoveAt(0);
            System.Windows.Application.Current.Resources.MergedDictionaries.Insert(0, new System.Windows.ResourceDictionary() { Source = uri });
        });

        public ICommand SaveSettingsBot => new AsyncCommand(async () =>
        {
            Properties.Settings.Default.Save();
            PatternUserList newAdminUser = GlobalUnit.authUsers.FirstOrDefault(f => f.IdUser == Properties.Settings.Default.AdminId);
            await messageBus.SendTo<LogsViewModel>(new Messages.TextMessage($"New bot administrator: {(newAdminUser != null ? newAdminUser.IdUser_Nickname : "Unauthorized user")}"));
            IdAdminForStartApp = Properties.Settings.Default.AdminId;
            await eventBus.Publish(new RefreshUsersListEvent());
        });
        //() => IdAdminForStartApp != Properties.Settings.Default.AdminId);

        public ICommand DefaultSettingBot => new AsyncCommand(async () =>
        {
            Properties.Settings.Default.AdminId = Properties.Settings.Default.RecoverIdAdmin;
            Properties.Settings.Default.Save();
            PatternUserList newAdminUser = GlobalUnit.authUsers.FirstOrDefault(f => f.IdUser == Properties.Settings.Default.AdminId);
            await messageBus.SendTo<LogsViewModel>(new Messages.TextMessage($"Restored default bot admin: {(newAdminUser != null ? newAdminUser.IdUser_Nickname : "Unauthorized user")}"));
            IdAdminForStartApp = Properties.Settings.Default.RecoverIdAdmin;
            await eventBus.Publish(new RefreshUsersListEvent());
        });
        //() => Properties.Settings.Default.AdminId != Properties.Settings.Default.RecoverIdAdmin);
    }
}
