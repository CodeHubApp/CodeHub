namespace CodeHub.iOS.WebViews
{
    public class SourceBrowserModel
    {
        public string Content { get; }
        public string Theme { get; }
        public string Language { get; }
        public int FontSize { get; }
        public decimal Scale { get; }

        public SourceBrowserModel(string content, string theme, int fontSize, bool shouldZoom, string file = null)
        {
            Content = content;
            Theme = theme;
            FontSize = fontSize;
            Scale = shouldZoom ? 1.0m : 0.4m;

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

