using System;
using GitHubSharp.Models;

namespace CodeHub.iOS.Views.Releases
{
    public class ReleaseRazorViewModel
    {
        public string Body { get; set; }

        public ReleaseModel Release { get; set; }

        public string ReleaseTime
        {
            get
            {
                if (Release.PublishedAt.HasValue)
                    return Release.PublishedAt.Value.ToDaysAgo();
                else
                    return Release.CreatedAt.ToDaysAgo();
            }
        }
    }
}

