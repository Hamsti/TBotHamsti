using System.Windows;

namespace TBotHamsti
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            ViewModelLocator.Init();
            base.OnStartup(e);
        }

        /// <summary>
        /// Creating a bot and working with it
        /// </summary>
        public static Telegram.Bot.TelegramBotClient Api { get; } = new Telegram.Bot.TelegramBotClient(TBotHamsti.Properties.Settings.Default.ApiBot);
    }
   
}
