using System;
using System.Collections.Generic;
using System.Linq;
using CodeFramework.iOS.ViewControllers;
using CodeFramework.iOS.Views;
using CodeFramework.ViewControllers;
using CodeHub.ViewControllers;
using GitHubSharp.Models;
using MonoTouch;
using MonoTouch.Dialog;
using MonoTouch.UIKit;

namespace CodeHub.iOS.Views.Issues
{
    public class IssueEditView : BaseDialogViewController
    {
        private const string Unassigned = "Unassigned";
        private const string None = "None";

        private MilestoneModel _selectedMilestone;
        private BasicUserModel _selectedAssignee;
        private List<LabelModel> _selectedLabels;

        public string Username { get; private set; }
        public string RepoSlug { get; private set; }
        public IssueModel ExistingIssue { get; set; }

        public List<MilestoneModel> Milestones { get; set; }
        public List<LabelModel> Labels { get; set; }

        public Action<IssueModel> Success;

        EntryElement _title;
        StyledStringElement _assignedTo, _milestone, _labels;
        MultilinedElement _content;
        TrueFalseElement _state;

        public IssueEditView(string username, string repoSlug)
            : base(true)
        {
            Title = "New Issue";
            NavigationItem.RightBarButtonItem = new UIBarButtonItem(NavigationButton.Create(Theme.CurrentTheme.SaveButton, SaveIssue));
            Username = username;
            RepoSlug = repoSlug;
            _selectedLabels = new List<LabelModel>();
        }

        private async void SaveIssue()
        {
            try
            {
                //Stop any editing!
                View.EndEditing(true);

                //Check the required fields
                if (string.IsNullOrEmpty(_title.Value))
                {
                    Utilities.ShowAlert("Missing field!", "You must enter a title for this issue.");
                    return;
                }

                IssueModel model = null;
                NavigationItem.RightBarButtonItem.Enabled = false;

                var title = _title.Value;
                var content = _content.Value;
                string assignedTo = null;
                if (_selectedAssignee != null)
                    assignedTo = _selectedAssignee.Login;

                var state = _state.Value ? "open" : "closed";
                uint? milestone = null;
                if (_selectedMilestone != null)
                    milestone = _selectedMilestone.Number;
                string[] labels = null;
                    if (_selectedLabels.Count > 0)
                        labels = _selectedLabels.Select(x => x.Name).ToArray();

                //New
                /*
                await this.DoWorkTest("Saving...", async () => {
                    var tryEditAgain = false;

                    try
                    {
                        var response = (ExistingIssue == null) ? 
                            await Application.Client.ExecuteAsync(Application.Client.Users[Username].Repositories[RepoSlug].Issues.Create(title, content, assignedTo, milestone, labels)) :
                            await Application.Client.ExecuteAsync(Application.Client.Users[Username].Repositories[RepoSlug].Issues[ExistingIssue.Number].Update(title, content, state, assignedTo, milestone, labels)); 
                        model = response.Data;
                    }
                    //There is a wierd bug in GitHub when editing an existing issue and the assignedTo is null
                    catch (GitHubSharp.InternalServerException)
                    {
                        if (ExistingIssue != null && assignedTo == null)
                            tryEditAgain = true;
                        else
                            throw;
                    }

                    if (tryEditAgain)
                    {
                        var response = await Application.Client.ExecuteAsync(Application.Client.Users[Username].Repositories[RepoSlug].Issues[ExistingIssue.Number].Update(title, content, state, assignedTo, milestone, labels)); 
                        model = response.Data;
                    }
                });
                */

                if (Success != null)
                    Success(model);

                if (NavigationController != null)
                    NavigationController.PopViewControllerAnimated(true);
            }
            catch (Exception e)
            {
                Utilities.ShowAlert("Unable to save issue!", e.Message);
            }
            finally
            {
                NavigationItem.RightBarButtonItem.Enabled = true;
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            PopulateRoot();
        }

        private void PopulateRoot()
        {
            _title = new InputElement("Title", string.Empty, string.Empty);

            _assignedTo = new StyledStringElement("Responsible", Unassigned, UITableViewCellStyle.Value1);
            _assignedTo.Accessory = UITableViewCellAccessory.DisclosureIndicator;
            _assignedTo.Tapped += () => {
//                var p = new IssueAssigneesView(Username, RepoSlug);
//                p.SelectedUser = (x) => {
//                    _selectedAssignee = x;
//                    NavigationController.PopViewControllerAnimated(true);
//                };
//                NavigationController.PushViewController(p, true);
            };

            _milestone = new StyledStringElement("Milestone".t(), None, UITableViewCellStyle.Value1);
            _milestone.Accessory = UITableViewCellAccessory.DisclosureIndicator;
            _milestone.Tapped += () => {
                var p = new IssueMilestonesView();
                p.MilestoneSelected = (x) => {
                    _selectedMilestone = x;
                    NavigationController.PopViewControllerAnimated(true);
                };
                NavigationController.PushViewController(p, true);
            };

            _labels = new StyledStringElement("Labels".t(), None, UITableViewCellStyle.Value1);
            _labels.Accessory = UITableViewCellAccessory.DisclosureIndicator;
            _labels.Tapped += () => {
//                var p = new IssueLabelsView(Username, RepoSlug) { SelectedLabels = _selectedLabels };
//                NavigationController.PushViewController(p, true);
            };

            _state = new TrueFalseElement("Open", true);
           
            _content = new MultilinedElement("Description");
            _content.Tapped += () =>
            {
                var composer = new Composer { Title = "Issue Description", Text = _content.Value, ActionButtonText = "Save" };
                composer.NewComment(this, (text) => {
                    _content.Value = text;
                    composer.CloseComposer();
                });
            };

            var root = new RootElement(Title) { new Section { _title, _assignedTo, _milestone, _labels }, new Section { _content } };

            //See if it's an existing issue or not...
            if (ExistingIssue != null)
            {
                _title.Value = ExistingIssue.Title;
                _selectedAssignee = ExistingIssue.Assignee;
                _selectedMilestone = ExistingIssue.Milestone;
                _selectedLabels = ExistingIssue.Labels;
                _content.Value = ExistingIssue.Body;
                _state.Value = ExistingIssue.State.Equals("open");

                //Insert the status thing inbetween title and assigned to elements
                root.Insert(1, new Section() { _state });
            }

            Root = root;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            //Update the elements
            _assignedTo.Value = _selectedAssignee == null ? Unassigned : _selectedAssignee.Login;
            _milestone.Value = _selectedMilestone == null ? None : _selectedMilestone.Title;

            if (_selectedLabels != null && _selectedLabels.Count > 0)
            {
                var sb = new System.Text.StringBuilder();
                foreach (var s in _selectedLabels)
                    sb.Append(s.Name + ", ");
                _labels.Value = sb.ToString().TrimEnd(',', ' ');
            }
            else
            {
                _labels.Value = None;
            }

            //Reload the table
            TableView.ReloadData();
        }
    }
}

