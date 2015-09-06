using System;
using CodeHub.iOS;
using MonoTouch.Dialog;
using UIKit;
using CodeFramework.iOS.ViewControllers;
using Cirrious.MvvmCross.Touch.Views;
using CodeHub.Core.ViewModels.Gists;
using CodeHub.ViewControllers;
using CodeFramework.iOS.Utils;

namespace CodeHub.iOS.Views.Gists
{
    public class GistCreateView : ViewModelDrivenDialogViewController, IMvxModalTouchView
    {
        private IHud _hud;

        public new GistCreateViewModel ViewModel
        {
            get { return (GistCreateViewModel)base.ViewModel; }
            set { base.ViewModel = value; }
        }

        public override void ViewDidLoad()
        {
            Title = "Create Gist";
            _hud = this.CreateHud();
            base.ViewDidLoad();
            NavigationItem.RightBarButtonItem = new UIBarButtonItem(Theme.CurrentTheme.SaveButton, UIBarButtonItemStyle.Plain, (s, e) => ViewModel.SaveCommand.Execute(null));
            ViewModel.Bind(x => x.Description, UpdateView);
            ViewModel.Bind(x => x.Files, UpdateView);
            ViewModel.Bind(x => x.Public, UpdateView);
            ViewModel.Bind(x => x.IsSaving, x =>
            {
                if (x)
                    _hud.Show("Saving...");
                else
                    _hud.Hide();
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
            var root = new RootElement(Title) { UnevenRows = true };
            var section = new Section();
            root.Add(section);

            var desc = new MultilinedElement("Description") { Value = ViewModel.Description };
            desc.Tapped += ChangeDescription;
            section.Add(desc);

            var pub = new TrueFalseElement("Public", ViewModel.Public, (e) => ViewModel.Public = e.Value); 
            section.Add(pub);

            var fileSection = new Section();
            root.Add(fileSection);

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

            Root = root;
        }

        private void ChangeDescription()
        {
            var composer = new Composer { Title = "Description", Text = ViewModel.Description };
            composer.NewComment(this, (text) => {
                ViewModel.Description = text;
                composer.CloseComposer();
            });
        }

		public override DialogViewController.Source CreateSizingSource(bool unevenRows)
        {
            return new EditSource(this);
        }

        private void Delete(Element element)
        {
            ViewModel.Files.Remove(element.Caption);
        }

        private class EditSource : DialogViewController.SizingSource
        {
            private readonly GistCreateView _parent;
            public EditSource(GistCreateView dvc) 
                : base (dvc)
            {
                _parent = dvc;
            }

            public override bool CanEditRow(UITableView tableView, Foundation.NSIndexPath indexPath)
            {
                return (indexPath.Section == 1 && indexPath.Row != (_parent.Root[1].Count - 1));
            }

            public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, Foundation.NSIndexPath indexPath)
            {
                if (indexPath.Section == 1 && indexPath.Row != (_parent.Root[1].Count - 1))
                    return UITableViewCellEditingStyle.Delete;
                return UITableViewCellEditingStyle.None;
            }

            public override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, Foundation.NSIndexPath indexPath)
            {
                switch (editingStyle)
                {
                    case UITableViewCellEditingStyle.Delete:
                        var section = _parent.Root[indexPath.Section];
                        var element = section[indexPath.Row];
                        _parent.Delete(element);
                        section.Remove(element);
                        break;
                }
            }
        }
    }
}

