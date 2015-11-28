using System;
using CodeHub.Core.Utilities;
using CodeHub.Core.ViewModels.Changesets;
using Splat;
using CodeHub.Core.ViewModels;

namespace CodeHub.Core.Services
{
    public class PushNotificationService : IPushNotificationService, IEnableLogger
    {
        private readonly IServiceConstructor _serviceConstructor;

        public PushNotificationService(IServiceConstructor serviceConstructor)
        {
            _serviceConstructor = serviceConstructor;
        }

        public PushNotificationAction Handle(PushNotificationRequest command)
        {
            try
            {
                var data = command.Attributes;
                var username = data["u"];
                var repoId = new RepositoryIdentifier(data["r"]);
                BaseViewModel baseViewModel;

                if (data.ContainsKey("c"))
                {
                    baseViewModel = _serviceConstructor
                        .Construct<CommitViewModel>()
                        .Init(repoId.Owner, repoId.Name, data["c"]);
                }
                else if (data.ContainsKey("i"))
                {
                    baseViewModel = _serviceConstructor
                        .Construct<CodeHub.Core.ViewModels.Issues.IssueViewModel>()
                        .Init(repoId.Owner, repoId.Name, int.Parse(data["i"]));
                }
                else if (data.ContainsKey("p"))
                {
                    baseViewModel = _serviceConstructor
                        .Construct<CodeHub.Core.ViewModels.PullRequests.PullRequestViewModel>()
                        .Init(repoId.Owner, repoId.Name, int.Parse(data["p"]));
                }
                else
                {
                    baseViewModel = _serviceConstructor
                        .Construct<CodeHub.Core.ViewModels.Repositories.RepositoryViewModel>()
                        .Init(repoId.Owner, repoId.Name);
                }

                return new PushNotificationAction(username, baseViewModel);
            }
            catch (Exception e)
            {
                this.Log().ErrorException("Unable to handle push notification", e);
                return null;
            }
        }
    }
}

