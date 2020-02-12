using DevExpress.Mvvm;
using System.Linq;
using HamstiBotWPF.Services;
using HamstiBotWPF.Core;
using HamstiBotWPF.Pages;
using HamstiBotWPF.Events;
using System.Windows.Input;
using HamstiBotWPF.Messages;

namespace HamstiBotWPF.ViewModels
{
    public class ChangeUserDataPageViewModel : BindableBase
    {
        private readonly PageService pageService;
        private readonly MessageBus messageBus;
        private readonly EventBus eventBus;
        private readonly System.Text.RegularExpressions.Regex regex;
        private PatternUser selectedUserList;
        private PatternUser selectedUserBeforeModify;

        public bool ExitsSelectedUser => UsersControlViewModel.ListUsers.Count(c => c == SelectedUserList) > 0; 

        public PatternUser SelectedUserList
        {
            get { return selectedUserList ?? new PatternUser(); }
            set
            {
                selectedUserList = value;
                RaisePropertiesChanged();
            }
        }

        public ChangeUserDataPageViewModel(PageService pageService, MessageBus messageBus, EventBus eventBus)
        {
            this.pageService = pageService;
            this.messageBus = messageBus;
            this.eventBus = eventBus;
            this.eventBus.Subscribe<ResetSelectedUserEvent>(async _ => SelectedUserList = new PatternUser());
            this.eventBus.Subscribe<ModifySelectedUser>(async _ =>
            {
                selectedUserBeforeModify = new PatternUser()
                {
                    IdUser = SelectedUserList.IdUser,
                    LocalNickname = SelectedUserList.LocalNickname,
                    IsBlocked = SelectedUserList.IsBlocked
                };
            });
            regex = new System.Text.RegularExpressions.Regex("[^0-9]+");
            selectedUserList = new PatternUser();
        }

        public ICommand CreateNewUser => new AsyncCommand(async () =>
        {
            await messageBus.SendTo<LogsViewModel>(new TextMessage($"Added new user: {SelectedUserList.IdUser_Nickname}"));
            GlobalUnit.authUsers.Add(new PatternUser
            {
                IdUser = SelectedUserList.IdUser,
                IsBlocked = SelectedUserList.IsBlocked,
                LocalNickname = SelectedUserList.LocalNickname
            });
            LogicRepository.RepUsers.saveInJson();
            await eventBus.Publish(new RefreshUsersListEvent());
            pageService.ChangePage(new UsersControlPage());
        }, () => !ExitsSelectedUser);

        public ICommand ModifySelectedUser => new AsyncCommand(async () =>
        {
            await messageBus.SendTo<LogsViewModel>(new TextMessage($"Modify user: ({selectedUserBeforeModify.IdUser_Nickname} | {selectedUserBeforeModify.IsBlocked}) => ({SelectedUserList.IdUser_Nickname} | {SelectedUserList.IsBlocked})"));
            GlobalUnit.authUsers = UsersControlViewModel.ListUsers.ToList();
            LogicRepository.RepUsers.saveInJson();
            await eventBus.Publish(new RefreshUsersListEvent());
            pageService.ChangePage(new UsersControlPage());
        }, () => ExitsSelectedUser);

        public ICommand DeleteSelectedUser => new DelegateCommand(async (obj) =>
        {
            // if the selected or default (new) user not found and if selected user is admin, then return
            if (GlobalUnit.authUsers.FindIndex((f) => f.IdUser == SelectedUserList.IdUser) < 0) return;
            await messageBus.SendTo<LogsViewModel>(new TextMessage($"Deleted user: {SelectedUserList.IdUser_Nickname} | {SelectedUserList.IsBlocked}"));
            GlobalUnit.authUsers.RemoveAt(GlobalUnit.authUsers.FindIndex((f) => f.IdUser == SelectedUserList.IdUser));
            LogicRepository.RepUsers.saveInJson();
            await eventBus.Publish(new RefreshUsersListEvent());
            SelectedUserList = new PatternUser();
        }, (obj) => !SelectedUserList.IsUserAdmin);

        public void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            e.Handled = regex.IsMatch(e.Text);
        }

        public ICommand IsIsBlockedUserChanged
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    if (bool.TryParse(obj.ToString(), out bool isIsBlocked))
                        SelectedUserList.IsBlocked = isIsBlocked;
                },
                (obj) => !SelectedUserList.IsUserAdmin);
            }
        }
    }
}
