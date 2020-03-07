using DevExpress.Mvvm;
using System;
using System.Linq;
using System.Threading.Tasks;
using HamstiBotWPF.Services;
using HamstiBotWPF.Pages;
using HamstiBotWPF.Events;
using System.Windows.Controls;
using System.Windows.Input;

namespace HamstiBotWPF.ViewModels
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

        public ICommand LogsPageChange => new DelegateCommand((obj) =>
        {
            pageService.ChangePage(new LogsPage());
        }, (obj) => PageSourceShortName != "LogsPage");
       
        public ICommand UserControlPageChange => new AsyncCommand(async () =>
        {
            await eventBus.Publish(new RefreshUsersListEvent());
            pageService.ChangePage(new UsersControlPage());
        }, () => PageSourceShortName != "UsersControlPage");

        public ICommand CommandsControlPageChange => new DelegateCommand((obj) =>
        {
            pageService.ChangePage(new CommandsControlPage());
        }, (obj) => PageSourceShortName != "CommandsControlPage");

        public ICommand SettingsPageChange => new DelegateCommand((obj) =>
        {
            pageService.ChangePage(new SettingsPage());
        }, (obj) => PageSourceShortName != "SettingsPage");       

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
                        System.Windows.Application.Current.Shutdown(0);
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

        public ICommand StartBot => new AsyncCommand(async () =>
        {
            try
            {
                await messageBus.SendTo<LogsViewModel>(new Messages.TextMessage("Start bot"));
                await Task.Run(() => ExecuteLaunchBot.RunBotAsync());
                if (GlobalUnit.Api.IsReceiving)
                    await messageBus.SendTo<LogsViewModel>(new Messages.TextMessage("Bot launched successfully"));
            }
            catch (Exception ex)
            {
                await messageBus.SendTo<LogsViewModel>(new Messages.TextMessage($"При запуске бота произошла ошибка: {ex.Message}"));
            }
        }, () => !GlobalUnit.Api.IsReceiving);

        public ICommand StopBot => new AsyncCommand(async () =>
        {
            try
            {
                await messageBus.SendTo<LogsViewModel>(new Messages.TextMessage("Stop bot"));
                await Task.Run(() => ExecuteLaunchBot.StopBotAsync());
                if (!GlobalUnit.Api.IsReceiving)
                    await messageBus.SendTo<LogsViewModel>(new Messages.TextMessage("Bot successfully stopped"));
            }
            catch (Exception ex)
            {
                await messageBus.SendTo<LogsViewModel>(new Messages.TextMessage($"При остановке бота произошла ошибка: {ex.Message}"));
            }
        }, () => GlobalUnit.Api.IsReceiving);

        public ICommand RestartBot => new AsyncCommand(async () =>
        {
            try
            {
                await messageBus.SendTo<LogsViewModel>(new Messages.TextMessage("Restart bot"));
                await Task.Run(() => ExecuteLaunchBot.RestartBotAsync());
                if (GlobalUnit.Api.IsReceiving)
                    await messageBus.SendTo<LogsViewModel>(new Messages.TextMessage("Bot successfully restarted"));
            }
            catch (Exception ex)
            {
                await messageBus.SendTo<LogsViewModel>(new Messages.TextMessage("При попытке перезапуска бота произошла ошибка:" + ex.Message));
            }
        }, () => GlobalUnit.Api.IsReceiving);
    }
}
