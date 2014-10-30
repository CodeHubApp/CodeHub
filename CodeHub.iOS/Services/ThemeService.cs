using CodeHub.iOS.Themes;

namespace CodeHub.iOS.Services
{
    public class ThemeService : IThemeService
    {
        public ThemeService()
        {
            CurrentTheme = new DefaultTheme();
        }

        public ITheme CurrentTheme { get; private set; }
    }
}

