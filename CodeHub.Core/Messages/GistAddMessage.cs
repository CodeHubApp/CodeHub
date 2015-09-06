using System;
using GitHubSharp.Models;
using Cirrious.MvvmCross.Plugins.Messenger;

namespace CodeHub.Core.Messages
{
    public class GistAddMessage : MvxMessage
    {
        public GistModel Gist { get; private set; }

        public GistAddMessage(object sender, GistModel gist) 
            : base(sender)
        {
            Gist = gist;
        }
    }
}

