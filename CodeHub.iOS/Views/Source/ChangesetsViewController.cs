using System;
using CodeFramework.ViewControllers;
using CodeHub.Core.ViewModels;
using MonoTouch.Dialog;

namespace CodeHub.iOS.Views.Source
{
    public class ChangesetsViewController : ViewModelCollectionDrivenViewController
    {
        public override void ViewDidLoad()
        {
            Title = "Changes".t();
            Root.UnevenRows = true;
            EnableSearch = false;

            base.ViewDidLoad();

            var vm = (ChangesetsViewModel) ViewModel;
            BindCollection(vm.Commits, x =>
            {
                var desc = (x.Commit.Message ?? "").Replace("\n", " ").Trim();
                string login;
                var date = DateTime.MinValue;

                if (x.Committer != null)
                    login = x.Committer.Login;
                else if (x.Author != null)
                    login = x.Author.Login;
                else if (x.Commit.Committer != null)
                    login = x.Commit.Committer.Name;
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

