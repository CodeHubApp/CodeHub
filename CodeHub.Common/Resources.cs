using System.Reflection;

namespace CodeHub
{
    public class Resources
    {
        public static string MarkdownScript 
        {
            get { return ResourceLoader.GetEmbeddedResourceString(typeof(ResourceLoader).GetTypeInfo().Assembly, "marked.js"); }
        }
    }
}

