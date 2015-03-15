using System;
using ReactiveUI;
using GitHubSharp.Models;
using System.Linq;

namespace CodeHub.Core.ViewModels.Source
{
    public class CommitedFileItemViewModel : ReactiveObject, ICanGoToViewModel
    {
        public string Name { get; private set; }

        public string RootPath { get; private set; }

        public string Ref { get; private set; }

        public IReactiveCommand<object> GoToCommand { get; private set; }

        internal CommitedFileItemViewModel(CommitModel.CommitFileModel file, Action<CommitedFileItemViewModel> gotoAction)
        {
            Name = System.IO.Path.GetFileName(file.Filename);
            RootPath = file.Filename.Substring(0, file.Filename.Length - Name.Length);
            GoToCommand = ReactiveCommand.Create().WithSubscription(_ => gotoAction(this));

            try
            {
                var queryString = file.ContentsUrl.Substring(file.ContentsUrl.IndexOf('?') + 1);
                var args = queryString.Split('#')
                    .Select(x => x.Split('='))
                    .ToDictionary(x => x[0].ToLower(), x => x[1]);
                Ref = args["ref"];
            }
            catch
            {
            }
        }
    }
}

