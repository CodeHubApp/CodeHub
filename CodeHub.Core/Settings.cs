using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace CodeHub.Core
{
    public static class Settings
    {
        private const string DefaultAccountKey = "DEFAULT_ACCOUNT";
        private const string ShouldStarKey = "SHOULD_STAR_CODEHUB";
        private const string ShouldWatchKey = "SHOULD_WATCH_CODEHUB";
        private const string HasSeenWelcomeKey = "HAS_SEEN_WELCOME_INTRO";
        private const string ProEditionKey = "com.dillonbuchanan.codehub.pro";
        private const string HasSeenOAuthKey = "HAS_SEEN_OAUTH_INFO";
        private const string ImgurUploadWarn = "IMGUR_UPLOAD_WARN";

        private static ISettings AppSettings => CrossSettings.Current;

        public static string DefaultAccount
        {
            get => AppSettings.GetValueOrDefault(DefaultAccountKey, null);
            set => AppSettings.AddOrUpdateValue(DefaultAccountKey, value);
        }

        public static bool ShouldStar
        {
            get => AppSettings.GetValueOrDefault(ShouldStarKey, false);
            set => AppSettings.AddOrUpdateValue(ShouldStarKey, value);
        }

        public static bool ShouldWatch
        {
            get => AppSettings.GetValueOrDefault(ShouldWatchKey, false);
            set => AppSettings.AddOrUpdateValue(ShouldWatchKey, value);
        }

        public static bool HasSeenWelcome
        {
            get => AppSettings.GetValueOrDefault(HasSeenWelcomeKey, false);
            set => AppSettings.AddOrUpdateValue(HasSeenWelcomeKey, value);
        }

        public static bool IsProEnabled
        {
            get => AppSettings.GetValueOrDefault(ProEditionKey, false);
            set => AppSettings.AddOrUpdateValue(ProEditionKey, value);
        }

        public static bool HasSeenOAuthWelcome
        {
            get => AppSettings.GetValueOrDefault(HasSeenOAuthKey, false);
            set => AppSettings.AddOrUpdateValue(HasSeenOAuthKey, value);
        }

        public static bool HasSeenImgurUploadWarn
        {
            get => AppSettings.GetValueOrDefault(ImgurUploadWarn, false);
            set => AppSettings.AddOrUpdateValue(ImgurUploadWarn, value);
        }
    }
}