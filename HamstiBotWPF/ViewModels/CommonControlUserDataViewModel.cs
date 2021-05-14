using DevExpress.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TBotHamsti.Core;
using TBotHamsti.LogicRepository;
using TBotHamsti.Messages;
using TBotHamsti.Pages;
using TBotHamsti.Services;
using StatusUser = TBotHamsti.LogicRepository.RepUsers.StatusUser;

namespace TBotHamsti.ViewModels
{
    public enum ModeEditUser
    { 
        Create,
        Edit,
        Delete
    }

    public class CommonControlUserDataViewModel : BindableBase
    {
        private readonly PageService pageService;
        private readonly MessageBus messageBus;
        private readonly System.Text.RegularExpressions.Regex regex;
        public static ObservableCollection<PatternUser> ListUsers => RepUsers.AuthUsers;

        private PatternUser selectedUserItem;
        private PatternUser selectedUserBeforeModify;

        public PatternUser SelectedUserItem => selectedUserItem;

        public ModeEditUser ModeEditUser
        {
            get; set;
        }

        public CommonControlUserDataViewModel(PageService pageService, MessageBus messageBus)
        {
            this.pageService = pageService;
            this.messageBus = messageBus;
            
            regex = new System.Text.RegularExpressions.Regex("[^0-9]+");
        }

        /// Below the commands to page of changing user data 
        public ICommand CreateUser => new AsyncCommand(async () =>
        {
            await messageBus.SendTo<LogsViewModel>(new TextMessage($"Added new user: {SelectedUserItem.IdUser_Nickname}", HorizontalAlignment.Right));
            ListUsers.Add(new PatternUser(SelectedUserItem));
            RepUsers.SaveRefresh();
            pageService.ChangePage(new UsersControlPage());
        });

        public ICommand ModifyUser => new AsyncCommand(async () =>
        {
            await messageBus.SendTo<LogsViewModel>(new TextMessage($"Modify user: ({selectedUserBeforeModify.IdUser_Nickname} | {selectedUserBeforeModify.IsBlocked}) => ({SelectedUserItem.IdUser_Nickname} | {SelectedUserItem.IsBlocked})", HorizontalAlignment.Right));
            RepUsers.SaveRefresh();
            pageService.ChangePage(new UsersControlPage());
        });

        public ICommand ConfirmDeleteUser => new AsyncCommand(async () =>
        {
            ListUsers.Remove(SelectedUserItem);
            await messageBus.SendTo<LogsViewModel>(new TextMessage($"Deleted user: {SelectedUserItem.IdUser_Nickname} | {SelectedUserItem.IsBlocked}", HorizontalAlignment.Right));
            RepUsers.SaveRefresh();
            pageService.ChangePage(new UsersControlPage());
        });

        public ICommand ChangeUserBlock => new DelegateCommand<object>((obj) =>
        {
            if (bool.TryParse(obj.ToString(), out bool isBlocked))
            {
                SelectedUserItem.IsBlocked = isBlocked;
            }
        });

        public ICommand CancelEditingUser => new DelegateCommand<object>((obj) =>
        {
            if (ListUsers.Remove(SelectedUserItem))
            {
                ListUsers.Add(selectedUserBeforeModify);
                RepUsers.Refresh();
            }

            pageService.ChangePage(new UsersControlPage());
        });

        public ICommand ChangeUserBookmark => new DelegateCommand<object>((obj) =>
        {
            if (bool.TryParse(obj.ToString(), out bool isSetBookmark))
            {
                SelectedUserItem.IsSetBookmark = isSetBookmark;
            }
        });

        public ICommand ChangeUserStatus => new DelegateCommand<object>((obj) =>
        {
            if (Enum.TryParse(obj.ToString(), out StatusUser status))
            {
                SelectedUserItem.Status = status;
            }
            else
            {
                throw new Exception("Not found selected status. Check please on corrent CommandParameter");
            }
        });
        
        /// Below the commands to list of users
        public ICommand ModifyUserPageChange => new DelegateCommand<object>((obj) =>
        {
            ModeEditUser = ModeEditUser.Edit;
            selectedUserBeforeModify = new PatternUser(SetUser(obj));
            pageService.ChangePage(new ChangeUserDataPage());
        });

        public ICommand DeleteUserPageChange => new DelegateCommand<object>((obj) =>
        {
            ModeEditUser = ModeEditUser.Delete;
            selectedUserBeforeModify = new PatternUser(SetUser(obj));
            pageService.ChangePage(new ChangeUserDataPage());
        });
        
        public ICommand CreateUserPageChange => new DelegateCommand(() =>
        {
            ModeEditUser = ModeEditUser.Create;
            selectedUserItem = new PatternUser();
            pageService.ChangePage(new ChangeUserDataPage());
        });
        
        public ICommand SetBookmark => new AsyncCommand<object>(async (obj) =>
        {
            SetUser(obj).IsSetBookmark = !SelectedUserItem.IsSetBookmark;
            await messageBus.SendTo<LogsViewModel>(new TextMessage($"Set bookmark user: ({SelectedUserItem.IdUser_Nickname} | {SelectedUserItem.IsBlocked}) => ({SelectedUserItem.IdUser_Nickname} | {SelectedUserItem.IsBlocked})", HorizontalAlignment.Right));
            RepUsers.SaveRefresh();
        });

        public ICommand BlockUser => new AsyncCommand<object>(async (obj) =>
        {
            SetUser(obj).IsBlocked = !SelectedUserItem.IsBlocked;
            await messageBus.SendTo<LogsViewModel>(new TextMessage($"Block user: ({SelectedUserItem.IdUser_Nickname} | {SelectedUserItem.IsBlocked}) => ({SelectedUserItem.IdUser_Nickname} | {SelectedUserItem.IsBlocked})", HorizontalAlignment.Right));
            RepUsers.SaveRefresh();
        });

        public void NumberValidationTextBox(object sender, TextCompositionEventArgs e) => e.Handled = regex.IsMatch(e.Text);
        private PatternUser SetUser(object obj) => selectedUserItem = ListUsers.Where(w => w.Equals(obj)).DefaultIfEmpty(new PatternUser()).SingleOrDefault();
    }
}
