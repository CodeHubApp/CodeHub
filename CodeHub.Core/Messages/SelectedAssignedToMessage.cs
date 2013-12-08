using System;
using Cirrious.MvvmCross.Plugins.Messenger;
using GitHubSharp.Models;

namespace CodeHub.Core.Messages
{
	public class SelectedAssignedToMessage : MvxMessage
	{
		public SelectedAssignedToMessage(object sender) : base(sender) {}
		public BasicUserModel User;
	}
}

