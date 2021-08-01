using DevExpress.Mvvm;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TBotHamsti.Models;
using TBotHamsti.Models.Messages;
using TBotHamsti.Models.Users;
using TBotHamsti.Services;
using TBotHamsti.Views;

namespace TBotHamsti.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private readonly PageService pageService;
        private readonly MessageBus messageBus;

        public Page PageSource { get; private set; }
        public string PageSourceTitle => PageSource.Title;//.ToString().Split('.').Last();

        public MainViewModel(PageService pageService, MessageBus messageBus)
        {
            this.pageService = pageService;
            this.messageBus = messageBus;

            this.pageService.OnPageChanged += (page) => PageSource = page;
            this.pageService.ChangePage(new LogsPage());
            
            if (Properties.Settings.Default.IsEnabledAutoStartBot)
            {
                _ = messageBus.SendTo<LogsViewModel>(new TextMessage("warning: automatic launch of the bot can increase the launch time of the application", HorizontalAlignment.Center));
                _ = StartBotCommand();
            }
        }

        public ICommand LogsPageChange => new DelegateCommand(() =>
        {
            pageService.ChangePage(new LogsPage());
        }, () => PageSourceTitle != "LogsPage");

        public ICommand UserControlPageChange => new DelegateCommand(() =>
        {
            pageService.ChangePage(new UsersControlPage());
            UsersFunc.Refresh();
        }, () => PageSourceTitle != "UsersControlPage");

        public ICommand CommandsControlPageChange => new DelegateCommand(() =>
        {
            pageService.ChangePage(new CommandsControlPage());
        }, () => PageSourceTitle != "CommandsControlPage");

        public ICommand SettingsPageChange => new DelegateCommand(() =>
        {
            pageService.ChangePage(new SettingsPage());
        }, () => PageSourceTitle != "SettingsPage");

        public async void WindowClosing_StopReceivingBot(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (App.Api.IsReceiving && StopBot.CanExecute(null))
            {
                await messageBus.SendTo<LogsViewModel>(new TextMessage("The bot will be stopping. Try again for closing app.", HorizontalAlignment.Center));
                StopBot.Execute(null);
                e.Cancel = true;
            }
            else
            {
                UsersFunc.SaveRefresh();
                Environment.Exit(0);
            }
        }

        public ICommand StartBot => new AsyncCommand(StartBotCommand, () => !App.Api.IsReceiving);

        private async Task StartBotCommand()
        {
            try
            {
                await LogsViewModel.MessageBus.SendTo<LogsViewModel>(new TextMessage("Start bot", HorizontalAlignment.Center));
                await ExecutionBot.StartReceivingBotAsync();
            }
            catch (Exception ex)
            {
                await messageBus.SendTo<LogsViewModel>(new TextMessage($"An error occurred while starting the bot: " + HandlerException.GetExceptionMessage(ex), HorizontalAlignment.Right));
            }
        }

        public ICommand StopBot => new AsyncCommand(async () =>
        {
            try
            {
                await LogsViewModel.MessageBus.SendTo<LogsViewModel>(new TextMessage("Stop bot", HorizontalAlignment.Center));
                await ExecutionBot.StopBotAsync();
            }
            catch (Exception ex)
            {
                await messageBus.SendTo<LogsViewModel>(new TextMessage($"An error occurred while stopping the bot: " + HandlerException.GetExceptionMessage(ex), HorizontalAlignment.Right));
            }
        }, () => App.Api.IsReceiving);

        public ICommand RestartBot => new AsyncCommand(async () =>
        {
            try
            {
                await messageBus.SendTo<LogsViewModel>(new TextMessage("Restart bot", HorizontalAlignment.Center));
                await ExecutionBot.RestartBotAsync();
            }
            catch (Exception ex)
            {
                await messageBus.SendTo<LogsViewModel>(new TextMessage("An error occurred while trying to restart the bot: " + HandlerException.GetExceptionMessage(ex), HorizontalAlignment.Right));
            }
        }, () => App.Api.IsReceiving);
    }
}
