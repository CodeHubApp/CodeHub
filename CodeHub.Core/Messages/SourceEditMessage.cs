using System;
using Cirrious.MvvmCross.Plugins.Messenger;
using GitHubSharp.Models;

namespace CodeHub.Core.Messages
{
	public class SourceEditMessage : MvxMessage
    {
		public SourceEditMessage(object sender) : base(sender) {}

		public string OldSha;
		public string Data;
		public ContentUpdateModel Update;
    }
}

