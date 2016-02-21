using System;
using CodeHub.iOS;
using GitHubSharp.Models;
using System.Collections.Generic;
using UIKit;
using System.Linq;
using CodeHub.iOS.ViewControllers;
using CodeHub.iOS.Utilities;
using CodeHub.iOS.Services;
using CodeHub.iOS.DialogElements;

namespace CodeHub.iOS.Views
{
    public class EditGistController : DialogViewController
    {
        private GistEditModel _model;
        public Action<GistModel> Created;
        private GistModel _originalGist;

        public EditGistController(GistModel gist)
            : base(UITableViewStyle.Grouped, true)
        {
            Title = "Edit Gist";
            _originalGist = gist;

            _model = new GistEditModel();
            _model.Description = gist.Description;
            _model.Files = new Dictionary<string, GistEditModel.File>();

            if (gist.Files != null)
                foreach (var f in gist.Files)
                    _model.Files.Add(f.Key, new GistEditModel.File() { Content = f.Value.Content });
        }

        private void Discard()
        {
            DismissViewController(true, null);
        }

        protected virtual void Save()
        {
            if (_model.Files.Count(x => x.Value != null) == 0)
            {
                AlertDialogService.ShowAlert("No Files", "You cannot modify a Gist without atleast one file");
                return;
            }

            this.DoWorkAsync("Saving...", async () =>
            {
                var app = MvvmCross.Platform.Mvx.Resolve<CodeHub.Core.Services.IApplicationService>();
                var newGist = await app.Client.ExecuteAsync(app.Client.Gists[_originalGist.Id].EditGist(_model));
                if (Created != null)
                    Created(newGist.Data);
                DismissViewController(true, null);
            });
        }

        private bool IsDuplicateName(string name)
        {
            if (_model.Files.Count(x => x.Key.Equals(name) && x.Value != null) > 0)
                return true;
            return _model.Files.Count(x => x.Value != null && name.Equals(x.Value.Filename)) > 0;
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
            var createController = new ModifyGistFileController();
            createController.Save = (name, content) => {
                if (string.IsNullOrEmpty(name))
                    name = GenerateName();

                if (IsDuplicateName(name))
                    throw new InvalidOperationException("A filename by that type already exists");
                _model.Files[name] = new GistEditModel.File { Content = content };
            };
            NavigationController.PushViewController(createController, true);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView.RowHeight = UITableView.AutomaticDimension;
            TableView.EstimatedRowHeight = 44f;

            var cancelButton = new UIBarButtonItem { Image = Theme.CurrentTheme.CancelButton  };
            var saveButton = new UIBarButtonItem { Image = Theme.CurrentTheme.SaveButton  };
            NavigationItem.LeftBarButtonItem = cancelButton;
            NavigationItem.RightBarButtonItem = saveButton;

            OnActivation(d =>
            {
                d(cancelButton.GetClickedObservable().Subscribe(_ => Discard()));
                d(saveButton.GetClickedObservable().Subscribe(_ => Save()));
            });
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

            var desc = new MultilinedElement("Description", _model.Description);
            desc.Clicked.Subscribe(_ => ChangeDescription());
            section.Add(desc);

            var fileSection = new Section();
            sections.Add(fileSection);

            foreach (var file in _model.Files.Keys)
            {
                var key = file;
                if (!_model.Files.ContainsKey(key) || _model.Files[file] == null || _model.Files[file].Content == null)
                    continue;

                var elName = key;
                if (_model.Files[key].Filename != null)
                    elName = _model.Files[key].Filename;

                var el = new FileElement(elName, key, _model.Files[key]);
                el.Clicked.Subscribe(_ => {
                    if (!_model.Files.ContainsKey(key))
                        return;
                    var createController = new ModifyGistFileController(key, _model.Files[key].Content);
                    createController.Save = (name, content) => {

                        if (string.IsNullOrEmpty(name))
                            throw new InvalidOperationException("Please enter a name for the file");

                        //If different name & exists somewhere else
                        if (!name.Equals(key))
                            if (IsDuplicateName(name))
                                throw new InvalidOperationException("A filename by that type already exists");

                        if (_originalGist.Files.ContainsKey(key))
                            _model.Files[key] = new GistEditModel.File { Content = content, Filename = name };
                        else
                        {
                            _model.Files.Remove(key);
                            _model.Files[name] = new GistEditModel.File { Content = content };
                        }
                    };

                    NavigationController.PushViewController(createController, true);
                });
                fileSection.Add(el);
            }

            var addFile = new StringElement("Add New File");
            addFile.Clicked.Subscribe(_ => AddFile());
            fileSection.Add(addFile);

            Root.Reset(sections);
        }

        private void ChangeDescription()
        {
            var composer = new Composer { Title = "Description", Text = _model.Description };
            composer.NewComment(this, (text) => {
                _model.Description = text;
                composer.CloseComposer();
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
            public readonly GistEditModel.File File;
            public readonly string Key;
            public FileElement(string name, string key, GistEditModel.File file)
                : base(name, String.Empty, UITableViewCellStyle.Subtitle)
            {
                File = file;
                Key = key;
                Accessory = UITableViewCellAccessory.DisclosureIndicator;

                if (file.Content != null)
                    Value = System.Text.ASCIIEncoding.UTF8.GetByteCount(file.Content) + " bytes";
            }
        }

        private class EditSource : Source
        {
            public EditSource(EditGistController dvc) 
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
                        (Container as EditGistController)?.Delete(element, section);
                        break;
                }
            }
        }
    }
}

