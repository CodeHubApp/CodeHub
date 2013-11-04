using System;
using System.Linq;
using System.Threading.Tasks;
using CodeFramework.Core.ViewModels;
using CodeHub.Core.Filters;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels
{
    public class NotificationsViewModel : BaseViewModel, ILoadableViewModel
    {
        private readonly FilterableCollectionViewModel<NotificationModel, NotificationsFilterModel> _notifications;
        private bool _isLoading;

        public bool IsLoading
        {
            get { return _isLoading; }
            protected set
            {
                _isLoading = value;
                RaisePropertyChanged(() => IsLoading);
            }
        }

        public FilterableCollectionViewModel<NotificationModel, NotificationsFilterModel> Notifications
        {
            get { return _notifications; }
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

        public Task Load(bool forceDataRefresh)
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

        private void UpdateAccountNotificationsCount()
        {
            // Only update if we're looking at 
            if (Notifications.Filter.All == false && Notifications.Filter.Participating == false)
                Application.Account.Notifications = Notifications.Items.Sum(x => x.Unread ? 1 : 0);
        }
    }
}

