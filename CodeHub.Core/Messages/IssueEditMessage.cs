using System;
using Cirrious.MvvmCross.Plugins.Messenger;
using GitHubSharp.Models;

namespace CodeHub.Core.Messages
{
	public class IssueEditMessage : MvxMessage
	{
		public IssueEditMessage(object sender) : base(sender) {}
		public IssueModel Issue;
	}
}

