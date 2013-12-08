using System;
using Cirrious.MvvmCross.Plugins.Messenger;
using GitHubSharp.Models;

namespace CodeHub.Core.Messages
{
	public class SelectedMilestoneMessage : MvxMessage
	{
		public SelectedMilestoneMessage(object sender) : base(sender) {}
		public MilestoneModel Milestone;
	}
}

