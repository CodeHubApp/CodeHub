using System;
using UIKit;
using CodeHub.Core.ViewModels.Gists;
using ReactiveUI;
using CodeHub.iOS.DialogElements;
using CodeHub.iOS.TableViewSources;
using System.Reactive.Linq;
using System.Linq;
using System.Reactive;

namespace CodeHub.iOS.ViewControllers.Gists
{
    public abstract class GistModifyViewController<TViewModel> : BaseTableViewController<TViewModel>, IModalViewController 
        where TViewModel : GistModifyViewModel
    {
        private readonly Section _fileSection = new Section("Files");

        public DialogTableViewSource Source { get; private set; }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var addFileElement = new StringElement("Add File", Octicon.Plus.ToImage());
            var descriptionElement = new ExpandingInputElement("Description (Optional)");

            Source = new EditSource(this);
            Source.Root.Add(_fileSection, new Section { addFileElement }, new Section("Details") { descriptionElement });

            TableView.Source = Source;
            TableView.TableFooterView = new UIView();

            OnActivation(d => {
                d(addFileElement.Clicked.InvokeCommand(ViewModel.AddGistFileCommand));

                d(this.WhenAnyValue(x => x.ViewModel.Description).Subscribe(x => descriptionElement.Value = x));
                d(descriptionElement.Changed.Subscribe(x => ViewModel.Description = x));

                d(this.WhenAnyValue(x => x.ViewModel.SaveCommand)
                    .ToBarButtonItem(Images.SaveButton, x => NavigationItem.RightBarButtonItem = x));

                d(this.WhenAnyValue(x => x.ViewModel.DismissCommand)
                    .ToBarButtonItem(Images.Cancel, x => NavigationItem.LeftBarButtonItem = x));

                d(this.WhenAnyValue(x => x.ViewModel.Files)
                    .Select(x => x.Changed.Select(_ => Unit.Default).StartWith(Unit.Default))
                    .Switch()
                    .Select(_ => ViewModel.Files.Select(x => new FileElement(x)))
                    .Subscribe(x => _fileSection.Reset(x)));
            });
        }

        private class EditSource : DialogTableViewSource
        {
            private readonly WeakReference<GistModifyViewController<TViewModel>> _parent;
            public EditSource(GistModifyViewController<TViewModel> dvc) 
                : base (dvc.TableView)
            {
                _parent = new WeakReference<GistModifyViewController<TViewModel>>(dvc);
            }

            public override bool CanEditRow(UITableView tableView, Foundation.NSIndexPath indexPath)
            {
                GistModifyViewController<TViewModel> parent;
                if (_parent.TryGetTarget(out parent))
                    return Root.IndexOf(parent._fileSection) == indexPath.Section;
                return false;
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
                Clicked.InvokeCommand(vm.EditCommand);
                ViewModel = vm;
            }
        }
    }
}

