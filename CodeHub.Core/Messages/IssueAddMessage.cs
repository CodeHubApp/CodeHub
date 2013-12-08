using System;
using Cirrious.MvvmCross.Plugins.Messenger;
using GitHubSharp.Models;

namespace CodeHub.Core.Messages
{
	public class IssueAddMessage : MvxMessage
	{
		public IssueAddMessage(object sender) : base(sender) {}
		public IssueModel Issue;
	}
}

