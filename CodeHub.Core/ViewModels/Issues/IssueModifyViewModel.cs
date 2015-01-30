using System.Threading.Tasks;
using ReactiveUI;
using System.Reactive;
using CodeHub.Core.Factories;

namespace CodeHub.Core.ViewModels.Issues
{
	public abstract class IssueModifyViewModel : BaseViewModel
    {
        private string _subject;
        public string Subject
        {
            get { return _subject; }
            set { this.RaiseAndSetIfChanged(ref _subject, value); }
        }

		private string _content;
		public string Content
		{
			get { return _content; }
            set { this.RaiseAndSetIfChanged(ref _content, value); }
		}

		public string RepositoryOwner { get; set; }

		public string RepositoryName { get; set; }

		public IReactiveCommand<Unit> SaveCommand { get; private set; }

        protected IssueModifyViewModel(IAlertDialogFactory alertDialogFactory)
	    {
            SaveCommand = ReactiveCommand.CreateAsyncTask(async _ => {
                using (alertDialogFactory.Activate("Saving..."))
                    await Save();
                Dismiss();
            });
	    }

		protected abstract Task Save();
    }
}

