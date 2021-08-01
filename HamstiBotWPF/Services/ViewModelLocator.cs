using Microsoft.Extensions.DependencyInjection;
using TBotHamsti.ViewModels;

namespace TBotHamsti.Services
{
    public class ViewModelLocator
    {
        private static ServiceProvider _provider;

        public static void Init()
        {
            var services = new ServiceCollection();

            services.AddSingleton<MainViewModel>();
            services.AddTransient<CommandsControlViewModel>();
            services.AddScoped<CommonControlUserDataViewModel>();
            services.AddScoped<SettingsViewModel>();
            services.AddScoped<LogsViewModel>();

            services.AddSingleton<PageService>();
            services.AddSingleton<MessageBus>();

            _provider = services.BuildServiceProvider();
            foreach (var item in services)
            {
                _provider.GetRequiredService(item.ServiceType);
            }
        }

        public MainViewModel MainViewModel => _provider.GetRequiredService<MainViewModel>();
        public CommandsControlViewModel CommandsControlViewModel => _provider.GetRequiredService<CommandsControlViewModel>();
        public SettingsViewModel SettingsViewModel => _provider.GetRequiredService<SettingsViewModel>();
        public LogsViewModel LogsViewModel => _provider.GetRequiredService<LogsViewModel>();
        public CommonControlUserDataViewModel CommonControlUserDataViewModel => _provider.GetRequiredService<CommonControlUserDataViewModel>();
    }
}
