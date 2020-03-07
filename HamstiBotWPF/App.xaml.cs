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
            LogicRepository.RepUsers.Refresh();
            base.OnStartup(e);
        }
    }
   
}
