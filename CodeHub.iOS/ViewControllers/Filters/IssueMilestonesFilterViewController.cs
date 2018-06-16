using System;
using System.Linq;
using CodeHub.Core.ViewModels;
using CodeHub.iOS.Utilities;
using CodeHub.iOS.DialogElements;
using Splat;
using ReactiveUI;

namespace CodeHub.iOS.ViewControllers.Filters
{
    public class IssueMilestonesFilterViewController : DialogViewController
    {
        private readonly ReactiveList<Octokit.Milestone> _milestones = new ReactiveList<Octokit.Milestone>();
        private string _username, _repository;

        public Action<string, int?, string> MilestoneSelected;

        public async override void ViewDidLoad()
        {
            base.ViewDidLoad();

            try
            {
                NetworkActivity.PushNetworkActive();
                var app = Locator.Current.GetService<Core.Services.IApplicationService>();
                var result = await app.GitHubClient.Issue.Milestone.GetAllForRepository(_username, _repository);
                _milestones.Reset(result);
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

            //var clearMilestone = new MilestoneModel { Title = "Clear milestone filter" };
            //var noMilestone = new MilestoneModel { Title = "Issues with no milestone" };
            //var withMilestone = new MilestoneModel { Title = "Issues with milestone" };

            _milestones.CollectionChanged += (sender, e) => {
                var items = _milestones.ToList();

                //items.Insert(0, noMilestone);
                //items.Insert(1, withMilestone);

                //if (alreadySelected)
                    //items.Insert(0, clearMilestone);

                var sec = new Section();
                foreach (var item in items)
                {
                    var x = item;
                    var element = new StringElement(x.Title);
                    element.Clicked.Subscribe(_ => {
                        if (MilestoneSelected != null)
                        {
                            //if (x == noMilestone)
                            //    MilestoneSelected(x.Title, null, "none");
                            //else if (x == withMilestone)
                            //    MilestoneSelected(x.Title, null, "*");
                            //else if (x == clearMilestone)
                            //    MilestoneSelected(null, null, null);
                            //else
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

