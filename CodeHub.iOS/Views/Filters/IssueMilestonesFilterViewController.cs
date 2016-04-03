using System;
using GitHubSharp.Models;
using CodeHub.iOS.ViewControllers;
using System.Linq;
using CodeHub.Core.ViewModels;
using CodeHub.iOS.Utilities;
using CodeHub.iOS.DialogElements;

namespace CodeHub.iOS.Views.Filters
{
    public class IssueMilestonesFilterViewController : DialogViewController
    {
        private readonly CollectionViewModel<MilestoneModel> _milestones = new CollectionViewModel<MilestoneModel>();
        private string _username, _repository;

        public Action<string, int?, string> MilestoneSelected;

        public async override void ViewDidLoad()
        {
            base.ViewDidLoad();

            try
            {
                NetworkActivity.PushNetworkActive();
                var app = MvvmCross.Platform.Mvx.Resolve<CodeHub.Core.Services.IApplicationService>();
                await _milestones.SimpleCollectionLoad(app.Client.Users[_username].Repositories[_repository].Milestones.GetAll());
            }
            catch {
            }
            finally
            {
                NetworkActivity.PopNetworkActive();
            }
        }

        public IssueMilestonesFilterViewController(string user, string repo, bool alreadySelected)
            : base(UIKit.UITableViewStyle.Plain)
        {
            _username = user;
            _repository = repo;
            Title = "Milestones";
            SearchPlaceholder = "Search Milestones";

            var clearMilestone = new MilestoneModel { Title = "Clear milestone filter" };
            var noMilestone = new MilestoneModel { Title = "Issues with no milestone" };
            var withMilestone = new MilestoneModel { Title = "Issues with milestone" };

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
                    var element = new StringElement(x.Title);
                    element.Clicked.Subscribe(_ => {
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
                    });
                    sec.Add(element);
                }

                InvokeOnMainThread(() => Root.Reset(sec));
            };
        }
    }
}

