using System;
using CodeHub.Core.ViewModels.Issues;
using CodeHub.iOS.Elements;
using MonoTouch.UIKit;
using ReactiveUI;
using Xamarin.Utilities.ViewControllers;

namespace CodeHub.iOS.Views.Issues
{
    public class IssueLabelsView : ViewModelCollectionViewController<IssueLabelsViewModel>
    {
		public IssueLabelsView()
            : base(searchbarEnabled: false)
		{
		}

        public override void ViewDidLoad()
        {
            Title = "Labels";
            //NoItemsText = "No Labels";

            base.ViewDidLoad();

			NavigationItem.LeftBarButtonItem = new UIBarButtonItem(Images.BackButton, UIBarButtonItemStyle.Plain,
			    (s, e) =>
			    {
			        if (ViewModel.SaveOnSelect)
                        ViewModel.SelectLabelsCommand.ExecuteIfCan();
			    });


            this.BindList(ViewModel.Labels, label =>
            {
                var element = new LabelElement(label);
                element.Tapped += () =>
                {
                    if (ViewModel.SelectedLabels.Contains(label))
                        ViewModel.SelectedLabels.Remove(label);
                    else
                        ViewModel.SelectedLabels.Add(label);
                };
                element.Accessory = ViewModel.SelectedLabels.Contains(label)
                           ? UITableViewCellAccessory.Checkmark
                           : UITableViewCellAccessory.None;
                return element;
            });

            ViewModel.SelectedLabels.Changed.Subscribe(x => ViewModel.Labels.Reset());
        }
    }
}

