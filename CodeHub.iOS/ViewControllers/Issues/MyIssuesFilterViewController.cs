using System;
using UIKit;
using CodeHub.iOS.DialogElements;
using CodeHub.Core.ViewModels.Issues;
using ReactiveUI;
using Humanizer;
using CodeHub.iOS.TableViewSources;
using CodeHub.iOS.Views;

namespace CodeHub.iOS.ViewControllers.Issues
{
    public class MyIssuesFilterViewController : BaseTableViewController<MyIssuesFilterViewModel>, IModalViewController
    {
        public MyIssuesFilterViewController()
            : base(UITableViewStyle.Grouped)
        {
            if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad)
                ModalPresentationStyle = UIModalPresentationStyle.FormSheet;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var typeElement = new StringElement("Type", string.Empty, UITableViewCellStyle.Value1);
            var stateElement = new StringElement("State", string.Empty, UITableViewCellStyle.Value1);
            var fieldElement = new StringElement("Field", string.Empty, UITableViewCellStyle.Value1);
            var ascElement = new BooleanElement("Ascending");
            var labelElement = new EntryElement("Labels", "bug,ui,@user", string.Empty) {
                TextAlignment = UITextAlignment.Right, AutocorrectionType = UITextAutocorrectionType.No, AutocapitalizationType = UITextAutocapitalizationType.None
            };

            var filterSection = new Section("Filter") { typeElement, stateElement, labelElement };
            var orderSection = new Section("Order By") { fieldElement, ascElement };
            var searchSection = new Section();
            var footerButton = new TableFooterButton("Search!");
            searchSection.FooterView = footerButton;

            var source = new DialogTableViewSource(TableView);
            TableView.Source = source;
            source.Root.Add(filterSection, orderSection, searchSection);

            OnActivation(d => {
                d(typeElement.Clicked.InvokeCommand(ViewModel.SelectFilterTypeCommand));
                d(stateElement.Clicked.InvokeCommand(ViewModel.SelectStateCommand));
                d(fieldElement.Clicked.InvokeCommand(ViewModel.SelectSortCommand));
                d(footerButton.Clicked.InvokeCommand(ViewModel.SaveCommand));
                d(ascElement.Changed.Subscribe(x => ViewModel.Ascending = x));
                d(labelElement.Changed.Subscribe(x => ViewModel.Labels = x));

                d(this.WhenAnyValue(x => x.ViewModel.FilterType).Subscribe(x => typeElement.Value = x.Humanize()));
                d(this.WhenAnyValue(x => x.ViewModel.State).Subscribe(x => stateElement.Value = x.Humanize()));
                d(this.WhenAnyValue(x => x.ViewModel.Labels).Subscribe(x => labelElement.Value = x));
                d(this.WhenAnyValue(x => x.ViewModel.SortType).Subscribe(x => fieldElement.Value = x.Humanize()));
                d(this.WhenAnyValue(x => x.ViewModel.Ascending).Subscribe(x => ascElement.Value = x));

                d(this.WhenAnyValue(x => x.ViewModel.DismissCommand)
                    .ToBarButtonItem(Images.Cancel, x => NavigationItem.LeftBarButtonItem = x));
                
                d(this.WhenAnyValue(x => x.ViewModel.SaveCommand)
                    .ToBarButtonItem(Images.Search, x => NavigationItem.RightBarButtonItem = x));
            });
        }
    }
}

