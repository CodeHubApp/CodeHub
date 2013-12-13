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
using CodeHub.Core.Messages;

namespace CodeHub.Core.ViewModels
{
    public class NotificationsViewModel : LoadableViewModel
    {
        private readonly FilterableCollectionViewModel<NotificationModel, NotificationsFilterModel> _notifications;
		private ICommand _readAllCommand;
		private ICommand _readCommand;
		private ICommand _readReposCommand;
		private int _shownIndex;
		private bool _isMarking;

        public FilterableCollectionViewModel<NotificationModel, NotificationsFilterModel> Notifications
        {
            get { return _notifications; }
        }

		public int ShownIndex
		{
			get { return _shownIndex; }
			private set
			{
				_shownIndex = value;
				RaisePropertyChanged(() => ShownIndex);
			}
		}

		public bool IsMarking
		{
			get { return _isMarking; }
			set
			{
				_isMarking = value;
				RaisePropertyChanged(() => IsMarking);
			}
		}

        public ICommand ReadCommand
        {
            get { return _readCommand ?? (_readCommand = new MvxCommand<NotificationModel>(Read));}
        }

		public ICommand ReadRepositoriesCommand
		{
			get { return _readReposCommand ?? (_readReposCommand = new MvxCommand<string>(MarkRepoAsRead)); }
		}

        public ICommand ReadAllCommand
        {
			get { return _readAllCommand ?? (_readAllCommand = new MvxCommand(MarkAllAsRead, () => ShownIndex != 2 && !IsLoading && !IsMarking && Notifications.Any())); }
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
			_notifications.Bind(x => x.Filter, () => LoadCommand.Execute(false));
			this.Bind(x => x.ShownIndex, x => {
				if (x == 0) _notifications.Filter = NotificationsFilterModel.CreateUnreadFilter();
				else if (x == 1) _notifications.Filter = NotificationsFilterModel.CreateParticipatingFilter();
				else _notifications.Filter = NotificationsFilterModel.CreateAllFilter();
				((IMvxCommand)ReadAllCommand).RaiseCanExecuteChanged();
			});
			this.Bind(x => x.IsLoading, ((IMvxCommand)ReadAllCommand).RaiseCanExecuteChanged);

			if (_notifications.Filter.Equals(NotificationsFilterModel.CreateUnreadFilter()))
				_shownIndex = 0;
			else if (_notifications.Filter.Equals(NotificationsFilterModel.CreateParticipatingFilter()))
				_shownIndex = 1;
			else
				_shownIndex = 2;

        }

        protected override Task Load(bool forceCacheInvalidation)
        {
			return Task.Run(() => this.RequestModel(this.GetApplication().Client.Notifications.GetAll(all: Notifications.Filter.All, participating: Notifications.Filter.Participating), forceCacheInvalidation, response => {
                Notifications.Items.Reset(response.Data);
                UpdateAccountNotificationsCount();
            }));
        }

		private async void Read(NotificationModel model)
        {
			// If its already read, ignore it
			if (!model.Unread)
				return;

			var response = await this.GetApplication().Client.ExecuteAsync(this.GetApplication().Client.Notifications[model.Id].MarkAsRead());
            if (response.Data) 
            {
                //We just read it
                model.Unread = false;
 
                //Update the notifications count on the account
				Notifications.Items.Remove(model);
                UpdateAccountNotificationsCount();
            }
        }

		private async void MarkRepoAsRead(string repo)
		{
			var items = Notifications.Items.Where(x => string.Equals(x.Repository.FullName, repo, StringComparison.OrdinalIgnoreCase)).ToList();

			try
			{
				IsMarking = true;

				foreach (var notification in items)
				{
					try
					{
						await this.GetApplication().Client.ExecuteAsync(this.GetApplication().Client.Notifications[notification.Id].MarkAsRead());
						notification.Unread = false;
					}
					catch
					{
						//Ignore?
					}
				}

				Notifications.Items.RemoveRange(items);
				UpdateAccountNotificationsCount();
			}
			finally
			{
				IsMarking = false;
			}
		}

		private async void MarkAllAsRead()
		{
			// Make sure theres some sort of notification
			if (!Notifications.Any())
				return;

			try
			{
				IsMarking = true;

				foreach (var notification in Notifications)
				{
					try
					{
						await this.GetApplication().Client.ExecuteAsync(this.GetApplication().Client.Notifications[notification.Id].MarkAsRead());
						notification.Unread = false;
					}
					catch
					{
						//Ignore?
					}
				}

				Notifications.Items.Clear();
				UpdateAccountNotificationsCount();
			}
			finally
			{
				IsMarking = false;
			}
        }

        private void UpdateAccountNotificationsCount()
        {
            // Only update if we're looking at 
			if (!Notifications.Filter.All && !Notifications.Filter.Participating)
				Messenger.Publish<NotificationCountMessage>(new NotificationCountMessage(this) { Count = Notifications.Items.Sum(x => x.Unread ? 1 : 0) });
        }
    }
}

