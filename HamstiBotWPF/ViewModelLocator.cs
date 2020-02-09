using Microsoft.Extensions.DependencyInjection;
using HamstiBotWPF.Services;
using HamstiBotWPF.ViewModels;

namespace HamstiBotWPF
{
    public class ViewModelLocator
    {
        private static ServiceProvider _provider;

        public static void Init()
        {
            var services = new ServiceCollection();

            services.AddTransient<CommandsControlViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<ViewModels.MainViewModel>();
            services.AddScoped<ChangeUserDataPageViewModel>();
            services.AddScoped<UsersControlViewModel>();
            services.AddScoped<LogsViewModel>();

            services.AddSingleton<PageService>();
            services.AddSingleton<EventBus>();
            services.AddSingleton<MessageBus>();

            _provider = services.BuildServiceProvider();

            foreach (var item in services)
            {
                _provider.GetRequiredService(item.ServiceType);
            }
        }

        public ViewModels.MainViewModel MainViewModel => _provider.GetRequiredService<ViewModels.MainViewModel>();
        public CommandsControlViewModel CommandsControlViewModel => _provider.GetRequiredService<CommandsControlViewModel>();
        public LogsViewModel LogsViewModel => _provider.GetRequiredService<LogsViewModel>();
        public SettingsViewModel SettingsViewModel => _provider.GetRequiredService<SettingsViewModel>();
        public UsersControlViewModel UsersControlViewModel => _provider.GetRequiredService<UsersControlViewModel>();
        public ChangeUserDataPageViewModel ChangeUserDataPageViewModel => _provider.GetRequiredService<ChangeUserDataPageViewModel>();
    }
}
