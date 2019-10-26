using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace HamstiBotWPF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SolidColorBrush brushNotBlocked = Brushes.White;
        private SolidColorBrush brushBlocked = Brushes.YellowGreen;

        public MainWindow()
        {
            InitializeComponent();
            subBotEvents();
            usersMenuInput();
            commandsMenuInput();         
            menuCommands.Items.Add(new MenuItem() { Header = "Чисто для примера", Foreground = Brushes.Green });
            menuCommands.Items.Add(new MenuItem() { Header = "Добавления элементов", Foreground = Brushes.DarkBlue });
        }

        /// <summary>
        /// Subscription to bot events
        /// </summary>
        private void subBotEvents()
        {
            GlobalUnit.Api.OnMessage += ExecuteLaunchBot.checkMessageBot;
            GlobalUnit.Api.OnMessageEdited += ExecuteLaunchBot.checkMessageBot;
            GlobalUnit.Api.OnMessageEdited += botOnMessageReceived;
            GlobalUnit.Api.OnMessage += botOnMessageReceived;
            GlobalUnit.Api.OnReceiveError += botOnReceiveError;
        }

        /// <summary>
        /// Stop the bot when closing the application
        /// </summary>
        private async void mainWindow_Closing_StopReceiving(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (GlobalUnit.Api.IsReceiving)
                    await ExecuteLaunchBot.stopBot();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Завершение приложения произошло с системной ошибкой: {ex.Message}");
            }
        }

        /// <summary>
        /// Adding all users in menu
        /// </summary>
        private void usersMenuInput(object sender = null, RoutedEventArgs e = null)
        {
            LogicRepository.RepUsers.loadFromJson();
            menuUsers.Items.Clear();
            for (int i = 0; i < GlobalUnit.authUsers.Count; i++)
            {
                if (GlobalUnit.authUsers[i].localNickname != null && GlobalUnit.authUsers[i].localNickname != "")
                {
                    if (GlobalUnit.authUsers[i].blocked)
                        menuUsers.Items.Add(new MenuItem() { Header = GlobalUnit.authUsers[i].localNickname + " | " + GlobalUnit.authUsers[i].idUser, Tag = GlobalUnit.authUsers[i].idUser, Foreground = brushBlocked });
                    else
                        menuUsers.Items.Add(new MenuItem() { Header = GlobalUnit.authUsers[i].localNickname + " | " + GlobalUnit.authUsers[i].idUser, Tag = GlobalUnit.authUsers[i].idUser, Foreground = brushNotBlocked });
                }
                else
                {
                    if (GlobalUnit.authUsers[i].blocked)
                        menuUsers.Items.Add(new MenuItem() { Header = "Не указано | " + GlobalUnit.authUsers[i].idUser, Tag = GlobalUnit.authUsers[i].idUser, Foreground = brushBlocked });
                    else
                        menuUsers.Items.Add(new MenuItem() { Header = "Не указано | " + GlobalUnit.authUsers[i].idUser, Tag = GlobalUnit.authUsers[i].idUser, Foreground = brushNotBlocked });
                }

                //Add in menuUsers sub menuDeleteItem
                MenuItem menuDeleteItem = new MenuItem() { Header = "Delete", Tag = i };
                menuDeleteItem.Click += (object sender1, RoutedEventArgs e1) =>
                {
                    GlobalUnit.authUsers.RemoveAt((int)menuDeleteItem.Tag);
                    LogicRepository.RepUsers.saveInJson();
                    usersMenuInput();
                };

                MenuItem menuBlockItem = new MenuItem() { Header = "Block", Tag = GlobalUnit.authUsers[i].idUser };
                menuBlockItem.Click += (object sender2, RoutedEventArgs e2) =>
                {
                    foreach (var user in GlobalUnit.authUsers.Where(w => (e2.Source as MenuItem).Tag != null && (int)(e2.Source as MenuItem).Tag == w.idUser))
                    {
                        user.blocked = user.blocked ? false : true;
                    }
                    LogicRepository.RepUsers.saveInJson();
                    usersMenuInput();
                };

                if ((int)((MenuItem)menuUsers.Items[i]).Tag != Properties.Settings.Default.AdminId)
                {
                    ((MenuItem)menuUsers.Items[i]).Items.Add(menuDeleteItem);
                    ((MenuItem)menuUsers.Items[i]).Items.Add(menuBlockItem);
                }
            }
        }


        /// <summary>
        /// Adding all commands in menu
        /// </summary>
        private void commandsMenuInput()
        {
            for (int i = 0; i < GlobalUnit.botCommands.Count; i++)
                menuCommands.Items.Add(new MenuItem() { Header = GlobalUnit.botCommands[i].ExampleCommand });
        }

        /// <summary>
        /// To start this bot onButtonClick
        /// </summary>
        private void startClick(object sender, RoutedEventArgs e)
        {
            try
            {
                logList.Items.Add("Start bot");
                Task.Run(()=> ExecuteLaunchBot.runBot());
            }
            catch (Exception ex)
            {
                logList.Items.Add($"При запуске бота произошла системная ошибка: {ex.Message}");
            }
        }

        /// <summary>
        /// To stop this bot onButtonClick
        /// </summary>
        private void stopClick(object sender, RoutedEventArgs e)
        {
            try
            { 
                logList.Items.Add("Stop bot");
                Task.Run(()=> ExecuteLaunchBot.stopBot());
            }
            catch (Exception ex)
            {
                logList.Items.Add($"При остановке бота произошла системная ошибка: {ex.Message}");
            }
        }

        /// <summary>
        /// Saving adminId in settings onButtonClick
        /// </summary>
        private void saveAdminClick(object sender, RoutedEventArgs e)
        {
            saveAdminIdSettings();
        }

        /// <summary>
        /// Restore default adminId onButtonClick
        /// </summary>
        private void defaultAdminClick(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.AdminId = 406777030;
            saveAdminIdSettings();
        }

        /// <summary>
        /// Saving adminId in settings
        /// </summary>
        private void saveAdminIdSettings()
        {
            Properties.Settings.Default.Save();
            GlobalUnit.authUsers[0].idUser = Properties.Settings.Default.AdminId;
            Task.Run(() => ExecuteLaunchBot.reloadBot());
            logList.Items.Add("Перезапуск бота...");
            usersMenuInput();
        }

        /// <summary>
        /// Cleaning the listbox of logging onButtonClick
        /// </summary>
        private void clearConsoleClick(object sender, RoutedEventArgs e)
        {
            logList.Items.Clear();
        }

        /// <summary>
        /// Adding to the log notification of an incoming message
        /// </summary>
        private void botOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            if (messageEventArgs.Message.Type == MessageType.Text)
                Dispatcher.Invoke(() => logList.Items.Add($"Пришло сообщение: {messageEventArgs.Message.Text}"));
            else if (messageEventArgs.Message.Type == MessageType.Photo)
                Dispatcher.Invoke(() => logList.Items.Add($"Пришло сообщение: {messageEventArgs.Message.Photo}"));
            else
                Dispatcher.Invoke(() => logList.Items.Add($"Пришло сообщение формата: {messageEventArgs.Message.Type}"));
        }

        private void botOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Dispatcher.Invoke(() => logList.Items.Add(string.Format("Произошла ошибка при прослушивании: {0} — {1}",
                receiveErrorEventArgs.ApiRequestException.ErrorCode,
                receiveErrorEventArgs.ApiRequestException.Message)));
        }

        private void ConfirmAddingNewUser(object sender, RoutedEventArgs e)
        {
            try
            {
                GlobalUnit.authUsers.Add(new Core.patternUserList { idUser = int.Parse(txtBoxIdUser.Text), blocked = (bool)radioBtnIdBlocked.IsChecked, localNickname = txtBoxNickname.Text });
                LogicRepository.RepUsers.saveInJson();
                txtBoxIdUser.Text = string.Empty;
                txtBoxNickname.Text = string.Empty;
                usersMenuInput();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }



        private static readonly Regex _regex = new Regex("[^0-9]+"); //regex that matches disallowed text
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        private void TxtBoxIdUser_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }
    }
}
