using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using CodeFramework.Core.ViewModels;
using CodeHub.Core.Filters;
using CodeHub.Core.ViewModels.Issues;
using CodeHub.Core.ViewModels.PullRequests;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels
{
    public class NotificationsViewModel : LoadableViewModel
    {
        private readonly FilterableCollectionViewModel<NotificationModel, NotificationsFilterModel> _notifications;
        private ICommand _readAllCommand;
        private ICommand _readCommand;

        public FilterableCollectionViewModel<NotificationModel, NotificationsFilterModel> Notifications
        {
            get { return _notifications; }
        }

        public ICommand ReadCommand
        {
            get { return _readCommand ?? (_readCommand = new MvxCommand<NotificationModel>(x => Read(x)));}
        }

        public ICommand ReadAllCommand
        {
            get
            {
                return _readAllCommand ?? (_readAllCommand = new MvxCommand(MarkAllAsRead, () =>
                {

                    return true;
                }));
            }
        }

        public ICommand GoToNotificationCommand
        {
            get { return new MvxCommand<NotificationModel>(GoToNotification); }
        }

        private void GoToNotification(NotificationModel x)
        {
            var subject = x.Subject.Type.ToLower();
            if (subject.Equals("issue"))
            {
                ReadCommand.Execute(x);
                var node = x.Subject.Url.Substring(x.Subject.Url.LastIndexOf('/') + 1);
                ShowViewModel<IssueViewModel>(new IssueViewModel.NavObject { Username = x.Repository.Owner.Login,Repository = x.Repository.Name, Id = ulong.Parse(node) });
            }
            else if (subject.Equals("pullrequest"))
            {
                ReadCommand.Execute(x);
                var node = x.Subject.Url.Substring(x.Subject.Url.LastIndexOf('/') + 1);
                ShowViewModel<PullRequestViewModel>(new PullRequestViewModel.NavObject { Username = x.Repository.Owner.Login, Repository = x.Repository.Name, Id = ulong.Parse(node) });
            }
            else if (subject.Equals("commit"))
            {
                ReadCommand.Execute(x);
                var node = x.Subject.Url.Substring(x.Subject.Url.LastIndexOf('/') + 1);
                ShowViewModel<ChangesetViewModel>(new ChangesetViewModel.NavObject { Username = x.Repository.Owner.Login, Repository = x.Repository.Name, Node = node });
            }
        }

        public NotificationsViewModel()
        {
            _notifications = new FilterableCollectionViewModel<NotificationModel, NotificationsFilterModel>("Notifications");
            _notifications.GroupingFunction = (n) => n.GroupBy(x => x.Repository.FullName);
            _notifications.Bind(x => x.Filter, async () =>
            {
                IsLoading = true;
                try
                {
                    await Load(false);
                }
                catch (Exception e)
                {
                }
                finally
                {
                    IsLoading = false;
                }
            });
        }

        protected override Task Load(bool forceDataRefresh)
        {
            return Task.Run(() => this.RequestModel(Application.Client.Notifications.GetAll(all: Notifications.Filter.All, participating: Notifications.Filter.Participating), forceDataRefresh, response => {
                Notifications.Items.Reset(response.Data);
                UpdateAccountNotificationsCount();
            }));
        }

        public async Task Read(NotificationModel model)
        {
//            var response = await Application.Client.ExecuteAsync(Application.Client.Notifications[model.Id].MarkAsRead());
//            if (response.Data) 
//            {
//                //We just read it
//                model.Unread = false;
//
//                // Only remove if we're not looking at all
//                if (Notifications.Filter.All == false)
//                    Notifications.Items.Remove(model);
//
//                //Update the notifications count on the account
//                UpdateAccountNotificationsCount();
//            }
        }

        private void MarkAllAsRead()
        {
            
        }

        private void UpdateAccountNotificationsCount()
        {
            // Only update if we're looking at 
            if (Notifications.Filter.All == false && Notifications.Filter.Participating == false)
                Application.Account.Notifications = Notifications.Items.Sum(x => x.Unread ? 1 : 0);
        }
    }
}

