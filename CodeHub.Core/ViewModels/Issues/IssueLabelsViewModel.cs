using System;
using CodeFramework.Core.ViewModels;
using System.Threading.Tasks;
using GitHubSharp.Models;
using System.Collections.Generic;
using Cirrious.MvvmCross.Plugins.Messenger;
using CodeHub.Core.Messages;
using System.Linq;

namespace CodeHub.Core.ViewModels.Issues
{
	public class IssueLabelsViewModel : LoadableViewModel
    {
		private readonly CollectionViewModel<LabelModel> _labels = new CollectionViewModel<LabelModel>();
		private readonly CollectionViewModel<LabelModel> _selectedLabels = new CollectionViewModel<LabelModel>();

		public CollectionViewModel<LabelModel> Labels
		{
			get { return _labels; }
		}

		public CollectionViewModel<LabelModel> SelectedLabels
		{
			get { return _selectedLabels; }
		}

		public string Username { get; private set; }

		public string Repository { get; private set; }

		public void Init(NavObject navObject)
		{
			Username = navObject.Username;
			Repository = navObject.Repository;
			SelectedLabels.Items.Reset(GetService<CodeFramework.Core.Services.IViewModelTxService>().Get() as IEnumerable<LabelModel>);

			var messenger = GetService<IMvxMessenger>();
			this.BindCollection(x => x.SelectedLabels, x => messenger.Publish(new SelectIssueLabelsMessage(this) { Labels = SelectedLabels.Items.ToArray() }));
		}

		protected override Task Load(bool forceCacheInvalidation)
		{
			return Labels.SimpleCollectionLoad(this.GetApplication().Client.Users[Username].Repositories[Repository].GetLabels(), forceCacheInvalidation);
		}

		public class NavObject
		{
			public string Username { get; set; }
			public string Repository { get; set; }
		}
    }
}

