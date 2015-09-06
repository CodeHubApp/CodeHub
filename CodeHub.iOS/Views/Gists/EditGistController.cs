using System;
using CodeFramework.iOS.Views;
using CodeHub.iOS;
using GitHubSharp.Models;
using System.Collections.Generic;
using MonoTouch.Dialog;
using UIKit;
using CoreGraphics;
using System.Linq;
using CodeFramework.ViewControllers;
using CodeFramework.Views;
using CodeFramework.iOS.Utils;
using CodeFramework.iOS.ViewControllers;

namespace CodeHub.ViewControllers
{
    public class EditGistController : BaseDialogViewController
    {
        private GistEditModel _model;
        public Action<GistModel> Created;
        private GistModel _originalGist;

        public EditGistController(GistModel gist)
            : base(true)
        {
            Title = "Edit Gist";
            Style = UITableViewStyle.Grouped;
            _originalGist = gist;

			NavigationItem.LeftBarButtonItem = new UIBarButtonItem (Theme.CurrentTheme.CancelButton, UIBarButtonItemStyle.Plain, (s, e) => Discard());
			NavigationItem.RightBarButtonItem = new UIBarButtonItem(Theme.CurrentTheme.SaveButton, UIBarButtonItemStyle.Plain, (s, e) => Save());

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
                MonoTouch.Utilities.ShowAlert("No Files", "You cannot modify a Gist without atleast one file");
                return;
            }

			this.DoWorkAsync("Saving...", async () =>
			{
				var app = Cirrious.CrossCore.Mvx.Resolve<CodeHub.Core.Services.IApplicationService>();
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

            var desc = new MultilinedElement("Description") { Value = _model.Description };
            desc.Tapped += ChangeDescription;
            section.Add(desc);

            var fileSection = new Section();
            root.Add(fileSection);

            foreach (var file in _model.Files.Keys)
            {
                var key = file;
                if (!_model.Files.ContainsKey(key) || _model.Files[file] == null || _model.Files[file].Content == null)
                    continue;

                var elName = key;
                if (_model.Files[key].Filename != null)
                    elName = _model.Files[key].Filename;

                var el = new FileElement(elName, key, _model.Files[key]);
                el.Tapped += () => {
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
                };
                fileSection.Add(el);
            }

            fileSection.Add(new StyledStringElement("Add New File", AddFile));

            Root = root;
        }

        private void ChangeDescription()
        {
            var composer = new Composer { Title = "Description", Text = _model.Description };
            composer.NewComment(this, (text) => {
                _model.Description = text;
                composer.CloseComposer();
            });
        }

		public override DialogViewController.Source CreateSizingSource(bool unevenRows)
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

        private class FileElement : StyledStringElement
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

        private class EditSource : SizingSource
        {
            private readonly EditGistController _parent;
            public EditSource(EditGistController dvc) 
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
                        _parent.Delete(element, section);
                        break;
                }
            }
        }
    }
}

