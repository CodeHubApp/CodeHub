using System;
using System.Linq;
using CodeFramework.iOS.Elements;
using CodeFramework.ViewControllers;
using CodeHub.Core.ViewModels;
using GitHubSharp.Models;

namespace CodeHub.iOS.Views.Issues
{
    public class IssueAssigneesView : ViewModelCollectionDrivenViewController
    {
        public Action<BasicUserModel> SelectedUser;

        public new RepositoryCollaboratorsViewModel ViewModel
        {
            get { return (RepositoryCollaboratorsViewModel)base.ViewModel; }
            protected set { base.ViewModel = value; }
        }

        public override void ViewDidLoad()
        {
            Title = "Assignees".t();
            NoItemsText = "No Assignees".t();
            SearchPlaceholder = "Search Assignees".t();

            base.ViewDidLoad();

            //Add a fake 'Unassigned' guy so we can always unassigned what we've done
            ViewModel.BindCollection(x => x.Collaborators, (ev) =>
            {
                var items = ViewModel.Collaborators.ToList();
                var notAssigned = new BasicUserModel { Id = 0, Login = "Unassigned" };
                items.Insert(0, notAssigned);

                RenderList(items, x =>
                {
                    var e = new UserElement(x.Login, string.Empty, string.Empty, x.AvatarUrl);
                    e.Accessory = MonoTouch.UIKit.UITableViewCellAccessory.DisclosureIndicator;
                    e.Tapped += () =>
                    {
                        if (SelectedUser != null)
                            SelectedUser(x == notAssigned ? null : x);
                    };
                    return e;
                }, ViewModel.Collaborators.MoreItems);
            });
        }
    }
}

