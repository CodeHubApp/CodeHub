using System;
using CodeHub.iOS;
using UIKit;
using CodeHub.iOS.ViewControllers;
using CodeHub.Core.ViewModels.Gists;
using CodeHub.iOS.Utilities;
using CodeHub.iOS.DialogElements;
using System.Collections.Generic;
using CodeHub.iOS.ViewControllers.Gists;

namespace CodeHub.iOS.Views.Gists
{
    public class GistCreateView : DialogViewController
    {
        public GistCreateViewModel ViewModel { get; }

        public GistCreateView() : base(UITableViewStyle.Grouped)
        {
            Title = "Create Gist";
            ViewModel = new GistCreateViewModel();
        }

        public static GistCreateView Show(UIViewController parent)
        {
            var ctrl = new GistCreateView();
            var weakVm = new WeakReference<GistCreateViewModel>(ctrl.ViewModel);
            ctrl.ViewModel.SaveCommand.Subscribe(_ => parent.DismissViewController(true, null));
            ctrl.NavigationItem.LeftBarButtonItem = new UIBarButtonItem { Image = Images.Buttons.CancelButton };
            ctrl.NavigationItem.LeftBarButtonItem.GetClickedObservable().Subscribe(_ => {
                weakVm.Get()?.CancelCommand.Execute(null);
                parent.DismissViewController(true, null);
            });
            parent.PresentViewController(new ThemedNavigationController(ctrl), true, null);
            return ctrl;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView.RowHeight = UITableView.AutomaticDimension;
            TableView.EstimatedRowHeight = 44f;

            var saveButton = NavigationItem.RightBarButtonItem = new UIBarButtonItem { Image = Images.Buttons.SaveButton };

            OnActivation(d =>
            {
                d(saveButton.GetClickedObservable().BindCommand(ViewModel.SaveCommand));
                d(ViewModel.Bind(x => x.Description).Subscribe(_ => UpdateView()));
                d(ViewModel.Bind(x => x.Files).Subscribe(_ => UpdateView()));
                d(ViewModel.Bind(x => x.Public).Subscribe(_ => UpdateView()));
                d(ViewModel.Bind(x => x.IsSaving).SubscribeStatus("Saving..."));
            });
        }

        int _gistFileCounter = 0;
        private void AddFile()
        {
            var createController = new GistFileAddViewController();
            createController.SaveCommand.Subscribe(_ => NavigationController.PopToViewController(this, true));
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
            ICollection<Section> sections = new LinkedList<Section>();
            var section = new Section();
            sections.Add(section);

            var desc = new MultilinedElement("Description", ViewModel.Description);
            desc.Clicked.Subscribe(_ => ChangeDescription());
            section.Add(desc);

            var pub = new BooleanElement("Public", ViewModel.Public); 
            pub.Changed.Subscribe(x => ViewModel.Public = x);
            section.Add(pub);

            var fileSection = new Section();
            sections.Add(fileSection);

            foreach (var file in ViewModel.Files.Keys)
            {
                var key = file;
                if (string.IsNullOrEmpty(ViewModel.Files[file]))
                    continue;

                var size = System.Text.Encoding.UTF8.GetByteCount(ViewModel.Files[file]);
                var el = new StringElement(file, size + " bytes", UITableViewCellStyle.Subtitle) { Accessory = UITableViewCellAccessory.DisclosureIndicator };
                el.Clicked.Subscribe(_ => {
                    if (!ViewModel.Files.ContainsKey(key))
                        return;
                    var createController = new GistFileEditViewController { Filename = key, Content = ViewModel.Files[key] };
                    createController.SaveCommand.Subscribe(__ => NavigationController.PopToViewController(this, true));
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
                });
                fileSection.Add(el);
            }

            var add = new StringElement("Add New File");
            add.Clicked.Subscribe(_ => AddFile());
            fileSection.Add(add);

            Root.Reset(sections);
        }

        private void ChangeDescription()
        {
            var composer = new Composer { Title = "Description", Text = ViewModel.Description };
            composer.NewComment(this, (text) => {
                ViewModel.Description = text;
                composer.CloseComposer();
            });
        }

        public override DialogViewController.Source CreateSizingSource()
        {
            return new EditSource(this);
        }

        private void Delete(Element element)
        {
            var e = element as StringElement;
            if (e != null)
                ViewModel.Files.Remove(e.Caption);
        }

        private class EditSource : Source
        {
            public EditSource(GistCreateView dvc) 
                : base (dvc)
            {
            }

            public override bool CanEditRow(UITableView tableView, Foundation.NSIndexPath indexPath)
            {
                return (indexPath.Section == 1 && indexPath.Row != ((Root?[1].Count ?? 0) - 1));
            }

            public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, Foundation.NSIndexPath indexPath)
            {
                if (indexPath.Section == 1 && indexPath.Row != ((Root?[1].Count ?? 0) - 1))
                    return UITableViewCellEditingStyle.Delete;
                return UITableViewCellEditingStyle.None;
            }

            public override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, Foundation.NSIndexPath indexPath)
            {
                switch (editingStyle)
                {
                    case UITableViewCellEditingStyle.Delete:
                        var section = Root?[indexPath.Section];
                        var element = section?[indexPath.Row];
                        (Container as GistCreateView)?.Delete(element);
                        section?.Remove(element);
                        break;
                }
            }
        }
    }
}

