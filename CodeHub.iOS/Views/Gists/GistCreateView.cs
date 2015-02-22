using System;
using UIKit;
using CodeHub.Core.ViewModels.Gists;
using ReactiveUI;
using CodeHub.iOS.DialogElements;
using CodeHub.iOS.TableViewSources;
using System.Reactive.Linq;
using System.Linq;
using CodeHub.Core.Factories;

namespace CodeHub.iOS.Views.Gists
{
    public class GistCreateView : BaseTableViewController<GistCreateViewModel>
    {
        private readonly ExpandingInputElement _descriptionElement = new ExpandingInputElement("Description (Optional)");
        private readonly BooleanElement _publicElement;
        private readonly StringElement _addFileElement = new StringElement("Add File");
        private readonly Section _fileSection = new Section("Files");
        private readonly IAlertDialogFactory _alertDialogFactory;

        public GistCreateView(IAlertDialogFactory alertDialogFactory)
        {
            _alertDialogFactory = alertDialogFactory;

            this.WhenAnyValue(x => x.ViewModel.SaveCommand)
                .Select(x => x.ToBarButtonItem(UIBarButtonSystemItem.Save))
                .Subscribe(x => NavigationItem.RightBarButtonItem = x);

            _publicElement = new BooleanElement("Public", false, (e) => ViewModel.IsPublic = e.Value);
            this.WhenAnyValue(x => x.ViewModel.IsPublic).Subscribe(x => _publicElement.Value = x);

            this.WhenAnyValue(x => x.ViewModel.Description).Subscribe(x => _descriptionElement.Value = x);
            _descriptionElement.ValueChanged += (sender, e) => ViewModel.Description = _descriptionElement.Value;

            _addFileElement.Image = Octicon.Plus.ToImage();
            _addFileElement.Tapped = () => ViewModel.AddGistFileCommand.ExecuteIfCan();

            this.WhenAnyValue(x => x.ViewModel.Files)
                .Select(x => x.Changed)
                .Switch()
                .Select(_ => ViewModel.Files.Select(x => new FileElement(x)))
                .Subscribe(x => _fileSection.Reset(x));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var source = new EditSource(this);
            source.Root.Add(_fileSection, new Section { _addFileElement }, new Section("Details") { _publicElement, _descriptionElement });

            TableView.Source = source;
            TableView.TableFooterView = new UIView();
        }

        private void PresentFileEditor()
        {

        }

        private class EditSource : DialogTableViewSource
        {
            private readonly GistCreateView _parent;
            public EditSource(GistCreateView dvc) 
                : base (dvc.TableView)
            {
                _parent = dvc;
            }

            public override bool CanEditRow(UITableView tableView, Foundation.NSIndexPath indexPath)
            {
                return Root.IndexOf(_parent._fileSection) == indexPath.Section;
            }

            public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, Foundation.NSIndexPath indexPath)
            {
                return CanEditRow(tableView, indexPath) ? UITableViewCellEditingStyle.Delete : UITableViewCellEditingStyle.None;
            }

            public override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, Foundation.NSIndexPath indexPath)
            {
                switch (editingStyle)
                {
                    case UITableViewCellEditingStyle.Delete:
                        var section = Root[indexPath.Section];
                        var element = section[indexPath.Row] as FileElement;
                        if (element != null)
                            element.ViewModel.DeleteCommand.ExecuteIfCan();
                        break;
                }
            }
        }

        private class FileElement : StringElement
        {
            public GistFileItemViewModel ViewModel { get; private set; }

            public FileElement(GistFileItemViewModel vm) 
                : base(vm.Name, vm.Size + " bytes", UITableViewCellStyle.Subtitle)
            {
                Image = Octicon.FileCode.ToImage();
                Tapped = () => vm.EditCommand.ExecuteIfCan();
                ViewModel = vm;
            }
        }
    }
}

