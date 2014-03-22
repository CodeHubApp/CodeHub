using System;
using CodeFramework.ViewControllers;
using CodeHub.Core.ViewModels;
using MonoTouch.Dialog;

namespace CodeHub.iOS.Views.Source
{
	public abstract class CommitsView : ViewModelCollectionDrivenDialogViewController
	{
		public override void ViewDidLoad()
		{
			Title = "Commits".t();
			Root.UnevenRows = true;

			base.ViewDidLoad();

			var vm = (CommitsViewModel) ViewModel;
			BindCollection(vm.Commits, x =>
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
				el.Tapped += () => vm.GoToChangesetCommand.Execute(x);
				return el;
			});
		}
	}
}

