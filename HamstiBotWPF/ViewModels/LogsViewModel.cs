using DevExpress.Mvvm;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
using TBotHamsti.Services;
using TBotHamsti.Messages;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using System.Threading.Tasks;
using System;
using TBotHamsti.Core;
using TBotHamsti.LogicRepository;

namespace TBotHamsti.ViewModels
{
    public class LogsViewModel : BindableBase
    {
        public static MessageBus MessageBus { get; private set; }
        public ObservableCollection<TextMessage> ListLogs { get; private set; }

        public LogsViewModel(MessageBus messageBus)
        {
            ListLogs = new ObservableCollection<TextMessage>();
            MessageBus = messageBus;
            MessageBus.Receive<TextMessage>(this, message => AddLog(new TextMessage(message.Text, message.HorizontalAlignment)));

            App.Api.OnMessageEdited += BotOnMessageReceived;
            App.Api.OnMessage += BotOnMessageReceived;
            App.Api.OnReceiveError += BotOnReceiveError;
            App.Api.OnReceiveGeneralError += Api_OnReceiveGeneralError;
            App.Api.OnMessage += Api_AdapterOnMessage_EditedAlso;
            App.Api.OnMessageEdited += Api_AdapterOnMessage_EditedAlso;
        }

        public ICommand ClearLogsBot => new DelegateCommand(() => ListLogs.Clear(), () => ListLogs.Count > 0);

        /// <summary>
        /// Adapter for processing message (new and edited)
        /// </summary>
        private async void Api_AdapterOnMessage_EditedAlso(object sender, MessageEventArgs e)
        {
            PatternUser user = RepUsers.GetUser(e.Message.From.Id);
            try
            {
                await ExecuteLaunchBot.CheckMessageBot(e.Message, user);
            }
            catch (Exception ex)
            {
                await AddLog(new TextMessage(ex.Message, HorizontalAlignment.Right));
                await (user?.SendMessageAsync(ex.Message) ??
                    AddLog(new TextMessage("User is null, message did't send: \"" + e.Message.Text + "\"", HorizontalAlignment.Right)));
            }
        }

        private Task AddLog(TextMessage message) => Application.Current.Dispatcher.InvokeAsync(() => ListLogs.Insert(0, message)).Task;

        /// <summary>
        /// Adding to the log notification of an incoming message
        /// </summary>
        private void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            AddLog(new TextMessage(message.Type switch
            {
                MessageType.Text => $"[{message.From.Id}]: {message.Text}",
                MessageType.Photo => $"Получено изображение: {message.Photo}",
                MessageType.Document => $"Получен документ: {message.Document}",
                MessageType.Audio => throw new NotImplementedException(),
                MessageType.Video => throw new NotImplementedException(),
                MessageType.Voice => throw new NotImplementedException(),
                MessageType.Sticker => throw new NotImplementedException(),
                MessageType.Location => throw new NotImplementedException(),
                MessageType.Contact => throw new NotImplementedException(),
                MessageType.Venue => throw new NotImplementedException(),
                MessageType.Game => throw new NotImplementedException(),
                MessageType.VideoNote => throw new NotImplementedException(),
                MessageType.Invoice => throw new NotImplementedException(),
                MessageType.SuccessfulPayment => throw new NotImplementedException(),
                MessageType.WebsiteConnected => throw new NotImplementedException(),
                MessageType.ChatMembersAdded => throw new NotImplementedException(),
                MessageType.ChatMemberLeft => throw new NotImplementedException(),
                MessageType.ChatTitleChanged => throw new NotImplementedException(),
                MessageType.ChatPhotoChanged => throw new NotImplementedException(),
                MessageType.MessagePinned => throw new NotImplementedException(),
                MessageType.ChatPhotoDeleted => throw new NotImplementedException(),
                MessageType.GroupCreated => throw new NotImplementedException(),
                MessageType.SupergroupCreated => throw new NotImplementedException(),
                MessageType.ChannelCreated => throw new NotImplementedException(),
                MessageType.MigratedToSupergroup => throw new NotImplementedException(),
                MessageType.MigratedFromGroup => throw new NotImplementedException(),
                MessageType.Poll => throw new NotImplementedException(),
                MessageType.Dice => throw new NotImplementedException(),
                MessageType.Unknown => throw new NotImplementedException(),
                _ => $"Пришло сообщение формата: {message.Type}",
            })).Wait();
        }

        private void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            AddLog(new TextMessage(string.Format("Произошла ошибка при прослушивании: {0} — {1}",
                receiveErrorEventArgs.ApiRequestException.ErrorCode,
                receiveErrorEventArgs.ApiRequestException.Message),
                HorizontalAlignment.Right)).Wait();
        }

        private void Api_OnReceiveGeneralError(object sender, ReceiveGeneralErrorEventArgs e)
        {
            AddLog(new TextMessage("Произошла ошибка при событии OnReceiveGeneralError: " + e.Exception.Message,
                HorizontalAlignment.Right)).Wait();
        }
    }
}
