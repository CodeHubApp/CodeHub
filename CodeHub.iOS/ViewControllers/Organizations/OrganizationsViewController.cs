using System;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using CodeHub.Core.Utilities;
using CodeHub.iOS.DialogElements;
using Octokit;
using Splat;
using UIKit;

namespace CodeHub.iOS.ViewControllers.Organizations
{
    public class OrganizationsViewController : GitHubListViewController<Organization>
    {
        public OrganizationsViewController(
            string username,
            IApplicationService applicationService = null)
            : base(DetermineUri(username, applicationService), Octicon.Organization)
        {
            Title = "Organizations";
        }

        public static Uri DetermineUri(string username, IApplicationService applicationService = null)
        {
            applicationService = applicationService ?? Locator.Current.GetService<IApplicationService>();
            var isCurrent = string.Equals(username, applicationService.Account.Username);
            return isCurrent ? ApiUrls.UserOrganizations() : ApiUrls.UserOrganizations(username);
        }

        protected override Element ConvertToElement(Organization item)
        {
            var avatar = new GitHubAvatar(item.AvatarUrl);
            var e = new UserElement(item.Login, item.Name, avatar);
            e.Clicked
             .Select(_ => new OrganizationViewController(item))
             .Subscribe(this.PushViewController);
            return e;
        }
    }
}

