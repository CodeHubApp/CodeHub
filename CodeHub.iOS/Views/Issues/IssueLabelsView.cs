using System.Collections.Generic;
using CodeFramework.ViewControllers;
using CodeHub.Core.ViewModels;
using GitHubSharp.Models;
using MonoTouch.Dialog;

namespace CodeHub.iOS.Views.Issues
{
    public class IssueLabelsView : ViewModelCollectionDrivenViewController
    {
        public List<LabelModel> SelectedLabels { get; set; }

        public IssueLabelsView()
        {
            Title = "Labels".t();
            NoItemsText = "No Labels".t();
            SearchPlaceholder = "Search Labels".t();
            SelectedLabels = new List<LabelModel>();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

//            BindCollection(ViewModel.Labels, x =>
//            {
//                var e = new StyledStringElement(x.Name);
//
//                if (SelectedLabels.Exists(y => y.Name.Equals(x.Name)))
//                    e.Accessory = MonoTouch.UIKit.UITableViewCellAccessory.Checkmark;
//
//                e.Tapped += () =>
//                {
//                    if (e.Accessory == MonoTouch.UIKit.UITableViewCellAccessory.Checkmark)
//                    {
//                        SelectedLabels.RemoveAll(y => y.Name.Equals(x.Name));
//                        e.Accessory = MonoTouch.UIKit.UITableViewCellAccessory.None;
//                    }
//                    else
//                    {
//                        SelectedLabels.Add(x);
//                        e.Accessory = MonoTouch.UIKit.UITableViewCellAccessory.Checkmark;
//                    }
//
//                    Root.Reload(e, MonoTouch.UIKit.UITableViewRowAnimation.None);
//                };
//
//                return e;
//            });
        }
    }
}

