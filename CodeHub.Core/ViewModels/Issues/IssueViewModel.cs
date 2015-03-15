using CodeHub.Core.Services;
using System;
using ReactiveUI;
using CodeHub.Core.Factories;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueViewModel : BaseIssueViewModel
    {
        private readonly ObservableAsPropertyHelper<Uri> _htmlUrl;
        protected override Uri HtmlUrl
        {
            get { return _htmlUrl.Value; }
        }

        public IssueViewModel(
            ISessionService applicationService, 
            IActionMenuFactory actionMenuFactory,
            IMarkdownService markdownService)
            : base(applicationService, markdownService, actionMenuFactory)
        {
            this.WhenAnyValue(x => x.Id)
                .Subscribe(x => Title = "Issue #" + x);

            _htmlUrl = this.WhenAnyValue(x => x.Issue.HtmlUrl)
                .ToProperty(this, x => x.HtmlUrl);
        }
    }
}

