using System;
using Foundation;

public static class LocalizationExtensions
{
    /// <summary>
    /// Gets the localized text for the specified string.
    /// </summary>
    public static string t(this string text)
    {
        return String.IsNullOrEmpty (text) ? text : NSBundle.MainBundle.LocalizedString (text, String.Empty, String.Empty);
    }
}

