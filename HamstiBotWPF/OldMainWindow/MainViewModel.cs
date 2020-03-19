using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace HamstiBotWPF.OldMainWindow
{
    public class MainViewModel : ViewModelBase
    {
        private int IdAdminForStartApp { get; set; } = Properties.Settings.Default.AdminId;
        public List<System.Windows.Controls.MenuItem> ListCommands { get; private set; }
        public ObservableCollection<string> ListLogs { get; private set; }
        public ObservableCollection<Core.PatternUser> ListUsers => GlobalUnit.AuthUsers;

        private Core.PatternUser _SelectedUserList = new Core.PatternUser();
        public Core.PatternUser SelectedUserList
        {
            get { return _SelectedUserList == null ? new Core.PatternUser() : _SelectedUserList; }
            set
            {
                _SelectedUserList = value;
                RaisePropertiesChanged();
            }
        }


        public MainViewModel()
        {
            LogicRepository.RepCommands.Refresh();
            LogicRepository.RepUsers.Upload();
            subBotEvents();
            ListCommands = new List<System.Windows.Controls.MenuItem>();
            foreach (var command in GlobalUnit.botCommands)
            {
                ListCommands.Add(new System.Windows.Controls.MenuItem()
                {
                    Header = command.ExampleCommand,
                    Foreground = command.VisibleForUsers ? System.Windows.Media.Brushes.White : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(97, 216, 162))
                });
            }

            ListLogs = new ObservableCollection<string>();

            //ListUsers = new ObservableCollection<Core.PatternUser>();
            ListUsersRefresh();

        }

        private void subBotEvents()
        {
            GlobalUnit.Api.OnMessage += ExecuteLaunchBot.CheckMessageBot;
            GlobalUnit.Api.OnMessageEdited += ExecuteLaunchBot.CheckMessageBot;
            GlobalUnit.Api.OnMessageEdited += botOnMessageReceived;
            GlobalUnit.Api.OnMessage += botOnMessageReceived;
            GlobalUnit.Api.OnReceiveError += botOnReceiveError;
            GlobalUnit.Api.OnReceiveGeneralError += Api_OnReceiveGeneralError;
            //GlobalUnit.Api.OnUpdate += (obj, e) => App.Current.Dispatcher.Invoke(() => ListLogs.Add("OnUpdate"));
        }

        

        /// <summary>
        /// Adding to the log notification of an incoming message
        /// </summary>
        private void botOnMessageReceived(object sender, Telegram.Bot.Args.MessageEventArgs messageEventArgs)
        {
            switch (messageEventArgs.Message.Type)
            {
                case Telegram.Bot.Types.Enums.MessageType.Text:
                    App.Current.Dispatcher.Invoke(() => ListLogs.Add($"Пришло сообщение: {messageEventArgs.Message.Text}")); break;
                case Telegram.Bot.Types.Enums.MessageType.Photo:
                    App.Current.Dispatcher.Invoke(() => ListLogs.Add($"Пришло сообщение: {messageEventArgs.Message.Photo}")); break;
                default:
                    App.Current.Dispatcher.Invoke(() => ListLogs.Add($"Пришло сообщение формата: {messageEventArgs.Message.Type}")); break;
            }
            RaisePropertiesChanged("ClearLogsBot");
        }

        private void botOnReceiveError(object sender, Telegram.Bot.Args.ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            App.Current.Dispatcher.Invoke(() => ListLogs.Add(string.Format("Произошла ошибка при прослушивании: {0} — {1}",
                receiveErrorEventArgs.ApiRequestException.ErrorCode,
                receiveErrorEventArgs.ApiRequestException.Message)));
            RaisePropertiesChanged("ClearLogsBot");
        }

        private void Api_OnReceiveGeneralError(object sender, Telegram.Bot.Args.ReceiveGeneralErrorEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() => ListLogs.Add("Произошла ошибка при событии OnReceiveGeneralError:\n\n" + e.Exception.Message));
            RaisePropertiesChanged("ClearLogsBot");
        }

        public void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        public async void WindowClosing_StopReceivingBot(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (GlobalUnit.Api.IsReceiving)
                {
                    if (System.Windows.MessageBox.Show("The bot is still running, are you sure you want to shut down the bot and close the application?",
                        "HamstiBot", System.Windows.MessageBoxButton.OKCancel, System.Windows.MessageBoxImage.Warning) == System.Windows.MessageBoxResult.OK)
                    {
                        await ExecuteLaunchBot.StopBotAsync();
                        App.Current.Shutdown(0);
                    }
                    else
                        e.Cancel = true;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Завершение приложения произошло с системной ошибкой: {ex.Message}");
            }
        }

        private void ListUsersRefresh()
        {
            ListUsers.Clear();
            GlobalUnit.AuthUsers.OrderBy(ord => ord.IsBlocked).ToList().ForEach(user => ListUsers.Add(new Core.PatternUser()
            {
                IdUser = user.IdUser,
                LocalNickname = user.LocalNickname,
                IsBlocked = user.IsBlocked
            }));
            if (ListUsers.Count > 0 && Properties.Settings.Default.AdminId > 0)
                ListUsers.Move(ListUsers.IndexOf(ListUsers.SingleOrDefault(s => s.IdUser == Properties.Settings.Default.AdminId)), 0);
            Properties.Settings.Default.Reload();
        }

        public ICommand StartBot
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    try
                    {
                        ListLogs.Add("Start bot");
                        Task.Run(() => ExecuteLaunchBot.RunBotAsync());
                    }
                    catch (Exception ex)
                    {
                        ListLogs.Add($"При запуске бота произошла системная ошибка: {ex.Message}");
                    }
                });
            }
        }

        public ICommand StopBot
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    try
                    {
                        ListLogs.Add("Stop bot");
                        Task.Run(() => ExecuteLaunchBot.StopBotAsync());
                    }
                    catch (Exception ex)
                    {
                        ListLogs.Add($"При остановке бота произошла системная ошибка: {ex.Message}");
                    }
                });
            }
        }

        public ICommand SaveSettingsBot => new DelegateCommand(
            (obj) =>
            {
                Properties.Settings.Default.Save();
                Core.PatternUser newAdminUser = GlobalUnit.AuthUsers.FirstOrDefault(f => f.IdUser == Properties.Settings.Default.AdminId);
                ListLogs.Add($"New bot administrator: {(newAdminUser != null ? newAdminUser.IdUser_Nickname : "Unauthorized user")}"); ;
                IdAdminForStartApp = Properties.Settings.Default.AdminId;
                ListUsersRefresh();
            },
            (obj) => IdAdminForStartApp != Properties.Settings.Default.AdminId);

        public ICommand DefaultSettingBot => new DelegateCommand(
            (obj) =>
            {
                Properties.Settings.Default.AdminId = Properties.Settings.Default.RecoverIdAdmin;
                Properties.Settings.Default.Save();
                Core.PatternUser newAdminUser = GlobalUnit.AuthUsers.FirstOrDefault(f => f.IdUser == Properties.Settings.Default.AdminId);
                ListLogs.Add($"Restored default bot admin: {(newAdminUser != null ? newAdminUser.IdUser_Nickname : "Unauthorized user")}"); ;
                IdAdminForStartApp = Properties.Settings.Default.RecoverIdAdmin;
                ListUsersRefresh();
            },
            (obj) => Properties.Settings.Default.AdminId != Properties.Settings.Default.RecoverIdAdmin);

        public ICommand ClearLogsBot => new DelegateCommand((obj) => ListLogs.Clear(), (obj) => ListLogs.Count > 0);

        public ICommand ConfirmAddNewUser
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    if (SelectedUserList == null)
                        SelectedUserList = new Core.PatternUser();
                    GlobalUnit.AuthUsers.Add(new Core.PatternUser
                    {
                        IdUser = SelectedUserList.IdUser,
                        IsBlocked = SelectedUserList.IsBlocked,
                        LocalNickname = SelectedUserList.LocalNickname
                    });
                    LogicRepository.RepUsers.Save();
                    ListUsersRefresh();
                });
            }
        }

        public ICommand ModifySelectedUser
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    if (SelectedUserList.IsUserAdmin)
                        SelectedUserList.IsBlocked = false;
                    //GlobalUnit.authUsers = ListUsers.ToList();
                    LogicRepository.RepUsers.Save();
                    ListUsersRefresh();
                });
            }
        }


        public ICommand DeleteCurrentUser
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    // if the selected or default (new) user not found and if selected user is admin, then return
                    if (GlobalUnit.AuthUsers.Count((f) => f.IdUser == SelectedUserList.IdUser) < 0) return;
                    GlobalUnit.AuthUsers.Remove(GlobalUnit.AuthUsers.Where((f) => f.IdUser == SelectedUserList.IdUser).FirstOrDefault());
                    LogicRepository.RepUsers.Save();
                    ListUsersRefresh();
                    SelectedUserList = new Core.PatternUser();
                },
                (obj) => !SelectedUserList.IsUserAdmin);
            }
        }

        public ICommand IsIsBlockedUserChanged
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    bool IsIsBlocked;
                    if (bool.TryParse(obj.ToString(), out IsIsBlocked))
                        SelectedUserList.IsBlocked = IsIsBlocked;
                },
                (obj) => !SelectedUserList.IsUserAdmin);
            }
        }

        public ICommand ChangeTheInterfaceForDarkTheme
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    Uri uri = new Uri($"pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Dark.xaml");
                    System.Windows.Application.Current.Resources.MergedDictionaries.RemoveAt(0);
                    System.Windows.Application.Current.Resources.MergedDictionaries.Insert(0, new System.Windows.ResourceDictionary() { Source = uri });
                });
            }
        }

        public ICommand ChangeTheInterfaceForLightTheme
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    Uri uri = new Uri($"pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Light.xaml");
                    System.Windows.Application.Current.Resources.MergedDictionaries.RemoveAt(0);
                    System.Windows.Application.Current.Resources.MergedDictionaries.Insert(0, new System.Windows.ResourceDictionary() { Source = uri });
                });
            }
        }
    }

}
