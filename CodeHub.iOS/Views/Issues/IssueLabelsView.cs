using CodeFramework.iOS.Views;
using MonoTouch.Dialog;
using CodeHub.Core.ViewModels.Issues;
using GitHubSharp.Models;
using System.Linq;
using MonoTouch.UIKit;
using CodeFramework.iOS.Utils;
using CodeFramework.iOS.Elements;

namespace CodeHub.iOS.Views.Issues
{
    public class IssueLabelsView : ViewModelCollectionDrivenDialogViewController
    {
		public IssueLabelsView()
		{
			EnableSearch = false;
			Title = "Labels".t();
			NoItemsText = "No Labels".t();
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

			var vm = (IssueLabelsViewModel)ViewModel;
			NavigationItem.LeftBarButtonItem = new UIBarButtonItem(Theme.CurrentTheme.BackButton, UIBarButtonItemStyle.Plain, (s, e) => vm.SaveLabelChoices.Execute(null));

			BindCollection(vm.Labels, x => 
            {
                var e = new LabelElement(x.Name, x.Color);
                e.Tapped += () =>
                {
                    if (e.Accessory == UITableViewCellAccessory.Checkmark)
						vm.SelectedLabels.Items.Remove(x);
                    else
						vm.SelectedLabels.Items.Add(x);
                };

				e.Accessory = vm.SelectedLabels.Contains(x) ? 
				               UITableViewCellAccessory.Checkmark : 
				               UITableViewCellAccessory.None;
                return e;
            });

			vm.BindCollection(x => x.SelectedLabels, x =>
			{
				if (Root.Count == 0)
					return;

				var elements = Root[0].Elements;
				foreach (var el in elements.Cast<LabelElement>())
				{
                    var element = el;
                    el.Accessory = vm.SelectedLabels.Any(y => string.Equals(y.Name, element.Name, System.StringComparison.OrdinalIgnoreCase)) ? 
					               UITableViewCellAccessory.Checkmark : 
					               UITableViewCellAccessory.None;
				}

				Root.Reload(Root[0], UITableViewRowAnimation.None);
			}, true);

			var _hud = new Hud(View);
			vm.Bind(x => x.IsSaving, x =>
			{
				if (x) _hud.Show("Saving...");
				else _hud.Hide();
			});
        }
    }
}

