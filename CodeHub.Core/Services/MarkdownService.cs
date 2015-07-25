namespace CodeHub.Core.Services
{
    public class MarkdownService : IMarkdownService
    {
        public string Convert(string s)
        {
            return CommonMark.CommonMarkConverter.Convert(s);
        }
    }
}
