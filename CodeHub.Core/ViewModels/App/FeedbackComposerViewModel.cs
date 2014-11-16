using ReactiveUI;
using Xamarin.Utilities.Core.ViewModels;
using System.Reactive;
using CodeHub.Core.Services;
using System.Reactive.Subjects;
using GitHubSharp.Models;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.IO;
using Xamarin.Utilities.Core.Services;
using CodeHub.Core.Extensions;
using Splat;
using System.Threading.Tasks;

namespace CodeHub.Core.ViewModels.App
{
    public class FeedbackComposerViewModel : BaseViewModel
    {
        private const string CodeHubOwner = "thedillonb";
        private const string CodeHubName = "TestTestTest";
        private readonly ISubject<IssueModel> _createdIssueSubject = new Subject<IssueModel>();

        public IObservable<IssueModel> CreatedIssueObservable
        {
            get { return _createdIssueSubject; }
        }

        private string _subject;
        public string Subject
        {
            get { return _subject; }
            set { this.RaiseAndSetIfChanged(ref _subject, value); }
        }

        private string _description;
        public string Description
        {
            get { return _description; }
            set { this.RaiseAndSetIfChanged(ref _description, value); }
        }

        private bool _isFeature;
        public bool IsFeature
        {
            get { return _isFeature; }
            set { this.RaiseAndSetIfChanged(ref _isFeature, value); }
        }

        public IReactiveCommand<Unit> SubmitCommand { get; private set; }

        public IReactiveCommand<string> PostToImgurCommand { get; private set; }

        public FeedbackComposerViewModel(IApplicationService applicationService, IImgurService imgurService, 
            IMediaPickerService mediaPicker, IStatusIndicatorService statusIndicatorService, IAlertDialogService alertDialogService)
        {
            this.WhenAnyValue(x => x.IsFeature).Subscribe(x => Title = x ? "New Feature" : "Bug Report");

            SubmitCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.Subject).Select(x => !string.IsNullOrEmpty(x)),
                async _ =>
            {
                if (string.IsNullOrEmpty(Subject))
                    throw new ArgumentException(string.Format("You must provide a title for this {0}!", IsFeature ? "feature" : "bug"));
                var labels = await applicationService.Client.ExecuteAsync(applicationService.Client.Users[CodeHubOwner].Repositories[CodeHubName].Labels.GetAll());
                var createLabels = labels.Data.Where(x => string.Equals(x.Name, IsFeature ? "feature request" : "bug", StringComparison.OrdinalIgnoreCase)).Select(x => x.Name).Distinct();
                var request = applicationService.Client.Users[CodeHubOwner].Repositories[CodeHubName].Issues.Create(Subject, Description, null, null, createLabels.ToArray());
                var createdIssue = await applicationService.Client.ExecuteAsync(request);
                _createdIssueSubject.OnNext(createdIssue.Data);
                DismissCommand.ExecuteIfCan();
            });

            PostToImgurCommand = ReactiveCommand.CreateAsyncTask(async _ =>
            {
                var photo = await mediaPicker.PickPhoto();
                var memoryStream = new MemoryStream();
                await photo.Save(Splat.CompressedBitmapFormat.Jpeg, 0.8f, memoryStream);
                using (statusIndicatorService.Activate("Uploading..."))
                {
                    var model = await imgurService.SendImage(memoryStream.ToArray());
                    if (model == null || model.Data == null || model.Data.Link == null)
                        throw new InvalidOperationException("Unable to upload to Imgur. Please try again later.");
                    return model.Data.Link;
                }
            });

            PostToImgurCommand.ThrownExceptions
                .Where(x => !(x is TaskCanceledException))
                .Subscribe(x => alertDialogService.Alert("Upload Error", x.Message));
        }
    }
}

