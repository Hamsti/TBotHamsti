using DevExpress.Mvvm;
using System.Linq;
using HamstiBotWPF.Services;
using System.Collections.ObjectModel;
using HamstiBotWPF.Core;
using HamstiBotWPF.Events;
using HamstiBotWPF.Pages;
using System.Windows.Input;

namespace HamstiBotWPF.ViewModels
{
    public class UsersControlViewModel : BindableBase
    {
        private readonly PageService pageService;
        private readonly EventBus eventBus;
        public static ObservableCollection<PatternUser> ListUsers { get; private set; }

        public UsersControlViewModel(PageService pageService, EventBus eventBus)
        {
            this.pageService = pageService;
            this.eventBus = eventBus;

            ListUsers = new ObservableCollection<PatternUser>();
            ListUsersRefresh();

            this.eventBus.Subscribe<RefreshUsersListEvent>(async _ => ListUsersRefresh());
        }

        private void ListUsersRefresh()
        {
            ListUsers.Clear();
            GlobalUnit.authUsers.OrderBy(ord => ord.IsBlocked).ToList().ForEach(user => ListUsers.Add(new PatternUser()
            {
                IdUser = user.IdUser,
                LocalNickname = user.LocalNickname,
                IsBlocked = user.IsBlocked
            }));
            if (ListUsers.Count > 0 && Properties.Settings.Default.AdminId > 0)
                ListUsers.Move(ListUsers.IndexOf(ListUsers.SingleOrDefault(s => s.IdUser == Properties.Settings.Default.AdminId)), 0);
            Properties.Settings.Default.Reload();
        }

        public ICommand CreateUserPageChange => new AsyncCommand(async () =>
        {
            await eventBus.Publish(new ResetSelectedUserEvent());
            pageService.ChangePage(new ChangeUserDataPage());
        });

        public ICommand ModifyUserPageChange => new AsyncCommand(async () =>
        {
            pageService.ChangePage(new ChangeUserDataPage());
            await eventBus.Publish(new ModifySelectedUser());

        });  
    }
}
