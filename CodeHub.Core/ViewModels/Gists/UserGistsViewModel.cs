using System.Collections.Generic;
using GitHubSharp;
using GitHubSharp.Models;
using Cirrious.MvvmCross.Plugins.Messenger;
using CodeHub.Core.Messages;
using Cirrious.MvvmCross.ViewModels;
using System.Windows.Input;

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

        public string Title
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

        public ICommand GoToCreateGistCommand
        {
            get { return new MvxCommand(() => ShowViewModel<GistCreateViewModel>()); }
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
