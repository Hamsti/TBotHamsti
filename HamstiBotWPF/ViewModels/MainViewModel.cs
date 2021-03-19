using DevExpress.Mvvm;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Threading.Tasks;
using TBotHamsti.Services;
using TBotHamsti.Pages;
using TBotHamsti.Events;
using TBotHamsti.Messages;

namespace TBotHamsti.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private readonly PageService pageService;
        private readonly MessageBus messageBus;
        private readonly EventBus eventBus;

        public Page PageSource { get; set; }
        public string PageSourceShortName => PageSource.ToString().Split('.').Last();

        public MainViewModel(PageService pageService, MessageBus messageBus, EventBus eventBus)
        {
            this.pageService = pageService;
            this.messageBus = messageBus;
            this.eventBus = eventBus;

            this.pageService.OnPageChanged += (page) => PageSource = page;
            this.pageService.ChangePage(new LogsPage());
        }

        public ICommand LogsPageChange => new DelegateCommand(() =>
        {
            pageService.ChangePage(new LogsPage());
        }, () => PageSourceShortName != "LogsPage");
       
        public ICommand UserControlPageChange => new AsyncCommand(async () =>
        {
            await eventBus.Publish(new RefreshUsersListEvent());
            pageService.ChangePage(new UsersControlPage());
        }, () => PageSourceShortName != "UsersControlPage");

        public ICommand CommandsControlPageChange => new DelegateCommand(() =>
        {
            pageService.ChangePage(new CommandsControlPage());
        }, () => PageSourceShortName != "CommandsControlPage");

        public ICommand SettingsPageChange => new DelegateCommand(() =>
        {
            pageService.ChangePage(new SettingsPage());
        }, () => PageSourceShortName != "SettingsPage");       

        public async void WindowClosing_StopReceivingBot(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (App.Api.IsReceiving)
                {
                    if (MessageBox.Show("The bot is still running, are you sure you want to shut down the bot and close the application?", Application.Current.MainWindow.Title, MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
                    {
                        await ExecuteLaunchBot.StopBotAsync();
                        Application.Current.Shutdown(0);
                    }
                    else
                        e.Cancel = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Завершение приложения произошло с системной ошибкой: {ex.Message}");
            }
        }

        public ICommand StartBot => new AsyncCommand(async () =>
        {
            try
            {
                await messageBus.SendTo<LogsViewModel>(new TextMessage("Start bot", HorizontalAlignment.Center));
                await Task.Run(() => ExecuteLaunchBot.RunBotAsync());
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
                await Task.Run(() => ExecuteLaunchBot.StopBotAsync());
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
                await Task.Run(() => ExecuteLaunchBot.RestartBotAsync());
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
