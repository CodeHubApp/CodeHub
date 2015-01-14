using System;
using CodeHub.Core.ViewModels.Gists;
using GitHubSharp.Models;
using MonoTouch.UIKit;
using ReactiveUI;
using CodeHub.iOS.DialogElements;
using CodeHub.iOS.TableViewSources;

namespace CodeHub.iOS.Views.Gists
{
    public class EditGistController : BaseDialogViewController<GistEditViewModel>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            NavigationItem.LeftBarButtonItem = new UIBarButtonItem(Images.Cancel,
                UIBarButtonItemStyle.Plain, (s, e) =>
                    ViewModel.DismissCommand.ExecuteIfCan());
            NavigationItem.LeftBarButtonItem.EnableIfExecutable(ViewModel.DismissCommand.CanExecuteObservable);

            NavigationItem.RightBarButtonItem = new UIBarButtonItem(Images.SaveButton,
                UIBarButtonItemStyle.Plain, (s, e) =>
                    ViewModel.SaveCommand.ExecuteIfCan());
            NavigationItem.LeftBarButtonItem.EnableIfExecutable(ViewModel.SaveCommand.CanExecuteObservable);
        }


        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            //UpdateView();
        }

//        protected void UpdateView()
//        {
//            var root = new RootElement(Title) { UnevenRows = true };
//            var section = new Section();
//            root.Add(section);
//
//            var desc = new MultilinedElement("Description") { Value = _model.Description };
//            desc.Tapped += () => ViewModel.GoToDescriptionCommand.ExecuteIfCan();
//            section.Add(desc);
//
//            var fileSection = new Section();
//            root.Add(fileSection);
//
//            foreach (var file in _model.Files.Keys)
//            {
//                var key = file;
//                if (!_model.Files.ContainsKey(key) || _model.Files[file] == null || _model.Files[file].Content == null)
//                    continue;
//
//                var elName = key;
//                if (_model.Files[key].Filename != null)
//                    elName = _model.Files[key].Filename;
//
//                var el = new FileElement(elName, key, _model.Files[key]);
//                el.Tapped += () => {
//                    if (!_model.Files.ContainsKey(key))
//                        return;
//                    var createController = new ModifyGistFileController(key, _model.Files[key].Content);
//                    createController.Save = (name, content) => {
//
//                        if (string.IsNullOrEmpty(name))
//                            throw new InvalidOperationException("Please enter a name for the file");
//
//                        //If different name & exists somewhere else
//                        if (!name.Equals(key))
//                            if (IsDuplicateName(name))
//                                throw new InvalidOperationException("A filename by that type already exists");
//
//                        if (_originalGist.Files.ContainsKey(key))
//                            _model.Files[key] = new GistEditModel.File { Content = content, Filename = name };
//                        else
//                        {
//                            _model.Files.Remove(key);
//                            _model.Files[name] = new GistEditModel.File { Content = content };
//                        }
//                    };
//
//                    NavigationController.PushViewController(createController, true);
//                };
//                fileSection.Add(el);
//            }
//
//            fileSection.Add(new StyledStringElement("Add New File", AddFile));
//
//            Root = root;
//        }

        private void Delete(Element element, Section section)
        {
            var fileEl = element as FileElement;
            if (fileEl == null)
                return;

//            var key = fileEl.Key;
//            if (_originalGist.Files.ContainsKey(key))
//                _model.Files[key] = null;
//            else
//                _model.Files.Remove(key);

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

        private class EditSource : DialogTableViewSource
        {
            private readonly EditGistController _parent;
            public EditSource(EditGistController dvc) 
                : base (dvc.TableView)
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
                        _parent.Delete(element, section);
                        break;
                }
            }
        }
    }
}

