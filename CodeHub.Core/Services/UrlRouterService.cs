using System;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using CodeHub.Core.ViewModels;

namespace CodeHub.Core.Services
{
    public class UrlRouterService : IUrlRouterService
    {
        private static readonly Route[] Routes = 
        {
            new Route("^gist.github.com/$", typeof(Core.ViewModels.Gists.UserGistsViewModel)),
            new Route("^gist.github.com/(?<Username>[^/]*)/$", typeof(Core.ViewModels.Gists.UserGistsViewModel)),
            new Route("^gist.github.com/(?<Username>[^/]*)/(?<Id>[^/]*)/$", typeof(Core.ViewModels.Gists.GistViewModel)),
            new Route("^[^/]*/stars/$", typeof(Core.ViewModels.Repositories.RepositoriesStarredViewModel)),
            new Route("^[^/]*/(?<Username>[^/]*)/$", typeof(Core.ViewModels.Users.UserViewModel)),
            new Route("^[^/]*/(?<RepositoryOwner>[^/]*)/(?<RepositoryName>[^/]*)/$", typeof(Core.ViewModels.Repositories.RepositoryViewModel)),
            new Route("^[^/]*/(?<RepositoryOwner>[^/]*)/(?<RepositoryName>[^/]*)/pulls/$", typeof(Core.ViewModels.PullRequests.PullRequestsViewModel)),
            new Route("^[^/]*/(?<RepositoryOwner>[^/]*)/(?<RepositoryName>[^/]*)/pull/(?<Id>[^/]*)/$", typeof(Core.ViewModels.PullRequests.PullRequestViewModel)),
            new Route("^[^/]*/(?<RepositoryOwner>[^/]*)/(?<RepositoryName>[^/]*)/issues/$", typeof(Core.ViewModels.Issues.IssuesViewModel)),
            new Route("^[^/]*/(?<RepositoryOwner>[^/]*)/(?<RepositoryName>[^/]*)/commits/$", typeof(Core.ViewModels.Changesets.BaseCommitsViewModel)),
            new Route("^[^/]*/(?<RepositoryOwner>[^/]*)/(?<RepositoryName>[^/]*)/commits/(?<Node>[^/]*)/$", typeof(Core.ViewModels.Changesets.CommitViewModel)),
            new Route("^[^/]*/(?<RepositoryOwner>[^/]*)/(?<RepositoryName>[^/]*)/issues/(?<Id>[^/]*)/$", typeof(Core.ViewModels.Issues.IssueViewModel)),
            new Route("^[^/]*/(?<RepositoryOwner>[^/]*)/(?<RepositoryName>[^/]*)/tree/(?<Branch>[^/]*)/(?<Path>.*)$", typeof(Core.ViewModels.Source.SourceTreeViewModel)),
        };

        private readonly IAccountsService _accountsService;

        public UrlRouterService(IAccountsService accountsService)
        {
            _accountsService = accountsService;
        }

        public IBaseViewModel Handle(string url)
        {
            Uri uri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
                return null;

            if (_accountsService.ActiveAccount == null)
                return null;

            Uri webDomain;
            if (!Uri.TryCreate(_accountsService.ActiveAccount.WebDomain, UriKind.Absolute, out webDomain))
                return null;

            if (uri.Scheme != webDomain.Scheme || uri.Host != webDomain.Host)
                return null;

            var relativePath = string.Concat(uri.Segments);
            if (!relativePath.EndsWith("/", StringComparison.Ordinal))
                relativePath += "/";

            try
            {
                foreach (var route in Routes)
                {
                    var regex = new Regex(route.Path, RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);
                    var match = regex.Match(relativePath);
                    var groups = regex.GetGroupNames().Skip(1).ToList();

                    if (match.Success)
                    {
//                        var vm = Locator.Current.GetService(route.ViewModelType);
////                        foreach (var group in groups)
////                        {
////                            var property = vm.GetType().GetProperty(group);
////                            if (property != null)
////                                property.SetValue(vm, match.Groups[group].Value);
////                        }
////
//                        return vm as IBaseViewModel;
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Unable to resolve Url (" + url + "): " + e.Message);
            }

            return null;
        }


        private class Route
        {
            public string Path { get; set; }
            public Type ViewModelType { get; set; }

            public Route(string path, Type viewModelType) 
            {
                Path = path;
                ViewModelType = viewModelType;
            }
        }
    }
}

