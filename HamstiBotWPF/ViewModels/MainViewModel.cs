using DevExpress.Mvvm;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Threading.Tasks;
using TBotHamsti.Services;
using TBotHamsti.Views;
using TBotHamsti.Models.Users;
using TBotHamsti.Models.Messages;
using TBotHamsti.Models;

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
            try
            {
                if (App.Api.IsReceiving)
                {
                    await messageBus.SendTo<LogsViewModel>(new TextMessage("The bot will be stopping. Try again for closing app.", HorizontalAlignment.Center));
                    StopBot.Execute(null);
                    e.Cancel = true;
                }
                else
                {
                    Application.Current.Shutdown(0);
                }
            }
            catch (Exception ex)
            {
                await messageBus.SendTo<LogsViewModel>(new TextMessage($"Завершение приложения произошло с системной ошибкой: {ex.Message}", HorizontalAlignment.Right));
            }
        }

        public ICommand StartBot => new AsyncCommand(async () =>
        {
            try
            {
                await messageBus.SendTo<LogsViewModel>(new TextMessage("Start bot", HorizontalAlignment.Center));
                UsersFunc.Upload();
                await Task.Run(() => ExecutionBot.RunBotAsync());
                if (App.Api.IsReceiving)
                    await messageBus.SendTo<LogsViewModel>(new TextMessage("Bot launched successfully", HorizontalAlignment.Right));
            }
            catch (Exception ex)
            {
                await messageBus.SendTo<LogsViewModel>(new TextMessage($"При запуске бота произошла ошибка: {ex.Message}", HorizontalAlignment.Right));
            }
        }, () => !App.Api.IsReceiving);

        public ICommand StopBot => new AsyncCommand(async () =>
        {
            try
            {
                await messageBus.SendTo<LogsViewModel>(new TextMessage("Stop bot", HorizontalAlignment.Center));
                UsersFunc.SaveRefresh();
                await Task.Run(() => ExecutionBot.StopBotAsync());
                if (!App.Api.IsReceiving)
                    await messageBus.SendTo<LogsViewModel>(new TextMessage("Bot successfully stopped", HorizontalAlignment.Right));
            }
            catch (Exception ex)
            {
                await messageBus.SendTo<LogsViewModel>(new TextMessage($"При остановке бота произошла ошибка: {ex.Message}", HorizontalAlignment.Right));
            }
        }, () => App.Api.IsReceiving);

        public ICommand RestartBot => new AsyncCommand(async () =>
        {
            try
            {
                await messageBus.SendTo<LogsViewModel>(new TextMessage("Restart bot", HorizontalAlignment.Center));
                await Task.Run(() => ExecutionBot.RestartBotAsync());
                if (App.Api.IsReceiving)
                    await messageBus.SendTo<LogsViewModel>(new TextMessage("Bot successfully restarted", HorizontalAlignment.Right));
            }
            catch (Exception ex)
            {
                await messageBus.SendTo<LogsViewModel>(new TextMessage("При попытке перезапуска бота произошла ошибка:" + ex.Message, HorizontalAlignment.Right));
            }
        }, () => App.Api.IsReceiving);
    }
}
