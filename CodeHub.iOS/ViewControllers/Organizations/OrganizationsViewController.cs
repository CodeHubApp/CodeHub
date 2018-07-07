using System;
using System.Reactive.Linq;
using CodeHub.Core.Services;
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
            var e = new ProfileElement(
                item.Login,
                item.Name,
                new Core.Utilities.GitHubAvatar(item.AvatarUrl));
            
            e.Clicked
             .Select(_ => new OrganizationViewController(item))
             .Subscribe(this.PushViewController);
            
            return e;
        }
    }
}

