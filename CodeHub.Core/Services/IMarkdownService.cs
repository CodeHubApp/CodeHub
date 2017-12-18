using System.Threading.Tasks;

namespace CodeHub.Core.Services
{
    public interface IMarkdownService
    {
        Task<string> Convert(string s);
    }
}

