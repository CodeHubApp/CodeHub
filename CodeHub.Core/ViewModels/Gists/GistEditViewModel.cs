using ReactiveUI;
using Xamarin.Utilities.ViewModels;

namespace CodeHub.Core.ViewModels.Gists
{
    public class GistEditViewModel : BaseViewModel
    {
        public IReactiveCommand SaveCommand { get; private set; }

        public IReactiveCommand GoToDescriptionCommand { get; private set; }

        public IReactiveCommand<object> DismissCommand { get; private set; }

        public GistEditViewModel()
        {
            Title = "Edit Gist";

            SaveCommand = ReactiveCommand.Create();
            DismissCommand = ReactiveCommand.Create();

            GoToDescriptionCommand = ReactiveCommand.Create();
        }


//        public async Task Edit(GistEditModel editModel)
//        {
//            var response = await _applicationService.Client.ExecuteAsync(_applicationService.Client.Gists[Id].EditGist(editModel));
//            Gist = response.Data;
//        }
//

//        protected virtual void Save()
//        {
//            if (_model.Files.Count(x => x.Value != null) == 0)
//            {
//                MonoTouch.Utilities.ShowAlert("No Files", "You cannot modify a Gist without atleast one file");
//                return;
//            }
//
//            this.DoWorkAsync("Saving...", async () =>
//            {
//                var app = Cirrious.CrossCore.Mvx.Resolve<CodeHub.Core.Services.IApplicationService>();
//                var newGist = await app.Client.ExecuteAsync(app.Client.Gists[_originalGist.Id].EditGist(_model));
//                if (Created != null)
//                    Created(newGist.Data);
//                DismissViewController(true, null);
//            });
//        }
//
//        private bool IsDuplicateName(string name)
//        {
//            if (_model.Files.Count(x => x.Key.Equals(name) && x.Value != null) > 0)
//                return true;
//            return _model.Files.Count(x => x.Value != null && name.Equals(x.Value.Filename)) > 0;
//        }
//
//        int _gistFileCounter = 0;
//        private string GenerateName()
//        {
//            var name = string.Empty;
//            //Keep trying until we get a valid filename
//            while (true)
//            {
//                name = "gistfile" + (++_gistFileCounter) + ".txt";
//                if (IsDuplicateName(name))
//                    continue;
//                break;
//            }
//            return name;
//        }
//
//        private void AddFile()
//        {
//            var createController = new ModifyGistFileController();
//            createController.Save = (name, content) =>
//            {
//                if (string.IsNullOrEmpty(name))
//                    name = GenerateName();
//
//                if (IsDuplicateName(name))
//                    throw new InvalidOperationException("A filename by that type already exists");
//                _model.Files[name] = new GistEditModel.File { Content = content };
//            };
//            NavigationController.PushViewController(createController, true);
//        }
    }
}