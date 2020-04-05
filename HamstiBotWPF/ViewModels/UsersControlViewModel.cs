using DevExpress.Mvvm;
using System.Collections.ObjectModel;
using System.Windows.Input;
using HamstiBotWPF.Services;
using HamstiBotWPF.Core;
using HamstiBotWPF.Events;
using HamstiBotWPF.Pages;
using HamstiBotWPF.LogicRepository;

namespace HamstiBotWPF.ViewModels
{
    public class UsersControlViewModel : BindableBase
    {
        private readonly PageService pageService;
        private readonly EventBus eventBus;
        public static ObservableCollection<PatternUser> ListUsers => RepUsers.AuthUsers;

        public UsersControlViewModel(PageService pageService, EventBus eventBus)
        {
            this.pageService = pageService;
            this.eventBus = eventBus;
            
            RepUsers.RefreshAndSort();

            this.eventBus.Subscribe<RefreshUsersListEvent>(async _ => RepUsers.RefreshAndSort());
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
