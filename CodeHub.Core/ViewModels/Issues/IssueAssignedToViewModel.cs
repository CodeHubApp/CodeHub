using System;
using CodeFramework.Core.ViewModels;
using GitHubSharp.Models;
using System.Threading.Tasks;
using CodeHub.Core.Messages;

namespace CodeHub.Core.ViewModels.Issues
{
	public class IssueAssignedToViewModel : LoadableViewModel
    {
		private readonly CollectionViewModel<BasicUserModel> _users = new CollectionViewModel<BasicUserModel>();

		private BasicUserModel _selectedUser;

		public BasicUserModel SelectedUser
		{
			get
			{
				return _selectedUser;
			}
			set
			{
				_selectedUser = value;
				RaisePropertyChanged(() => SelectedUser);
			}
		}

		public CollectionViewModel<BasicUserModel> Users
		{
			get { return _users; }
		}

		public string Username 
		{ 
			get; 
			private set; 
		}

		public string Repository 
		{ 
			get; 
			private set; 
		}

		public void Init(NavObject navObject) 
		{
			Username = navObject.Username;
			Repository = navObject.Repository;
			SelectedUser = TxSevice.Get() as BasicUserModel;
			this.Bind(x => x.SelectedUser, x => {
				Messenger.Publish(new SelectedAssignedToMessage(this) { User = x });
				ChangePresentation(new Cirrious.MvvmCross.ViewModels.MvxClosePresentationHint(this));
			});
		}

		protected override Task Load(bool forceCacheInvalidation)
		{
			return Users.SimpleCollectionLoad(this.GetApplication().Client.Users[Username].Repositories[Repository].GetCollaborators(), forceCacheInvalidation);
		}

		public class NavObject
		{
			public string Username { get; set; }
			public string Repository { get; set; }
		}
    }
}

