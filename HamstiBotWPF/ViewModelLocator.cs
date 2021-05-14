using Microsoft.Extensions.DependencyInjection;
using TBotHamsti.Services;
using TBotHamsti.ViewModels;

namespace TBotHamsti
{
    public class ViewModelLocator
    {
        private static ServiceProvider _provider;

        public static void Init()
        {
            var services = new ServiceCollection();

            services.AddTransient<MainViewModel>();
            services.AddTransient<CommandsControlViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddScoped<LogsViewModel>();
            services.AddScoped<CommonControlUserDataViewModel>();

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
