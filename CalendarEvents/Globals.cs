// Global usings.
global using CalendarEvents.Resources.Languages;
global using System.Globalization;
global using Microsoft.AppCenter.Crashes;

namespace CalendarEvents;

// Global variables and methods.
static class Globals
{
    // Global variables.
    public static string cTheme;
    public static string cDateFormatSelect;
    public static string cDateFormatDatePicker;
    public static string cDateFormat;
    public static string cTimeFormat;   
    public static string cAddDaysToStart;
    public static string cAddDaysToEnd;
    public static string cLanguage;
    public static bool bLanguageChanged = false;
    public static string cLanguageSpeech;
    public static string[] cLanguageLocales;
    public static bool bLanguageLocalesExist = false;
    public static bool bTextToSpeechIsBusy = false;
    public static bool bLicense;

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