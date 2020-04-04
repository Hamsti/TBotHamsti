using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows.Input;
using DevExpress.Mvvm;
using HamstiBotWPF.Services;
using HamstiBotWPF.Core;
using HamstiBotWPF.Pages;
using HamstiBotWPF.Events;
using HamstiBotWPF.Messages;
using HamstiBotWPF.LogicRepository;
using StatusUser = HamstiBotWPF.LogicRepository.RepUsers.StatusUser;
using System.Windows;

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
        public static ObservableCollection<PatternUser> ListUsers => GlobalUnit.AuthUsers;

        public bool ExitsSelectedUser => ListUsers.Count(c => c == SelectedUserList) > 0;

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
            this.eventBus.Subscribe<ModifySelectedUser>(async _ => selectedUserBeforeModify = new PatternUser(SelectedUserList));
            regex = new System.Text.RegularExpressions.Regex("[^0-9]+");
            selectedUserList = new PatternUser();
        }

        public ICommand CreateNewUser => new AsyncCommand(async () =>
        {
            await messageBus.SendTo<LogsViewModel>(new TextMessage($"Added new user: {SelectedUserList.IdUser_Nickname}", HorizontalAlignment.Right));
            GlobalUnit.AuthUsers.Add(new PatternUser(SelectedUserList));
            RepUsers.Save();
            await eventBus.Publish(new RefreshUsersListEvent());
            pageService.ChangePage(new UsersControlPage());
        }, () => !ExitsSelectedUser);

        public ICommand ModifySelectedUser => new AsyncCommand(async () =>
        {
            await messageBus.SendTo<LogsViewModel>(new TextMessage($"Modify user: ({selectedUserBeforeModify.IdUser_Nickname} | {selectedUserBeforeModify.IsBlocked}) => ({SelectedUserList.IdUser_Nickname} | {SelectedUserList.IsBlocked})", HorizontalAlignment.Right));
            RepUsers.Save();
            await eventBus.Publish(new RefreshUsersListEvent());
            pageService.ChangePage(new UsersControlPage());
        }, () => ExitsSelectedUser);

        public ICommand DeleteSelectedUser => new AsyncCommand(async () =>
        {
            // if the selected or default (new) user not found and if selected user is admin, then return
            if (ListUsers.Count((f) => f.IdUser == SelectedUserList.IdUser) < 0) return;
            await messageBus.SendTo<LogsViewModel>(new TextMessage($"Deleted user: {SelectedUserList.IdUser_Nickname} | {SelectedUserList.IsBlocked}", HorizontalAlignment.Right));
            ListUsers.Remove(ListUsers.Where((f) => f.IdUser == SelectedUserList.IdUser).FirstOrDefault());
            RepUsers.Save();
            await eventBus.Publish(new RefreshUsersListEvent());
            SelectedUserList = new PatternUser();
        }, () => ListUsers.Count(c => c.Status == StatusUser.Admin && c != SelectedUserList) > 0);

        public void NumberValidationTextBox(object sender, TextCompositionEventArgs e) => e.Handled = regex.IsMatch(e.Text);

        public ICommand IsIsBlockedUserChanged => new DelegateCommand((obj) =>
        {
            if (bool.TryParse(obj.ToString(), out bool isBlocked))
                SelectedUserList.IsBlocked = isBlocked;
        }, (obj) => ListUsers.Count(c => !c.IsBlocked && c.Status == StatusUser.Admin && c != SelectedUserList) > 0 || SelectedUserList.Status < StatusUser.Admin);

        public ICommand IsStatusUserChanged => new DelegateCommand((obj) =>
        {
            foreach (var Status in Enum.GetValues(typeof(StatusUser)))
                if (Status.ToString() == obj.ToString())
                {
                    SelectedUserList.Status = (StatusUser)Status;
                    return;
                }
            throw new Exception("Not found selected status. Check please on corrent CommandParameter");
        });
    }
}
