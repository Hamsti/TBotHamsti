using DevExpress.Mvvm;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
using TBotHamsti.Services;
using TBotHamsti.Messages;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace TBotHamsti.ViewModels
{
    public class LogsViewModel : BindableBase
    {
        private readonly MessageBus messageBus;
        public ObservableCollection<TextMessage> ListLogs { get; private set; }

        public LogsViewModel(MessageBus messageBus)
        {
            ListLogs = new ObservableCollection<TextMessage>();
            this.messageBus = messageBus;
            this.messageBus.Receive<TextMessage>(this, async message => ListLogs.Insert(0, new TextMessage(message.Text,
                                                                                                           message.HorizontalAlignment)));
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
            var message = messageEventArgs.Message;
            Application.Current.Dispatcher.Invoke(() => ListLogs.Insert(0, message.Type switch
            {
                MessageType.Text => new TextMessage($"[{message.From.Id}]: {message.Text}"),
                MessageType.Photo => new TextMessage($"Получено изображение: {message.Photo}"),
                MessageType.Document => new TextMessage($"Получен документ: {message.Document}"),
                MessageType.Audio => throw new System.NotImplementedException(),
                MessageType.Video => throw new System.NotImplementedException(),
                MessageType.Voice => throw new System.NotImplementedException(),
                MessageType.Sticker => throw new System.NotImplementedException(),
                MessageType.Location => throw new System.NotImplementedException(),
                MessageType.Contact => throw new System.NotImplementedException(),
                MessageType.Venue => throw new System.NotImplementedException(),
                MessageType.Game => throw new System.NotImplementedException(),
                MessageType.VideoNote => throw new System.NotImplementedException(),
                MessageType.Invoice => throw new System.NotImplementedException(),
                MessageType.SuccessfulPayment => throw new System.NotImplementedException(),
                MessageType.WebsiteConnected => throw new System.NotImplementedException(),
                MessageType.ChatMembersAdded => throw new System.NotImplementedException(),
                MessageType.ChatMemberLeft => throw new System.NotImplementedException(),
                MessageType.ChatTitleChanged => throw new System.NotImplementedException(),
                MessageType.ChatPhotoChanged => throw new System.NotImplementedException(),
                MessageType.MessagePinned => throw new System.NotImplementedException(),
                MessageType.ChatPhotoDeleted => throw new System.NotImplementedException(),
                MessageType.GroupCreated => throw new System.NotImplementedException(),
                MessageType.SupergroupCreated => throw new System.NotImplementedException(),
                MessageType.ChannelCreated => throw new System.NotImplementedException(),
                MessageType.MigratedToSupergroup => throw new System.NotImplementedException(),
                MessageType.MigratedFromGroup => throw new System.NotImplementedException(),
                MessageType.Poll => throw new System.NotImplementedException(),
                MessageType.Dice => throw new System.NotImplementedException(),
                MessageType.Unknown => throw new System.NotImplementedException(),
                _ => new TextMessage($"Пришло сообщение формата: {message.Type}"),
            }));
        }

        private void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Application.Current.Dispatcher.Invoke(() => ListLogs.Insert(0, new TextMessage(string.Format(
                "Произошла ошибка при прослушивании: {0} — {1}",
                receiveErrorEventArgs.ApiRequestException.ErrorCode,
                receiveErrorEventArgs.ApiRequestException.Message), HorizontalAlignment.Right)));
        }

        private void Api_OnReceiveGeneralError(object sender, ReceiveGeneralErrorEventArgs e) =>
            Application.Current.Dispatcher.Invoke(() => ListLogs.Insert(0, new TextMessage(
                "Произошла ошибка при событии OnReceiveGeneralError: " + e.Exception.Message,
                HorizontalAlignment.Right)));
    }
}
