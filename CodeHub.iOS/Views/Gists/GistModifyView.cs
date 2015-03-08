using System;
using UIKit;
using CodeHub.Core.ViewModels.Gists;
using ReactiveUI;
using CodeHub.iOS.DialogElements;
using CodeHub.iOS.TableViewSources;
using System.Reactive.Linq;
using System.Linq;
using System.Reactive;

namespace CodeHub.iOS.Views.Gists
{
    public abstract class GistModifyView<TViewModel> : BaseTableViewController<TViewModel>, IModalView 
        where TViewModel : GistModifyViewModel
    {
        private readonly ExpandingInputElement _descriptionElement = new ExpandingInputElement("Description (Optional)");
        private readonly StringElement _addFileElement = new StringElement("Add File");
        private readonly Section _fileSection = new Section("Files");

        public DialogTableViewSource Source { get; private set; }

        protected GistModifyView()
        {
            this.WhenAnyValue(x => x.ViewModel.SaveCommand)
                .Select(x => x.ToBarButtonItem(UIBarButtonSystemItem.Save))
                .Subscribe(x => NavigationItem.RightBarButtonItem = x);

            this.WhenAnyValue(x => x.ViewModel.DismissCommand)
                .Select(x => x.ToBarButtonItem(Images.Cancel))
                .Subscribe(x => NavigationItem.LeftBarButtonItem = x);

            this.WhenAnyValue(x => x.ViewModel.Description).Subscribe(x => _descriptionElement.Value = x);
            _descriptionElement.ValueChanged += (sender, e) => ViewModel.Description = _descriptionElement.Value;

            _addFileElement.Image = Octicon.Plus.ToImage();
            _addFileElement.Tapped = () => ViewModel.AddGistFileCommand.ExecuteIfCan();

            this.WhenAnyValue(x => x.ViewModel.Files)
                .Select(x => x.Changed.Select(_ => Unit.Default).StartWith(Unit.Default))
                .Switch()
                .Select(_ => ViewModel.Files.Select(x => new FileElement(x)))
                .Subscribe(x => _fileSection.Reset(x));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Source = new EditSource(this);
            Source.Root.Add(_fileSection, new Section { _addFileElement }, new Section("Details") { _descriptionElement });

            TableView.Source = Source;
            TableView.TableFooterView = new UIView();
        }

        private class EditSource : DialogTableViewSource
        {
            private readonly GistModifyView<TViewModel> _parent;
            public EditSource(GistModifyView<TViewModel> dvc) 
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
                : base(vm.Name)
            {
                Image = Octicon.FileCode.ToImage();
                Tapped = () => vm.EditCommand.ExecuteIfCan();
                ViewModel = vm;
            }
        }
    }
}

