using System.Collections.Generic;
using GitHubSharp;
using GitHubSharp.Models;
using CodeHub.Core.Messages;
using MvvmCross.Plugins.Messenger;

namespace CodeHub.Core.ViewModels.Gists
{
    public class UserGistsViewModel : GistsViewModel
    {
        private readonly MvxSubscriptionToken _addToken;

        public string Username
        {
            get;
            private set;
        }

        public bool IsMine
        {
            get { return this.GetApplication().Account.Username.Equals(Username); }
        }

        public UserGistsViewModel()
        {
            _addToken = Messenger.SubscribeOnMainThread<GistAddMessage>(x => Gists.Items.Insert(0, x.Gist));
        }

        public void Init(NavObject navObject)
        {
            Username = navObject.Username ?? this.GetApplication().Account.Username;

            //Assign some sort of title
            if (Username != null)
            {
                if (IsMine)
                    Title = "My Gists";
                else
                {
                    if (Username.EndsWith("s", System.StringComparison.Ordinal))
                        Title = Username + "' Gists";
                    else
                        Title = Username + "'s Gists";
                }
            }
            else
            {
                Title = "Gists";
            }
        }

        protected override GitHubRequest<List<GistModel>> CreateRequest()
        {
            return this.GetApplication().Client.Users[Username].Gists.GetGists();
        }

        public class NavObject
        {
            public string Username { get; set; }
        }
    }

}
