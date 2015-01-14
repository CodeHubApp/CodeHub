using System;
using System.Reactive;
using MonoTouch.UIKit;
using CodeHub.Core.ViewModels.Gists;
using ReactiveUI;
using CodeHub.iOS.ViewControllers;
using CodeHub.iOS.ViewComponents;
using System.Collections.Generic;
using CodeHub.iOS.DialogElements;
using CodeHub.iOS.TableViewSources;

namespace CodeHub.iOS.Views.Gists
{
    public class GistCreateView : BaseDialogViewController<GistCreateViewModel>
    {
        private readonly BooleanElement _publicElement;
        private readonly Section _fileSection;
        private readonly MultilinedElement _descriptionElement;

        public GistCreateView()
        {
            HeaderView.Image = Images.LoginUserUnknown;

            this.WhenAnyValue(x => x.ViewModel.SaveCommand).Subscribe(x => 
                NavigationItem.RightBarButtonItem = x != null ? x.ToBarButtonItem(UIBarButtonSystemItem.Save) : null);

            this.WhenAnyValue(x => x.ViewModel.CurrentAccount).Subscribe(x =>
            {
                HeaderView.SubText = x.Username;
                HeaderView.ImageUri = x.AvatarUrl;
            });

            _publicElement = new BooleanElement("Public", false, (e) => ViewModel.IsPublic = e.Value);
            this.WhenAnyValue(x => x.ViewModel.IsPublic).Subscribe(x => _publicElement.Value = x);

            _descriptionElement = new MultilinedElement("Description");
            _descriptionElement.Tapped += ChangeDescription;
            this.WhenAnyValue(x => x.ViewModel.Description).Subscribe(x => _descriptionElement.Value = x);

            _fileSection = new Section(null, new TableFooterButton("Add File", () => ViewModel.AddGistFileCommand.ExecuteIfCan()));
            this.WhenAnyValue(x => x.ViewModel.Files).Subscribe(x =>
            {
                if (x == null)
                {
                    _fileSection.Clear();
                    return;
                }

                var elements = new List<Element>();
                foreach (var file in x.Keys)
                {
                    var key = file;
                    if (string.IsNullOrEmpty(ViewModel.Files[file]))
                        continue;

                    var size = System.Text.Encoding.UTF8.GetByteCount(ViewModel.Files[file]);
                    var el = new StyledStringElement(file, size + " bytes", UITableViewCellStyle.Subtitle) { 
                        Accessory = UITableViewCellAccessory.DisclosureIndicator,
                        Image = Images.FileCode
                    };

                    el.Tapped += () => ViewModel.ModifyGistFileCommand.ExecuteIfCan(key);
                    elements.Add(el);
                }

                _fileSection.Reset(elements);
            });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            ViewModel.WhenAnyValue(x => x.Description, x => x.Files, (x, x1) => Unit.Default)
                .Subscribe(x => Root.Reset(new Section(), new Section { _descriptionElement, _publicElement }, _fileSection));
        }

        private void ChangeDescription()
        {
            var composer = new ComposerViewController() { Title = "Description", Text = ViewModel.Description };
            composer.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(Images.Cancel, UIBarButtonItemStyle.Plain, (s, e) => composer.DismissViewController(true, null));
            composer.NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Save, (s, e) => 
            {
                ViewModel.Description = composer.Text;
                composer.DismissViewController(true, null);
            });

            NavigationController.PresentViewController(new ThemedNavigationController(composer), true, null);
        }

        private void Delete(Element element)
        {
            ViewModel.Files.Remove(element.Caption);
        }

        protected override DialogTableViewSource CreateTableViewSource()
        {
            return new EditSource(this);
        }

        private class EditSource : DialogTableViewSource
        {
            private readonly GistCreateView _parent;
            public EditSource(GistCreateView dvc) 
                : base (dvc.TableView, true)
            {
                _parent = dvc;
            }

            public override bool CanEditRow(UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
            {
                return (indexPath.Section == 1 && indexPath.Row != (Root[1].Count - 1));
            }

            public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
            {
                if (indexPath.Section == 1 && indexPath.Row != (Root[1].Count - 1))
                    return UITableViewCellEditingStyle.Delete;
                return UITableViewCellEditingStyle.None;
            }

            public override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, MonoTouch.Foundation.NSIndexPath indexPath)
            {
                switch (editingStyle)
                {
                    case UITableViewCellEditingStyle.Delete:
                        var section = Root[indexPath.Section];
                        var element = section[indexPath.Row];
                        _parent.Delete(element);
                        section.Remove(element);
                        break;
                }
            }
        }

        public class ComposerViewController : MessageComposerViewController<object>
        {
        }
    }
}

