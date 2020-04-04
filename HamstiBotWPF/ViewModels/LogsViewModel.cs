﻿using DevExpress.Mvvm;
using System.Collections.ObjectModel;
using System.Windows;
using HamstiBotWPF.Services;
using HamstiBotWPF.Messages;
using System.Windows.Input;

namespace HamstiBotWPF.ViewModels
{
    public class LogsViewModel : BindableBase
    {
        private readonly MessageBus messageBus;
        public ObservableCollection<TextMessage> ListLogs { get; private set; }

        public LogsViewModel(MessageBus messageBus)
        {
            this.messageBus = messageBus;
            ListLogs = new ObservableCollection<TextMessage>();

            this.messageBus.Receive<TextMessage>(this, async message => ListLogs.Insert(0, new TextMessage(message.Text, message.HorizontalAlignment)));

            GlobalUnit.Api.OnMessageEdited += BotOnMessageReceived;
            GlobalUnit.Api.OnMessage += BotOnMessageReceived;
            GlobalUnit.Api.OnReceiveError += BotOnReceiveError;
            GlobalUnit.Api.OnReceiveGeneralError += Api_OnReceiveGeneralError;
            GlobalUnit.Api.OnMessage += ExecuteLaunchBot.CheckMessageBot;
            GlobalUnit.Api.OnMessageEdited += ExecuteLaunchBot.CheckMessageBot;
        }

        public ICommand ClearLogsBot => new DelegateCommand((obj) => ListLogs.Clear(), (obj) => ListLogs.Count > 0);

        /// <summary>
        /// Adding to the log notification of an incoming message
        /// </summary>
        private void BotOnMessageReceived(object sender, Telegram.Bot.Args.MessageEventArgs messageEventArgs)
        {
            switch (messageEventArgs.Message.Type)
            {
                case Telegram.Bot.Types.Enums.MessageType.Text:
                    Application.Current.Dispatcher.Invoke(() => ListLogs.Insert(0, new TextMessage($"Получено сообщение: {messageEventArgs.Message.Text}"))); break;
                case Telegram.Bot.Types.Enums.MessageType.Photo:
                    Application.Current.Dispatcher.Invoke(() => ListLogs.Insert(0, new TextMessage($"Получено изображение: {messageEventArgs.Message.Photo}"))); break;
                case Telegram.Bot.Types.Enums.MessageType.Document:
                    Application.Current.Dispatcher.Invoke(() => ListLogs.Insert(0, new TextMessage($"Получен документ: {messageEventArgs.Message.Document}"))); break;
                default:
                    Application.Current.Dispatcher.Invoke(() => ListLogs.Insert(0, new TextMessage($"Пришло сообщение формата: {messageEventArgs.Message.Type}"))); break;
            }
        }

        private void BotOnReceiveError(object sender, Telegram.Bot.Args.ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Application.Current.Dispatcher.Invoke(() => ListLogs.Insert(0, new TextMessage(string.Format("Произошла ошибка при прослушивании: {0} — {1}",
                receiveErrorEventArgs.ApiRequestException.ErrorCode,
                receiveErrorEventArgs.ApiRequestException.Message), HorizontalAlignment.Right)));
        }

        private void Api_OnReceiveGeneralError(object sender, Telegram.Bot.Args.ReceiveGeneralErrorEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => ListLogs.Insert(0, new TextMessage("Произошла ошибка при событии OnReceiveGeneralError:\n\n" + e.Exception.Message, HorizontalAlignment.Right)));
        }
    }
}
