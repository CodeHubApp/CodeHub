using System;
using System.Collections.Generic;
using UIKit;
using System.Linq;
using CodeHub.iOS.Utilities;
using CodeHub.iOS.Services;
using CodeHub.iOS.DialogElements;
using System.Threading.Tasks;
using CodeHub.iOS.ViewControllers.Gists;
using Octokit;

namespace CodeHub.iOS.ViewControllers
{
    public class GistEditViewController : DialogViewController
    {
        private GistUpdate _model;
        public Action<Gist> Created;
        private Gist _originalGist;

        public GistEditViewController(Gist gist)
            : base(UITableViewStyle.Grouped, true)
        {
            Title = "Edit Gist";
            _originalGist = gist;

            _model = new GistUpdate();
            _model.Description = gist.Description;

            if (gist.Files != null)
                foreach (var f in gist.Files)
                    _model.Files.Add(f.Key, new GistFileUpdate { Content = f.Value.Content });
        }

        private void Discard()
        {
            DismissViewController(true, null);
        }

        private async Task Save()
        {
            if (_model.Files.Count(x => x.Value != null) == 0)
            {
                AlertDialogService.ShowAlert("No Files", "You cannot modify a Gist without atleast one file");
                return;
            }

            var app = MvvmCross.Platform.Mvx.Resolve<Core.Services.IApplicationService>();
            var hud = this.CreateHud();
            NetworkActivity.PushNetworkActive();

            try
            {
                hud.Show("Saving...");
                var newGist = await app.GitHubClient.Gist.Edit(_originalGist.Id, _model);
                Created?.Invoke(newGist);
                DismissViewController(true, null);
            }
            catch (Exception e)
            {
                AlertDialogService.ShowAlert("Error", "Unable to save gist: " + e.Message);
            }
            finally
            {
                hud.Hide();
                NetworkActivity.PopNetworkActive();
            }
        }

        private bool IsDuplicateName(string name)
        {
            if (_model.Files.Count(x => x.Key.Equals(name) && x.Value != null) > 0)
                return true;
            return _model.Files.Count(x => x.Value != null && name.Equals(x.Value.NewFileName)) > 0;
        }

        int _gistFileCounter = 0;
        private string GenerateName()
        {
            var name = string.Empty;
            //Keep trying until we get a valid filename
            while (true)
            {
                name = "gistfile" + (++_gistFileCounter) + ".txt";
                if (IsDuplicateName(name))
                    continue;
                break;
            }
            return name;
        }

        private void AddFile()
        {
            var createController = new GistFileAddViewController();
            createController.SaveCommand.Subscribe(_ => NavigationController.PopToViewController(this, true));
            createController.Save = (name, content) => {
                if (string.IsNullOrEmpty(name))
                    name = GenerateName();

                if (IsDuplicateName(name))
                    throw new InvalidOperationException("A filename by that type already exists");
                _model.Files[name] = new GistFileUpdate { Content = content };
            };
            NavigationController.PushViewController(createController, true);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView.RowHeight = UITableView.AutomaticDimension;
            TableView.EstimatedRowHeight = 44f;

            var cancelButton = new UIBarButtonItem(UIBarButtonSystemItem.Cancel);
            var saveButton = new UIBarButtonItem(UIBarButtonSystemItem.Save);
            NavigationItem.LeftBarButtonItem = cancelButton;
            NavigationItem.RightBarButtonItem = saveButton;

            OnActivation(d =>
            {
                d(cancelButton.GetClickedObservable().Subscribe(_ => Discard()));
                d(saveButton.GetClickedObservable().Subscribe(_ => Save().ToBackground()));
                d(_descriptionElement.Clicked.Subscribe(_ => ChangeDescription()));
                d(_addFileElement.Clicked.Subscribe(_ => AddFile()));
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            UpdateView();
        }

        private readonly MultilinedElement _descriptionElement = new MultilinedElement("Description");
        private readonly StringElement _addFileElement = new StringElement("Add New File");

        protected void UpdateView()
        {
            ICollection<Section> sections = new LinkedList<Section>();
            var section = new Section();
            sections.Add(section);

            _descriptionElement.Details = _model.Description;
            section.Add(_descriptionElement);

            var fileSection = new Section();
            sections.Add(fileSection);

            foreach (var file in _model.Files.Keys)
            {
                var key = file;
                if (!_model.Files.ContainsKey(key) || _model.Files[file] == null || _model.Files[file].Content == null)
                    continue;

                var elName = key;
                if (_model.Files[key].NewFileName != null)
                    elName = _model.Files[key].NewFileName;

                var el = new FileElement(elName, key, _model.Files[key]);
                el.Clicked.Subscribe(MakeCallback(this, key));
                fileSection.Add(el);
            }

            fileSection.Add(_addFileElement);
            Root.Reset(sections);
        }

        private static Action<object> MakeCallback(GistEditViewController ctrl, string key)
        {
            var weakCtrl = new WeakReference<GistEditViewController>(ctrl);
            return new Action<object>(_ =>
            {
                var model = weakCtrl.Get()?._model;
                if (model == null || !model.Files.ContainsKey(key))
                    return;

                var originalGist = weakCtrl.Get()?._originalGist;

                var createController = new GistFileEditViewController { Filename = key, Content = model.Files[key].Content };
                createController.SaveCommand.Subscribe(__ => weakCtrl.Get()?.NavigationController.PopToViewController(weakCtrl.Get(), true));
                createController.Save = (name, content) =>
                {
                    if (string.IsNullOrEmpty(name))
                        throw new InvalidOperationException("Please enter a name for the file");

                    //If different name & exists somewhere else
                    if (!name.Equals(key))
                    if (weakCtrl.Get()?.IsDuplicateName(name) == true)
                        throw new InvalidOperationException("A filename by that type already exists");

                    if (originalGist?.Files.ContainsKey(key) == true)
                        model.Files[key] = new GistFileUpdate { Content = content, NewFileName = name };
                    else
                    {
                        model.Files.Remove(key);
                        model.Files[name] = new GistFileUpdate { Content = content };
                    }
                };

                weakCtrl.Get()?.NavigationController.PushViewController(createController, true);
            });
        }

        private void ChangeDescription()
        {
            var composer = new Composer
            {
                Title = "Description",
                Text = _model.Description
            };

            composer.PresentAsModal(this, text =>
            {
                _model.Description = text;
                this.DismissViewController(true, null);
            });
        }

        public override DialogViewController.Source CreateSizingSource()
        {
            return new EditSource(this);
        }

        private void Delete(Element element, Section section)
        {
            var fileEl = element as FileElement;
            if (fileEl == null)
                return;

            var key = fileEl.Key;
            if (_originalGist.Files.ContainsKey(key))
                _model.Files[key] = null;
            else
                _model.Files.Remove(key);

            section.Remove(element);
        }

        private class FileElement : StringElement
        {
            public readonly GistFileUpdate File;
            public readonly string Key;
            public FileElement(string name, string key, GistFileUpdate file)
                : base(name, String.Empty, UITableViewCellStyle.Subtitle)
            {
                File = file;
                Key = key;
                if (file.Content != null)
                    Value = System.Text.Encoding.UTF8.GetByteCount(file.Content) + " bytes";
            }
        }

        private class EditSource : Source
        {
            public EditSource(GistEditViewController dvc) 
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
                        (Container as GistEditViewController)?.Delete(element, section);
                        break;
                }
            }
        }
    }
}

