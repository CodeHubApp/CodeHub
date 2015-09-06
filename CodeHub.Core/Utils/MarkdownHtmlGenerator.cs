using System;
using System.Text;

namespace CodeHub.Core.Utils
{
    public static class MarkdownHtmlGenerator
    {
        public static string CreateFile(string content)
        {
            var markup = System.IO.File.ReadAllText("Markdown/markdown.html", Encoding.UTF8);

            var tmp = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetTempFileName() + ".html");
            using (var tmpStream = new System.IO.FileStream(tmp, System.IO.FileMode.Create))
            {
                var fs = new System.IO.StreamWriter(tmpStream, Encoding.UTF8);
                var dataIndex = markup.IndexOf("{{DATA}}", StringComparison.Ordinal);
                fs.Write(markup.Substring(0, dataIndex));
                fs.Write(content);
                fs.Write(markup.Substring(dataIndex + 8));
                fs.Flush();
            }
            return tmp;
        }
    }
}

