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
    public class NewMainViewModel : ViewModelBase
    {
        static int idAdminForStartApp;
        static Core.PatternUserList selectedUserList;
        public static List<System.Windows.Controls.MenuItem> ListCommands { get; }
        public static ObservableCollection<string> ListLogs { get; }
        public static ObservableCollection<Core.PatternUserList> ListUsers { get; }
        public Core.PatternUserList SelectedUserList
        {
            get { return selectedUserList == null ? new Core.PatternUserList() : selectedUserList; }
            set
            {
                selectedUserList = value;
                RaisePropertiesChanged();
            }
        }

        static NewMainViewModel()
        {
            idAdminForStartApp = Properties.Settings.Default.AdminId;
            ListCommands = new List<System.Windows.Controls.MenuItem>();
            ListLogs = new ObservableCollection<string>();
            ListUsers = new ObservableCollection<Core.PatternUserList>();
            selectedUserList = new Core.PatternUserList();
        }


        public NewMainViewModel()
        {
            subBotEvents();
            GlobalUnit.botCommands.ForEach(botCom => ListCommands.Add(new System.Windows.Controls.MenuItem()
            {
                Header = botCom.ExampleCommand,
                Foreground = System.Windows.Media.Brushes.White
                //Foreground = botCom.VisibleForUsers ? System.Windows.Media.Brushes.White : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(97, 216, 162))
            }));
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
                    App.Current.Dispatcher.Invoke(() => ListLogs.Add($"Получено сообщение: {messageEventArgs.Message.Text}")); break;
                case Telegram.Bot.Types.Enums.MessageType.Photo:
                    App.Current.Dispatcher.Invoke(() => ListLogs.Add($"Получено изображение: {messageEventArgs.Message.Photo}")); break;
                default:
                    App.Current.Dispatcher.Invoke(() => ListLogs.Add($"Получено сообщение формата: {messageEventArgs.Message.Type}")); break;
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
            App.Current.Dispatcher.Invoke(() => ListLogs.Add("Произошла ошибка при обработке события OnReceiveGeneralError:\n\n" + e.Exception.Message));
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
            GlobalUnit.authUsers.OrderBy(ord => ord.IsBlocked).ToList().ForEach(user => ListUsers.Add(new Core.PatternUserList()
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
                Core.PatternUserList newAdminUser = GlobalUnit.authUsers.FirstOrDefault(f => f.IdUser == Properties.Settings.Default.AdminId);
                ListLogs.Add($"New bot administrator: {(newAdminUser != null ? newAdminUser.IdUser_Nickname : "Unauthorized user")}"); ;
                idAdminForStartApp = Properties.Settings.Default.AdminId;
                ListUsersRefresh();
            },
            (obj) => idAdminForStartApp != Properties.Settings.Default.AdminId);

        public ICommand DefaultSettingBot => new DelegateCommand(
            (obj) =>
            {
                Properties.Settings.Default.AdminId = Properties.Settings.Default.RecoverIdAdmin;
                Properties.Settings.Default.Save();
                Core.PatternUserList newAdminUser = GlobalUnit.authUsers.FirstOrDefault(f => f.IdUser == Properties.Settings.Default.AdminId);
                ListLogs.Add($"Restored default bot admin: {(newAdminUser != null ? newAdminUser.IdUser_Nickname : "Unauthorized user")}"); ;
                idAdminForStartApp = Properties.Settings.Default.RecoverIdAdmin;
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
                        SelectedUserList = new Core.PatternUserList();
                    GlobalUnit.authUsers.Add(new Core.PatternUserList
                    {
                        IdUser = SelectedUserList.IdUser,
                        IsBlocked = SelectedUserList.IsBlocked,
                        LocalNickname = SelectedUserList.LocalNickname
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
                        SelectedUserList.IsBlocked = false;
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
                    if (GlobalUnit.authUsers.FindIndex((f) => f.IdUser == SelectedUserList.IdUser) < 0) return;
                    GlobalUnit.authUsers.RemoveAt(GlobalUnit.authUsers.FindIndex((f) => f.IdUser == SelectedUserList.IdUser));
                    LogicRepository.RepUsers.saveInJson();
                    ListUsersRefresh();
                    SelectedUserList = new Core.PatternUserList();
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