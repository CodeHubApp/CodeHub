namespace CodeHub.WebViews
{
    public class SourceBrowserModel
    {
        public string Content { get; private set; }
        public string Theme { get; private set; }
        public string Language { get; private set; }
        public int FontSize { get; private set; }

        public SourceBrowserModel(string content, string theme, int fontSize, string file = null)
        {
            Content = content;
            Theme = theme;
            FontSize = fontSize;

            if (file != null)
                Language = CalculateLanguage(System.IO.Path.GetExtension(file));
        }

        private static string CalculateLanguage(string extension)
        {
            if (extension != null)
            {
                switch (extension.ToLower().Trim('.', ' '))
                {
                    case "rb":
                    case "erb":
                        return "ruby";
                    case "go":
                        return "go";
                    case "cs":
                        return "cs";
                    case "fs":
                        return "fsharp";
                    case "py":
                        return "python";
                    case "js":
                        return "javascript";
                    case "css":
                        return "css";
                }
            }

            return string.Empty;
        }
    }
}

