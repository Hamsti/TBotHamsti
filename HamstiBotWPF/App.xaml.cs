using System.Windows;

namespace HamstiBotWPF
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            ViewModelLocator.Init();
            LogicRepository.RepCommands.Refresh();
            LogicRepository.RepUsers.Upload();
            base.OnStartup(e);
        }

        /// <summary>
        /// Creating a bot and working with it
        /// </summary>
        public static Telegram.Bot.TelegramBotClient Api { get; } = new Telegram.Bot.TelegramBotClient(HamstiBotWPF.Properties.Settings.Default.ApiBot);
    }
   
}
