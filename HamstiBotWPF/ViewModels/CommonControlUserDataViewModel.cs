using DevExpress.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using TBotHamsti.Models.Messages;
using TBotHamsti.Models.Users;
using TBotHamsti.Services;
using TBotHamsti.Views;

namespace TBotHamsti.ViewModels
{
    public class CommonControlUserDataViewModel : BindableBase
    {
        private readonly PageService pageService;
        private readonly MessageBus messageBus;
        private static readonly Regex regex;
        private User selectedUserItem;
        private User selectedUserBeforeModify;
        public static ObservableCollection<User> ListUsers => UsersFunc.AuthUsers;
        public User SelectedUserItem => selectedUserItem;
        public ModeEditUser ModeEditUser { get; set; }

        static CommonControlUserDataViewModel()
        { 
            regex = new Regex("[^0-9]+");
        }

        public CommonControlUserDataViewModel(PageService pageService, MessageBus messageBus)
        {
            this.pageService = pageService;
            this.messageBus = messageBus;
        }

        /// Below the commands to page of changing user data 
        public ICommand CreateUser => new AsyncCommand(async () =>
        {
            await messageBus.SendTo<LogsViewModel>(new TextMessage($"Added new user: {SelectedUserItem.Id_Username}", HorizontalAlignment.Right));
            ListUsers.Add(new User(SelectedUserItem));
            UsersFunc.SaveRefresh();
            pageService.ChangePage(new UsersControlPage());
        });

        public ICommand ModifyUser => new AsyncCommand(async () =>
        {
            await messageBus.SendTo<LogsViewModel>(new TextMessage($"Modify user: ({selectedUserBeforeModify.Id_Username} | {selectedUserBeforeModify.IsBlocked}) => ({SelectedUserItem.Id_Username} | {SelectedUserItem.IsBlocked})", HorizontalAlignment.Right));
            UsersFunc.SaveRefresh();
            pageService.ChangePage(new UsersControlPage());
        });

        public ICommand ConfirmDeleteUser => new AsyncCommand(async () =>
        {
            ListUsers.Remove(SelectedUserItem);
            await messageBus.SendTo<LogsViewModel>(new TextMessage($"Deleted user: {SelectedUserItem.Id_Username} | {SelectedUserItem.IsBlocked}", HorizontalAlignment.Right));
            UsersFunc.SaveRefresh();
            pageService.ChangePage(new UsersControlPage());
        });

        public ICommand CancelEditingUser => new DelegateCommand<object>((obj) =>
        {
            if (ListUsers.Remove(SelectedUserItem))
            {
                ListUsers.Add(selectedUserBeforeModify);
                UsersFunc.Refresh();
            }

            pageService.ChangePage(new UsersControlPage());
        });

        public ICommand ChangeUserBlock => new DelegateCommand<object>((obj) =>
        {
            if (bool.TryParse(obj.ToString(), out bool isBlocked))
            {
                SelectedUserItem.IsBlocked = isBlocked;
            }
            else
            { 
                throw new ArgumentException("invalid parameter value", nameof(isBlocked));
            }
        });

        public ICommand ChangeUserBookmark => new DelegateCommand<object>((obj) =>
        {
            if (bool.TryParse(obj.ToString(), out bool isSetBookmark))
            {
                SelectedUserItem.IsSetBookmark = isSetBookmark;
            }
            else
            { 
                throw new ArgumentException("invalid parameter value", nameof(isSetBookmark));
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
                throw new ArgumentException("Not found selected status. Check please on corrent CommandParameter", nameof(status));
            }
        });
        
        /// Below the commands to list of users
        public ICommand ModifyUserPageChange => new DelegateCommand<object>((obj) =>
        {
            ModeEditUser = ModeEditUser.Edit;
            selectedUserBeforeModify = new User(SetUser(obj));
            pageService.ChangePage(new ChangeUserDataPage());
        });

        public ICommand DeleteUserPageChange => new DelegateCommand<object>((obj) =>
        {
            ModeEditUser = ModeEditUser.Delete;
            selectedUserBeforeModify = new User(SetUser(obj));
            pageService.ChangePage(new ChangeUserDataPage());
        });
        
        public ICommand CreateUserPageChange => new DelegateCommand(() =>
        {
            ModeEditUser = ModeEditUser.Create;
            selectedUserItem = new User();
            pageService.ChangePage(new ChangeUserDataPage());
        });
        
        public ICommand SetBookmark => new AsyncCommand<object>(async (obj) =>
        {
            SetUser(obj).IsSetBookmark = !SelectedUserItem.IsSetBookmark;
            await messageBus.SendTo<LogsViewModel>(new TextMessage($"Set bookmark user: ({SelectedUserItem.Id_Username} | {SelectedUserItem.IsBlocked}) => ({SelectedUserItem.Id_Username} | {SelectedUserItem.IsBlocked})", HorizontalAlignment.Right));
            UsersFunc.SaveRefresh();
        });

        public ICommand BlockUser => new AsyncCommand<object>(async (obj) =>
        {
            SetUser(obj).IsBlocked = !SelectedUserItem.IsBlocked;
            await messageBus.SendTo<LogsViewModel>(new TextMessage($"Block user: ({SelectedUserItem.Id_Username} | {SelectedUserItem.IsBlocked}) => ({SelectedUserItem.Id_Username} | {SelectedUserItem.IsBlocked})", HorizontalAlignment.Right));
            UsersFunc.SaveRefresh();
        });

        public void NumberValidationTextBox(object sender, TextCompositionEventArgs e) => e.Handled = regex.IsMatch(e.Text);
        private User SetUser(object obj) => selectedUserItem = ListUsers.Where(w => w.Equals(obj)).DefaultIfEmpty(new User()).SingleOrDefault();
    }
}
