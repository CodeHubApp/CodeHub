using System;
using GitHubSharp.Models;
using MonoTouch.Dialog;
using CodeFramework.ViewControllers;
using System.Linq;
using CodeFramework.Core.ViewModels;
using CodeHub.Core.ViewModels;

namespace CodeHub.iOS.Views.Filters
{
	public class IssueMilestonesFilterViewController : BaseDialogViewController
    {
		private readonly CollectionViewModel<MilestoneModel> _milestones = new CollectionViewModel<MilestoneModel>();
		private string _username, _repository;

        public Action<string, int?, string> MilestoneSelected;

		public async override void ViewDidLoad()
		{
			base.ViewDidLoad();

			try
			{
				MonoTouch.Utilities.PushNetworkActive();
				var app = Cirrious.CrossCore.Mvx.Resolve<CodeHub.Core.Services.IApplicationService>();
				await _milestones.SimpleCollectionLoad(app.Client.Users[_username].Repositories[_repository].Milestones.GetAll(), false);
			}
			catch {
			}
			finally
			{
				MonoTouch.Utilities.PopNetworkActive();
			}
		}

        public IssueMilestonesFilterViewController(string user, string repo, bool alreadySelected)
			: base(true)
        {
			Style = UIKit.UITableViewStyle.Plain;
			_username = user;
			_repository = repo;
            Title = "Milestones".t();
            SearchPlaceholder = "Search Milestones".t();

            var clearMilestone = new MilestoneModel { Title = "Clear milestone filter".t() };
            var noMilestone = new MilestoneModel { Title = "Issues with no milestone".t() };
            var withMilestone = new MilestoneModel { Title = "Issues with milestone".t() };

			_milestones.CollectionChanged += (sender, e) => {
				var items = _milestones.ToList();

				items.Insert(0, noMilestone);
				items.Insert(1, withMilestone);

				if (alreadySelected)
					items.Insert(0, clearMilestone);

				var sec = new Section();
				foreach (var item in items)
				{
					var x = item;
					sec.Add(new StyledStringElement(x.Title, () => {
						if (MilestoneSelected != null)
						{
							if (x == noMilestone)
								MilestoneSelected(x.Title, null, "none");
							else if (x == withMilestone)
								MilestoneSelected(x.Title, null, "*");
							else if (x == clearMilestone)
								MilestoneSelected(null, null, null);
							else
								MilestoneSelected(x.Title, x.Number, x.Number.ToString());
						}
					}));
				}

				InvokeOnMainThread(() => {
					Root = new RootElement(Title) { sec };
				});
			};
        }
    }
}

