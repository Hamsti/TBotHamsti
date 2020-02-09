using System.Windows;

namespace HamstiBotWPF
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        App()
        {
            LogicRepository.RepCommands.AddAllCommands();
            LogicRepository.RepUsers.AddAllUsers();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            ViewModelLocator.Init();

            base.OnStartup(e);
        }
    }
   
}
