using System.Collections.Generic;

namespace CodeHub.Core.Data
{
    public class Showcase
    {
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
    }

    public class ShowcaseRepositories
    {
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public List<Octokit.Repository> Repositories { get; set; } 
    }
}

