using System;
using System.Reactive;
using System.Threading.Tasks;
using CodeHub.Core.Services;
using CodeHub.iOS.Services;
using CodeHub.iOS.Utilities;
using CodeHub.iOS.ViewControllers;
using Octokit;
using ReactiveUI;
using UIKit;
using Splat;
using System.Reactive.Linq;
using CodeHub.Core.Messages;

namespace CodeHub.iOS.Views.Source
{
    public class EditSourceViewController : TextViewController
    {
        private readonly ReactiveCommand<Unit, Unit> _loadCommand;
        private readonly ReactiveCommand<Unit, Unit> _commitCommand;

        private RepositoryContent _content;
        private RepositoryContent Content
        {
            get => _content;
            set => this.RaiseAndSetIfChanged(ref _content, value);
        }

        private string _commitText;
        private string CommitText
        {
            get => _commitText;
            set => this.RaiseAndSetIfChanged(ref _commitText, value);
        }
    
        public EditSourceViewController(
            string username,
            string repository,
            string path,
            string branch,
            IApplicationService applicationService = null,
            IMessageService messageService = null)
        {
            applicationService = applicationService ?? Locator.Current.GetService<IApplicationService>();
            messageService = messageService ?? Locator.Current.GetService<IMessageService>();

            TextView.Font = UIFont.FromName("Courier", UIFont.PreferredBody.PointSize);
            TextView.SpellCheckingType = UITextSpellCheckingType.No;
            TextView.AutocorrectionType = UITextAutocorrectionType.No;
            TextView.AutocapitalizationType = UITextAutocapitalizationType.None;

            Title = "Edit";

            _loadCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var result = await applicationService.GitHubClient.Repository.Content.GetAllContentsByRef(
                     username, repository, path, branch);

                if (result.Count == 0)
                    throw new Exception("Path contains no files!");

                Content = result[0];
            });

            _commitCommand = ReactiveCommand.CreateFromTask(
                async () =>
                {
                    var updateRequest = new UpdateFileRequest(CommitText, Text, Content.Sha, branch);
                    var result = await applicationService.GitHubClient.Repository.Content.UpdateFile(
                        username, repository, path, updateRequest);

                    messageService.Send(new SourceEditMessage
                    {
                        OldSha = Content.Sha,
                        Data = Text,
                        Update = result
                    });
                },
                this.WhenAnyValue(x => x.Content).Select(x => x != null));

            _commitCommand.IsExecuting.Subscribe(x =>
            {
                if (x)
                    UIApplication.SharedApplication.BeginIgnoringInteractionEvents();
                else
                    UIApplication.SharedApplication.EndIgnoringInteractionEvents();
            });

            Appearing
                .Take(1)
                .Select(_ => Unit.Default)
                .InvokeReactiveCommand(_loadCommand);

            this.WhenAnyValue(x => x.Content)
                .Subscribe(x => Text = x?.Content);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            //var saveButton = NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Save);

            //OnActivation(d =>
            //{
            //    d(saveButton.GetClickedObservable().Subscribe(_ => Commit()));
            //});
        }

        //private void Commit()
        //{
        //    var composer = new Composer
        //    {
        //        Title = "Commit Message",
        //        Text = "Update " + ViewModel.Path.Substring(ViewModel.Path.LastIndexOf('/') + 1)
        //    };

        //    var content = Text;
        //    composer.PresentAsModal(this, text => CommitThis(content, text).ToBackground());
        //}

        //private async Task CommitThis(string content, string message)
        //{
        //    try
        //    {
        //        UIApplication.SharedApplication.BeginIgnoringInteractionEvents();
        //        await this.DoWorkAsync("Commiting...", () => ViewModel.Commit(content, message));
        //        this.PresentingViewController?.DismissViewController(true, null);
        //    }
        //    catch (Exception ex)
        //    {
        //        AlertDialogService.ShowAlert("Error", ex.Message);
        //    }
        //    finally
        //    {
        //        UIApplication.SharedApplication.EndIgnoringInteractionEvents();
        //    }
        //}
    }
}

