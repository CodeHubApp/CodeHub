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

            this.WhenActivated(d =>
            {
                d(SearchTextChanging.Subscribe(x => ViewModel.SearchKeyword = x));
            });
        }

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

            this.BindList(ViewModel.Commits, x =>
			{
                var el = new NameTimeStringElement { Name = x.Commiter, Time = x.CommitDate.ToDaysAgo(), String = x.Message, Lines = 4 };
				el.Tapped += () => ViewModel.GoToChangesetCommand.Execute(x);
				return el;
			});
		}
	}
}

