using System;
using UIKit;
using CodeHub.iOS.DialogElements;
using CodeHub.iOS.ViewComponents;
using CodeHub.Core.ViewModels.Issues;
using ReactiveUI;
using Humanizer;
using CodeHub.iOS.TableViewSources;
using System.Reactive.Linq;

namespace CodeHub.iOS.Views.Issues
{
    public class MyIssuesFilterView : BaseTableViewController<MyIssuesFilterViewModel>, IModalView
    {
        public MyIssuesFilterView()
            : base(UITableViewStyle.Grouped)
        {
            this.WhenAnyValue(x => x.ViewModel.DismissCommand)
                .Select(x => x.ToBarButtonItem(Images.Cancel))
                .Subscribe(x => NavigationItem.LeftBarButtonItem = x);

            this.WhenAnyValue(x => x.ViewModel.SaveCommand)
                .Select(x => x.ToBarButtonItem(Images.Search))
                .Subscribe(x => NavigationItem.RightBarButtonItem = x);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var source = new DialogTableViewSource(TableView);
            TableView.Source = source;

            var typeElement = new StringElement("Type", () => ViewModel.SelectFilterTypeCommand.ExecuteIfCan());
            typeElement.Style = UITableViewCellStyle.Value1;

            var stateElement = new StringElement("State", () => ViewModel.SelectStateCommand.ExecuteIfCan());
            stateElement.Style = UITableViewCellStyle.Value1;

            var labelElement = new EntryElement("Labels", "bug,ui,@user", string.Empty) {
                TextAlignment = UITextAlignment.Right, AutocorrectionType = UITextAutocorrectionType.No, AutocapitalizationType = UITextAutocapitalizationType.None
            };
            labelElement.Changed += (sender, e) => ViewModel.Labels = labelElement.Value;

            var fieldElement = new StringElement("Field", () => ViewModel.SelectSortCommand.ExecuteIfCan());
            fieldElement.Style = UITableViewCellStyle.Value1;

            var ascElement = new BooleanElement("Ascending", false, x => ViewModel.Ascending = x.Value);

            var filterSection = new Section("Filter") { typeElement, stateElement, labelElement };
            var orderSection = new Section("Order By") { fieldElement, ascElement };
            var searchSection = new Section();
            searchSection.FooterView = new TableFooterButton("Search!", () => ViewModel.SaveCommand.ExecuteIfCan());
            source.Root.Add(filterSection, orderSection, searchSection);

            this.WhenActivated(d => {
                d(this.WhenAnyValue(x => x.ViewModel.FilterType).Subscribe(x => typeElement.Value = x.Humanize()));
                d(this.WhenAnyValue(x => x.ViewModel.State).Subscribe(x => stateElement.Value = x.Humanize()));
                d(this.WhenAnyValue(x => x.ViewModel.Labels).Subscribe(x => labelElement.Value = x));
                d(this.WhenAnyValue(x => x.ViewModel.SortType).Subscribe(x => fieldElement.Value = x.Humanize()));
                d(this.WhenAnyValue(x => x.ViewModel.Ascending).Subscribe(x => ascElement.Value = x));
            });
        }
    }
}

