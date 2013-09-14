using System;
using System.Linq;
using CodeFramework.Controllers;
using GitHubSharp.Models;
using CodeHub.Controllers;
using MonoTouch.Dialog;
using CodeFramework.Elements;
using System.Collections.Generic;

namespace CodeHub.ViewControllers
{
    public class IssueLabelsViewController : BaseListControllerDrivenViewController, IListView<LabelModel>
    {
        public List<LabelModel> SelectedLabels { get; set; }

        public IssueLabelsViewController(string user, string repo)
        {
            Title = "Labels".t();
            NoItemsText = "No Labels".t();
            SearchPlaceholder = "Search Labels".t();
            Controller = new RepositoryLabelsController(this, user, repo);
            SelectedLabels = new List<LabelModel>();
        }

        public void Render(ListModel<LabelModel> model)
        {
            this.RenderList(model, x => {
                var e = new StyledStringElement(x.Name);

                if (SelectedLabels.Exists(y => y.Name.Equals(x.Name)))
                    e.Accessory = MonoTouch.UIKit.UITableViewCellAccessory.Checkmark;

                e.Tapped += () => {
                    if (e.Accessory == MonoTouch.UIKit.UITableViewCellAccessory.Checkmark)
                    {
                        SelectedLabels.RemoveAll(y => y.Name.Equals(x.Name));
                        e.Accessory = MonoTouch.UIKit.UITableViewCellAccessory.None;
                    }
                    else
                    {
                        SelectedLabels.Add(x);
                        e.Accessory = MonoTouch.UIKit.UITableViewCellAccessory.Checkmark;
                    }

                    Root.Reload(e, MonoTouch.UIKit.UITableViewRowAnimation.None);
                };

                return e;
            });
        }
    }
}

