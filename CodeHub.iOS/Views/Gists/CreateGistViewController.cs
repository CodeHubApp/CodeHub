using System;
using CodeFramework.iOS.Views;
using CodeHub.iOS;
using GitHubSharp.Models;
using System.Collections.Generic;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using System.Drawing;
using CodeFramework.ViewControllers;
using CodeFramework.Views;
using CodeFramework.iOS.ViewControllers;
using CodeFramework.iOS.Utils;
using Cirrious.CrossCore;

namespace CodeHub.ViewControllers
{
    public class CreateGistViewController : BaseDialogViewController
    {
        protected GistCreateModel _model;
        protected TrueFalseElement _public;
        public Action<GistModel> Created;
        protected bool _publicEditable = true;

        public CreateGistViewController()
            : base(true)
        {
            Title = "Create Gist";
            Style = UITableViewStyle.Grouped;

            NavigationItem.RightBarButtonItem = new UIBarButtonItem(NavigationButton.Create(Theme.CurrentTheme.SaveButton, Save));

            _model = new GistCreateModel() { Public = true };
            _model.Files = new Dictionary<string, GistCreateModel.File>();
        }

        private void Discard()
        {
            DismissViewController(true, null);
        }

        protected async virtual void Save()
        {
            if (_model.Files.Count == 0)
            {
                MonoTouch.Utilities.ShowAlert("No Files", "You cannot create a Gist without atleast one file");
                return;
            }

            try
            {
                GistModel newGist = null;
                _model.Public = _public.Value;
                await this.DoWorkAsync("Saving...", async () => {
					var app = Mvx.Resolve<CodeHub.Core.Services.IApplicationService>();
					newGist = (await app.Client.ExecuteAsync(app.Client.AuthenticatedUser.Gists.CreateGist(_model))).Data;
                });

                if (Created != null && newGist != null)
                    Created(newGist);

                //Dismiss me!
                NavigationController.PopViewControllerAnimated(true);
            }
            catch (Exception e)
            {
                MonoTouch.Utilities.ShowAlert("Unable to Save!", e.Message);
            }
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
                        if (_model.Files.ContainsKey(name))
                            continue;
                        break;
                    }
                }

                if (_model.Files.ContainsKey(name))
                    throw new InvalidOperationException("A filename by that type already exists");
                _model.Files.Add(name, new GistCreateModel.File { Content = content });
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

            if (_public == null)
                _public = new TrueFalseElement("Public", _model.Public, (e) => { }); 

            if (_publicEditable)
                section.Add(_public);

            var fileSection = new Section();
            root.Add(fileSection);

            foreach (var file in _model.Files.Keys)
            {
                var key = file;
                if (!_model.Files.ContainsKey(key) || _model.Files[file].Content == null)
                    continue;

                var size = System.Text.ASCIIEncoding.UTF8.GetByteCount(_model.Files[file].Content);
                var el = new StyledStringElement(file, size + " bytes", UITableViewCellStyle.Subtitle) { Accessory = UITableViewCellAccessory.DisclosureIndicator };
                el.Tapped += () => {
                    if (!_model.Files.ContainsKey(key))
                        return;
                    var createController = new ModifyGistFileController(key, _model.Files[key].Content);
                    createController.Save = (name, content) => {

                        if (string.IsNullOrEmpty(name))
                            throw new InvalidOperationException("Please enter a name for the file");

                        //If different name & exists somewhere else
                        if (!name.Equals(key) && _model.Files.ContainsKey(name))
                            throw new InvalidOperationException("A filename by that type already exists");

                        //Remove old
                        _model.Files.Remove(key);

                        //Put new
                        _model.Files[name] = new GistCreateModel.File { Content = content };
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

        public override Source CreateSizingSource(bool unevenRows)
        {
            return new EditSource(this);
        }

        private void Delete(Element element)
        {
            _model.Files.Remove(element.Caption);
        }

        private class EditSource : SizingSource
        {
            private readonly CreateGistViewController _parent;
            public EditSource(CreateGistViewController dvc) 
                : base (dvc)
            {
                _parent = dvc;
            }

            public override bool CanEditRow(UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
            {
                return (indexPath.Section == 1 && indexPath.Row != (_parent.Root[1].Count - 1));
            }

            public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
            {
                if (indexPath.Section == 1 && indexPath.Row != (_parent.Root[1].Count - 1))
                    return UITableViewCellEditingStyle.Delete;
                return UITableViewCellEditingStyle.None;
            }

            public override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, MonoTouch.Foundation.NSIndexPath indexPath)
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

