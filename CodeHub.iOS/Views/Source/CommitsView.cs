using System;
using CodeHub.Core.ViewModels.Changesets;
using ReactiveUI;
using Xamarin.Utilities.ViewControllers;
using Xamarin.Utilities.DialogElements;

namespace CodeHub.iOS.Views.Source
{
    public abstract class CommitsView<TViewModel> : ViewModelCollectionViewController<TViewModel> where TViewModel : CommitsViewModel
	{
        protected CommitsView()
            : base(unevenRows: true)
        {
            Title = "Commits";
        }

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

            this.BindList(ViewModel.Commits, x =>
			{
				var msg = x.Commit.Message ?? string.Empty;
				var firstLine = msg.IndexOf("\n", StringComparison.Ordinal);
				var desc = firstLine > 0 ? msg.Substring(0, firstLine) : msg;

				string login;
				var date = DateTimeOffset.MinValue;
           
                if (x.Commit.Author != null && !string.IsNullOrEmpty(x.Commit.Author.Name))
                    login = x.Commit.Author.Name;
                else if (x.Commit.Committer != null && !string.IsNullOrEmpty(x.Commit.Committer.Name))
                    login = x.Commit.Committer.Name;
                else if (x.Author != null)
                    login = x.Author.Login;
                else if (x.Committer != null)
					login = x.Committer.Login;
				else
					login = "Unknown";

				if (x.Commit.Committer != null)
					date = x.Commit.Committer.Date;

				var el = new NameTimeStringElement { Name = login, Time = date.ToDaysAgo(), String = desc, Lines = 4 };
				el.Tapped += () => ViewModel.GoToChangesetCommand.Execute(x);
				return el;
			});
		}
	}
}

