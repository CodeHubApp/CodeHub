using System;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using CodeHub.Core.Services;

namespace CodeHub.iOS
{
    public static class UrlRouteProvider
    {
        private static readonly Route[] Routes = {
            new Route("^gist.github.com/$", typeof(Core.ViewModels.Gists.UserGistsViewModel)),
            new Route("^gist.github.com/(?<Username>[^/]*)/$", typeof(Core.ViewModels.Gists.UserGistsViewModel)),
            new Route("^gist.github.com/(?<Username>[^/]*)/(?<Id>[^/]*)/$", typeof(Core.ViewModels.Gists.GistViewModel)),
            new Route("^[^/]*/stars/$", typeof(Core.ViewModels.Repositories.RepositoriesStarredViewModel)),
            new Route("^[^/]*/(?<Username>[^/]*)/$", typeof(Core.ViewModels.Users.UserViewModel)),
            new Route("^[^/]*/(?<Username>[^/]*)/(?<Repository>[^/]*)/$", typeof(Core.ViewModels.Repositories.RepositoryViewModel)),
            new Route("^[^/]*/(?<Username>[^/]*)/(?<Repository>[^/]*)/pulls/$", typeof(Core.ViewModels.PullRequests.PullRequestsViewModel)),
            new Route("^[^/]*/(?<Username>[^/]*)/(?<Repository>[^/]*)/pull/(?<Id>[^/]*)/$", typeof(Core.ViewModels.PullRequests.PullRequestViewModel)),
            new Route("^[^/]*/(?<Username>[^/]*)/(?<Repository>[^/]*)/issues/$", typeof(Core.ViewModels.Issues.IssuesViewModel)),
            new Route("^[^/]*/(?<Username>[^/]*)/(?<Repository>[^/]*)/commits/$", typeof(Core.ViewModels.Changesets.CommitsViewModel)),
            new Route("^[^/]*/(?<Username>[^/]*)/(?<Repository>[^/]*)/commits/(?<Node>[^/]*)/$", typeof(Core.ViewModels.Changesets.ChangesetViewModel)),
            new Route("^[^/]*/(?<Username>[^/]*)/(?<Repository>[^/]*)/issues/(?<Id>[^/]*)/$", typeof(Core.ViewModels.Issues.IssueViewModel)),
            new Route("^[^/]*/(?<Username>[^/]*)/(?<Repository>[^/]*)/tree/(?<Branch>[^/]*)/(?<Path>.*)$", typeof(Core.ViewModels.Source.SourceTreeViewModel)),
        };

        public static bool Handle(string path)
        {
            if (!path.EndsWith("/", StringComparison.Ordinal))
                path += "/";

            foreach (var route in Routes)
            {
                var regex = new Regex(route.Path, RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);
                var match = regex.Match(path);
                var groups = regex.GetGroupNames().Skip(1).ToList();

                if (match.Success)
                {
                    var vm = IoC.Resolve(route.ViewModelType);
                    foreach (var group in groups)
                    {
                        var property = vm.GetType().GetProperty(group);
                        if (property != null)
                            property.SetValue(vm, match.Groups[group].Value);
                    }


                    //appService.SetUserActivationAction(() => viewDispatcher.ShowViewModel(rec));
                    return true;
                }
            }

            return false;
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

