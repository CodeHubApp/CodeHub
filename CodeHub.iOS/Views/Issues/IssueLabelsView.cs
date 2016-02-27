using System;
using CodeHub.iOS.ViewControllers;
using CodeHub.Core.ViewModels.Issues;
using System.Linq;
using UIKit;
using CodeHub.iOS.Utilities;
using CodeHub.iOS.DialogElements;

namespace CodeHub.iOS.Views.Issues
{
    public class IssueLabelsView : ViewModelCollectionDrivenDialogViewController
    {
        public IssueLabelsView()
        {
            EnableSearch = false;
            Title = "Labels";
          
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Tag.ToEmptyListImage(), "There are no labels."));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var vm = (IssueLabelsViewModel)ViewModel;
            NavigationItem.LeftBarButtonItem = new UIBarButtonItem(Images.Buttons.BackButton, UIBarButtonItemStyle.Plain, (s, e) => vm.SaveLabelChoices.Execute(null));

            BindCollection(vm.Labels, x => 
            {
                var e = new LabelElement(x.Name, x.Color);
                e.Clicked.Subscribe(_ =>
                {
                    if (e.Accessory == UITableViewCellAccessory.Checkmark)
                        vm.SelectedLabels.Items.Remove(x);
                    else
                        vm.SelectedLabels.Items.Add(x);
                });

                e.Accessory = vm.SelectedLabels.Contains(x) ? 
                               UITableViewCellAccessory.Checkmark : 
                               UITableViewCellAccessory.None;
                return e;
            });

            vm.BindCollection(x => x.SelectedLabels, true).Subscribe(_ =>
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
            });

            vm.Bind(x => x.IsSaving).SubscribeStatus("Saving...");
        }
    }
}

