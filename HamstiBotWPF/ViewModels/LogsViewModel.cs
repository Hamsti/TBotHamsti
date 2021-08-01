using DevExpress.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TBotHamsti.Models;
using TBotHamsti.Models.Messages;
using TBotHamsti.Models.Users;
using TBotHamsti.Services;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace TBotHamsti.ViewModels
{
    public class LogsViewModel : BindableBase
    {
        private static int MillisecondsDelayOnReceiveError => (int)(Properties.Settings.Default.SecondsDelayOnReceiveError * 1000);
        public static MessageBus MessageBus { get; private set; }
        public ObservableCollection<TextMessage> ListLogs { get; private set; }

        public LogsViewModel(MessageBus messageBus)
        {
            ListLogs = new ObservableCollection<TextMessage>();
            MessageBus = messageBus;
            MessageBus.Receive<TextMessage>(this, AddLog_Text);

            App.Api.OnMessage += AddLog_BotOnMessageRecived_EditedAlso;
            App.Api.OnMessage += AnalysisMessage_BotOnMessageRecived_EditedAlso;
            App.Api.OnMessageEdited += AddLog_BotOnMessageRecived_EditedAlso;
            App.Api.OnMessageEdited += AnalysisMessage_BotOnMessageRecived_EditedAlso;
            App.Api.OnReceiveError += BotOnReceiveError;
            App.Api.OnReceiveGeneralError += Api_OnReceiveGeneralError;
        }

        public ICommand ClearLogsBot => new DelegateCommand(() =>
            {
                ListLogs.Clear();
                GC.Collect();
            }, () => ListLogs.Count > 0);

        /// <summary>
        /// Adapter for processing message (new and edited)
        /// </summary>
        private async void AnalysisMessage_BotOnMessageRecived_EditedAlso(object sender, MessageEventArgs e)
        {
            User user = null;
            try
            {
                await ExecutionBot.CheckMessage(ref user, e.Message);
            }
            catch (Exception ex)
            {
                string exMessage = HandlerException.GetExceptionMessage(ex);
                await AddLog_Text(new TextMessage(exMessage, HorizontalAlignment.Right));
                await (user?.SendMessageAsync(exMessage)
                    ?? AddLog_Text(new TextMessage("User is null, message did't send: \"" + exMessage + "\"", HorizontalAlignment.Right)));
            }
        }

        private Task AddLog_Text(IMessage message)
        {
            App.UiContext.Post(_ => ListLogs.Insert(0, (TextMessage)message), null);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Adding to the log notification of an incoming message
        /// </summary>
        private void AddLog_BotOnMessageRecived_EditedAlso(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            AddLog_Text(new TextMessage($"[{message.From.Id}]: " + (message.Type switch
            {
                MessageType.Text => $"{message.Text}",
                MessageType.Photo => $"The image is downloaded: {message.Photo}",
                MessageType.Document => $"The document is downloaded: {message.Document}",
                _ => $"Received a message of the format: {message.Type}",
            }))).Wait();
        }

        private void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            AddLog_Text(new TextMessage(string.Format("An error occured while listening: {0} — {1}",
                receiveErrorEventArgs.ApiRequestException.ErrorCode,
                receiveErrorEventArgs.ApiRequestException.Message),
                HorizontalAlignment.Right)).Wait();
            Task.Delay(MillisecondsDelayOnReceiveError).Wait();
        }

        private void Api_OnReceiveGeneralError(object sender, ReceiveGeneralErrorEventArgs e)
        {
            AddLog_Text(new TextMessage("An error occurred on the event OnReceiveGeneralError: " + e.Exception.Message,
                HorizontalAlignment.Right)).Wait();
            Task.Delay(MillisecondsDelayOnReceiveError).Wait();
        }
    }
}
