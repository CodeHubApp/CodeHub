namespace CodeHub.WebViews
{
    public class SyntaxHighlighterModel
    {
        public string Content { get; }
        public string Theme { get; }
        public string Language { get; }
        public int FontSize { get; }
        public string Viewport { get; }

        public SyntaxHighlighterModel(string content, string theme, int fontSize, bool shouldZoom, bool lockWidth = false, string file = null)
        {
            Content = content;
            Theme = theme;
            FontSize = fontSize;

            var scale = shouldZoom ? 1.0m : 0.4m;
            var width = lockWidth ? "width=device-width" : string.Empty;
            Viewport = $"minimum-scale={scale} maximum-scale=4.0 {width}";

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

