using System;
using MvvmCross.Core.ViewModels;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using MvvmCross.Platform;
using CodeHub.Core.Services;
using MvvmCross.Core.Views;

namespace CodeHub.iOS
{
    public static class UrlRouteProvider
    {
        private static Route[] Routes = {
            new Route("^gist.github.com/$", typeof(CodeHub.Core.ViewModels.Gists.UserGistsViewModel)),
            new Route("^gist.github.com/(?<Username>[^/]*)/$", typeof(CodeHub.Core.ViewModels.Gists.UserGistsViewModel)),
            new Route("^gist.github.com/(?<Username>[^/]*)/(?<Id>[^/]*)/$", typeof(CodeHub.Core.ViewModels.Gists.GistViewModel)),
            new Route("^[^/]*/stars/$", typeof(CodeHub.Core.ViewModels.Repositories.RepositoriesStarredViewModel)),
            new Route("^[^/]*/(?<Username>[^/]*)/$", typeof(CodeHub.Core.ViewModels.User.UserViewModel)),
            new Route("^[^/]*/(?<Username>[^/]*)/(?<Repository>[^/]*)/$", typeof(CodeHub.Core.ViewModels.Repositories.RepositoryViewModel)),
            new Route("^[^/]*/(?<Username>[^/]*)/(?<Repository>[^/]*)/pulls/$", typeof(CodeHub.Core.ViewModels.PullRequests.PullRequestsViewModel)),
            new Route("^[^/]*/(?<Username>[^/]*)/(?<Repository>[^/]*)/pull/(?<Id>[^/]*)/$", typeof(CodeHub.Core.ViewModels.PullRequests.PullRequestViewModel)),
            new Route("^[^/]*/(?<Username>[^/]*)/(?<Repository>[^/]*)/issues/$", typeof(CodeHub.Core.ViewModels.Issues.IssuesViewModel)),
            new Route("^[^/]*/(?<Username>[^/]*)/(?<Repository>[^/]*)/commits/$", typeof(CodeHub.Core.ViewModels.Changesets.CommitsViewModel)),
            new Route("^[^/]*/(?<Username>[^/]*)/(?<Repository>[^/]*)/commits/(?<Node>[^/]*)/$", typeof(CodeHub.Core.ViewModels.Changesets.ChangesetViewModel)),
            new Route("^[^/]*/(?<Username>[^/]*)/(?<Repository>[^/]*)/issues/(?<Id>[^/]*)/$", typeof(CodeHub.Core.ViewModels.Issues.IssueViewModel)),
            new Route("^[^/]*/(?<Username>[^/]*)/(?<Repository>[^/]*)/tree/(?<Branch>[^/]*)/(?<Path>.*)$", typeof(CodeHub.Core.ViewModels.Source.SourceTreeViewModel)),
        };

        public static bool Handle(string path)
        {
            var viewDispatcher = Mvx.Resolve<IMvxViewDispatcher>();
            var appService = Mvx.Resolve<IApplicationService>();
            if (!path.EndsWith("/", StringComparison.Ordinal))
                path += "/";

            foreach (var route in Routes)
            {
                var regex = new Regex(route.Path, RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);
                var match = regex.Match(path);
                var groups = regex.GetGroupNames().Skip(1);

                if (match.Success)
                {
                    var rec = new MvxViewModelRequest();
                    rec.ViewModelType = route.ViewModelType;
                    rec.ParameterValues = new Dictionary<string, string>();
                    foreach (var group in groups)
                        rec.ParameterValues.Add(group, match.Groups[group].Value);
                    appService.SetUserActivationAction(() => viewDispatcher.ShowViewModel(rec));
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

