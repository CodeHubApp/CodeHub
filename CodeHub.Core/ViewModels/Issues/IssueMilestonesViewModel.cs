using System.Threading.Tasks;
using CodeFramework.Core.ViewModels;
using GitHubSharp.Models;
using CodeHub.Core.Messages;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueMilestonesViewModel : LoadableViewModel
    {
        private readonly CollectionViewModel<MilestoneModel> _milestones = new CollectionViewModel<MilestoneModel>();
		private MilestoneModel _selectedMilestone;

		public MilestoneModel SelectedMilestone
		{
			get
			{
				return _selectedMilestone;
			}
			set
			{
				_selectedMilestone = value;
				RaisePropertyChanged(() => SelectedMilestone);
			}
		}

        public CollectionViewModel<MilestoneModel> Milestones
        {
            get { return _milestones; }
        }

        public string Username
        {
            get;
            private set;
        }

        public string Repository
        {
            get;
            private set;
        }

        public void Init(NavObject navObject)
        {
            Username = navObject.Username;
            Repository = navObject.Repository;
			SelectedMilestone = TxSevice.Get() as MilestoneModel;
			this.Bind(x => x.SelectedMilestone, x => {
				Messenger.Publish(new SelectedMilestoneMessage(this) { Milestone = x });
				ChangePresentation(new Cirrious.MvvmCross.ViewModels.MvxClosePresentationHint(this));
			});
        }

        protected override Task Load(bool forceCacheInvalidation)
        {
			return Milestones.SimpleCollectionLoad(this.GetApplication().Client.Users[Username].Repositories[Repository].Milestones.GetAll(), forceCacheInvalidation);
        }

        public class NavObject
        {
            public string Username { get; set; }
            public string Repository { get; set; }
        }
    }
}

