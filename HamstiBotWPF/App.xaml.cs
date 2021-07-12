using System.Windows;
using Telegram.Bot;
using System.Threading;

namespace TBotHamsti
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Creating a bot and working with it
        /// </summary>
        public static TelegramBotClient Api { get; } = new TelegramBotClient(TBotHamsti.Properties.Settings.Default.ApiBot);
        public static SynchronizationContext UiContext { get; private set; }
        
        protected override void OnStartup(StartupEventArgs e)
        {
            UiContext = SynchronizationContext.Current;
            ViewModelLocator.Init();
            LogicRepository.CollectionCommands.Init();
            base.OnStartup(e);
        }
    }
}
