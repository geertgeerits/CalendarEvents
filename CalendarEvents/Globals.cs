// Global usings.
global using CalendarEvents.Resources.Languages;
global using System.Globalization;

namespace CalendarEvents;

// Global variables and methods.
static class Globals
{
    // Global variables.
    public static string cTheme;
    public static bool bDateFormatSystem;
    public static string cSysDateFormat;
    public static string cDateFormat;
    public static string cLanguage;
    public static bool bLanguageChanged = false;

    // Global methods.
    // Set the current UI culture of the selected language.
    public static void SetCultureSelectedLanguage()
    {
        try
        {
            CultureInfo switchToCulture = new(cLanguage);
            LocalizationResourceManager.Instance.SetCulture(switchToCulture);
        }
        catch
        {
            // Do nothing.
        }
    }
}