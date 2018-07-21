using System.Collections.Generic;

namespace CodeHub.iOS.ViewControllers.Issues
{
    public class MyIssuesViewController : SegmentViewController
    {
        private readonly IssueListViewController _created;
        private readonly IssueListViewController _assigned;
        private readonly IssueListViewController _mentioned;

        public MyIssuesViewController()
            : base(new string[] { "Created", "Assigned", "Mentioned" })
        {
            _created = IssueListViewController.MyIssues(
                new Dictionary<string, string> { { "filter", "created" } });
            _assigned = IssueListViewController.MyIssues(
                new Dictionary<string, string> { { "filter", "assigned" } });
            _mentioned = IssueListViewController.MyIssues(
                new Dictionary<string, string> { { "filter", "mentioned" } });
        }

        protected override void SegmentValueChanged(int id)
        {
            if (id == 0)
            {
                AddTable(_created);
                RemoveIfLoaded(_assigned);
                RemoveIfLoaded(_mentioned);
            }
            else if (id == 1)
            {
                AddTable(_assigned);
                RemoveIfLoaded(_created);
                RemoveIfLoaded(_mentioned); 
            }
            else if (id == 2)
            {
                AddTable(_mentioned);
                RemoveIfLoaded(_created);
                RemoveIfLoaded(_assigned);
            }
        }
    }
}
