﻿using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace HamstiBotWPF
{
    public class MainViewModel : ViewModelBase
    {
        private int IdAdminForStartApp { get; set; } = Properties.Settings.Default.AdminId;
        public List<System.Windows.Controls.MenuItem> ListCommands { get; private set; }
        public ObservableCollection<string> ListLogs { get; private set; }
        public ObservableCollection<Core.patternUserList> ListUsers { get; private set; }

        private Core.patternUserList _SelectedUserList = new Core.patternUserList();
        public Core.patternUserList SelectedUserList
        {
            get { return _SelectedUserList == null ? new Core.patternUserList() : _SelectedUserList; }
            set
            {
                _SelectedUserList = value;
                RaisePropertiesChanged();
            }
        }


        public MainViewModel()
        {
            subBotEvents();
            ListCommands = new List<System.Windows.Controls.MenuItem>();
            GlobalUnit.botCommands.ForEach(botCom => ListCommands.Add(new System.Windows.Controls.MenuItem()
            {
                Header = botCom.ExampleCommand,
                Foreground = botCom.VisibleCommand ? System.Windows.Media.Brushes.White : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(97, 216, 162))
            }));

            ListLogs = new ObservableCollection<string>();

            ListUsers = new ObservableCollection<Core.patternUserList>();
            ListUsersRefresh();
        }

        private void subBotEvents()
        {
            GlobalUnit.myBot.Api.OnMessage += ExecuteLaunchBot.checkMessageBot;
            GlobalUnit.myBot.Api.OnMessageEdited += ExecuteLaunchBot.checkMessageBot;
            GlobalUnit.myBot.Api.OnMessageEdited += botOnMessageReceived;
            GlobalUnit.myBot.Api.OnMessage += botOnMessageReceived;
            GlobalUnit.myBot.Api.OnReceiveError += botOnReceiveError;
            //GlobalUnit.myBot.Api.OnUpdate += (obj, e) => App.Current.Dispatcher.Invoke(() => ListLogs.Add("OnUpdate"));
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

        public void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        public async void WindowClosing_StopReceivingBot(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (GlobalUnit.myBot.Api.IsReceiving)
                {
                    if (System.Windows.MessageBox.Show("The bot is still running, are you sure you want to shut down the bot and close the application?",
                        "HamstiBot", System.Windows.MessageBoxButton.OKCancel, System.Windows.MessageBoxImage.Warning) == System.Windows.MessageBoxResult.OK)
                    {
                        await ExecuteLaunchBot.stopBot();
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
            GlobalUnit.authUsers.OrderBy(ord => ord.locked).ToList().ForEach(user => ListUsers.Add(new Core.patternUserList()
            {
                idUser = user.idUser,
                localNickname = user.localNickname,
                locked = user.locked
            }));
            if (ListUsers.Count > 0 && Properties.Settings.Default.AdminId > 0)
                ListUsers.Move(ListUsers.IndexOf(ListUsers.SingleOrDefault(s => s.idUser == Properties.Settings.Default.AdminId)), 0);
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
                        Task.Run(() => ExecuteLaunchBot.runBot());
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
                        Task.Run(() => ExecuteLaunchBot.stopBot());
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
                Core.patternUserList newAdminUser = GlobalUnit.authUsers.FirstOrDefault(f => f.idUser == Properties.Settings.Default.AdminId);
                ListLogs.Add($"New bot administrator: {(newAdminUser != null ? newAdminUser.idUser_Nickname : "Unauthorized user")}"); ;
                IdAdminForStartApp = Properties.Settings.Default.AdminId;
                ListUsersRefresh();
            },
            (obj) => IdAdminForStartApp != Properties.Settings.Default.AdminId);

        public ICommand DefaultSettingBot => new DelegateCommand(
            (obj) =>
            {
                Properties.Settings.Default.AdminId = Properties.Settings.Default.RecoverIdAdmin;
                Properties.Settings.Default.Save();
                Core.patternUserList newAdminUser = GlobalUnit.authUsers.FirstOrDefault(f => f.idUser == Properties.Settings.Default.AdminId);
                ListLogs.Add($"Restored default bot admin: {(newAdminUser != null ? newAdminUser.idUser_Nickname : "Unauthorized user")}"); ;
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
                        SelectedUserList = new Core.patternUserList();
                    GlobalUnit.authUsers.Add(new Core.patternUserList
                    {
                        idUser = SelectedUserList.idUser,
                        locked = SelectedUserList.locked,
                        localNickname = SelectedUserList.localNickname
                    });
                    LogicRepository.RepUsers.saveInJson();
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
                        SelectedUserList.locked = false;
                    GlobalUnit.authUsers = ListUsers.ToList();
                    LogicRepository.RepUsers.saveInJson();
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
                    if (GlobalUnit.authUsers.FindIndex((f) => f.idUser == SelectedUserList.idUser) < 0) return;
                    GlobalUnit.authUsers.RemoveAt(GlobalUnit.authUsers.FindIndex((f) => f.idUser == SelectedUserList.idUser));
                    LogicRepository.RepUsers.saveInJson();
                    ListUsersRefresh();
                    SelectedUserList = new Core.patternUserList();
                },
                (obj) => !SelectedUserList.IsUserAdmin);
            }
        }

        public ICommand IsLockedUserChanged
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    bool IsLocked;
                    if (bool.TryParse(obj.ToString(), out IsLocked))
                        SelectedUserList.locked = IsLocked;
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
