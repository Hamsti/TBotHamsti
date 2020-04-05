using DevExpress.Mvvm;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
using HamstiBotWPF.Services;
using HamstiBotWPF.Messages;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace HamstiBotWPF.ViewModels
{
    public class LogsViewModel : BindableBase
    {
        private readonly MessageBus messageBus;
        public ObservableCollection<TextMessage> ListLogs { get; private set; }

        public LogsViewModel(MessageBus messageBus)
        {
            ListLogs = new ObservableCollection<TextMessage>();
            this.messageBus = messageBus;
            this.messageBus.Receive<TextMessage>(this, async message => ListLogs.Insert(0, new TextMessage(message.Text, message.HorizontalAlignment)));

            App.Api.OnMessageEdited += BotOnMessageReceived;
            App.Api.OnMessage += BotOnMessageReceived;
            App.Api.OnReceiveError += BotOnReceiveError;
            App.Api.OnReceiveGeneralError += Api_OnReceiveGeneralError;
            App.Api.OnMessage += ExecuteLaunchBot.CheckMessageBot;
            App.Api.OnMessageEdited += ExecuteLaunchBot.CheckMessageBot;
        }

        public ICommand ClearLogsBot => new DelegateCommand(() => ListLogs.Clear(), () => ListLogs.Count > 0);

        /// <summary>
        /// Adding to the log notification of an incoming message
        /// </summary>
        private void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            switch (messageEventArgs.Message.Type)
            {
                case MessageType.Text:
                    Application.Current.Dispatcher.Invoke(() => ListLogs.Insert(0, new TextMessage($"Получено сообщение: {messageEventArgs.Message.Text}"))); break;
                case MessageType.Photo:
                    Application.Current.Dispatcher.Invoke(() => ListLogs.Insert(0, new TextMessage($"Получено изображение: {messageEventArgs.Message.Photo}"))); break;
                case MessageType.Document:
                    Application.Current.Dispatcher.Invoke(() => ListLogs.Insert(0, new TextMessage($"Получен документ: {messageEventArgs.Message.Document}"))); break;
                default:
                    Application.Current.Dispatcher.Invoke(() => ListLogs.Insert(0, new TextMessage($"Пришло сообщение формата: {messageEventArgs.Message.Type}"))); break;
            }
        }

        private void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Application.Current.Dispatcher.Invoke(() => ListLogs.Insert(0, new TextMessage(string.Format("Произошла ошибка при прослушивании: {0} — {1}",
                receiveErrorEventArgs.ApiRequestException.ErrorCode,
                receiveErrorEventArgs.ApiRequestException.Message), HorizontalAlignment.Right)));
        }

        private void Api_OnReceiveGeneralError(object sender, ReceiveGeneralErrorEventArgs e) =>
            Application.Current.Dispatcher.Invoke(() => ListLogs.Insert(0, new TextMessage("Произошла ошибка при событии OnReceiveGeneralError:\n\n" + e.Exception.Message, HorizontalAlignment.Right)));
    }
}
