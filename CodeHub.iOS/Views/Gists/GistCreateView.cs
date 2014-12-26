using System;
using System.Reactive;
using MonoTouch.UIKit;
using CodeHub.Core.ViewModels.Gists;
using ReactiveUI;
using Xamarin.Utilities.ViewControllers;
using Xamarin.Utilities.DialogElements;
using Xamarin.Utilities.Delegates;

namespace CodeHub.iOS.Views.Gists
{
    public class GistCreateView : ReactiveTableViewController<GistCreateViewModel>
    {
        private EditSource _source;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _source = new EditSource(this);

            NavigationItem.RightBarButtonItem = new UIBarButtonItem(Images.SaveButton, UIBarButtonItemStyle.Plain, (s, e) => ViewModel.SaveCommand.Execute(null));
            NavigationItem.RightBarButtonItem.EnableIfExecutable(ViewModel.SaveCommand.CanExecuteObservable);

            ViewModel.WhenAnyValue(x => x.Description, x => x.Files, x => x.IsPublic, (x, x1, x2) => Unit.Default)
                .Subscribe(x => UpdateView());

            ViewModel.SaveCommand.IsExecuting.Subscribe( x =>
            {
//                if (x)
//                    _hud.Show("Saving...");
//                else
//                    _hud.Hide();
            });
        }

        int _gistFileCounter = 0;
        private void AddFile()
        {
            var createController = new ModifyGistFileController();
            createController.Save = (name, content) => {
                if (string.IsNullOrEmpty(name))
                {
                    //Keep trying until we get a valid filename
                    while (true)
                    {
                        name = "gistfile" + (++_gistFileCounter) + ".txt";
                        if (ViewModel.Files.ContainsKey(name))
                            continue;
                        break;
                    }
                }

                if (ViewModel.Files.ContainsKey(name))
                    throw new InvalidOperationException("A filename by that type already exists");
                ViewModel.Files.Add(name, content);
                ViewModel.Files = ViewModel.Files;
            };

            NavigationController.PushViewController(createController, true);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            UpdateView();
        }

        protected void UpdateView()
        {
            var section = new Section();
            var fileSection = new Section();

            var desc = new MultilinedElement("Description") { Value = ViewModel.Description };
            desc.Tapped += ChangeDescription;
            section.Add(desc);

            var pub = new BooleanElement("Public", ViewModel.IsPublic, (e) => ViewModel.IsPublic = e.Value); 
            section.Add(pub);

            foreach (var file in ViewModel.Files.Keys)
            {
                var key = file;
                if (string.IsNullOrEmpty(ViewModel.Files[file]))
                    continue;

                var size = System.Text.Encoding.UTF8.GetByteCount(ViewModel.Files[file]);
                var el = new StyledStringElement(file, size + " bytes", UITableViewCellStyle.Subtitle) { Accessory = UITableViewCellAccessory.DisclosureIndicator };
                el.Tapped += () => {
                    if (!ViewModel.Files.ContainsKey(key))
                        return;
                    var createController = new ModifyGistFileController(key, ViewModel.Files[key]);
                    createController.Save = (name, content) => {

                        if (string.IsNullOrEmpty(name))
                            throw new InvalidOperationException("Please enter a name for the file");

                        //If different name & exists somewhere else
                        if (!name.Equals(key) && ViewModel.Files.ContainsKey(name))
                            throw new InvalidOperationException("A filename by that type already exists");

                        ViewModel.Files.Remove(key);
                        ViewModel.Files[name] = content;
                        ViewModel.Files = ViewModel.Files; // Trigger refresh
                    };

                    NavigationController.PushViewController(createController, true);
                };
                fileSection.Add(el);
            }

            fileSection.Add(new StyledStringElement("Add New File", AddFile));

            _source.Root.Reset(section, fileSection);
        }

        private void ChangeDescription()
        {
//            var composer = new Composer { Title = "Description", Text = ViewModel.Description };
//            composer.NewComment(this, (text) => {
//                ViewModel.Description = text;
//                composer.CloseComposer();
//            });
        }

        private void Delete(Element element)
        {
            ViewModel.Files.Remove(element.Caption);
        }

        private class EditSource : DialogTableViewSource
        {
            private readonly GistCreateView _parent;
            public EditSource(GistCreateView dvc) 
                : base (dvc.TableView)
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
    }
}

