//// Global usings
global using CalendarEvents.Resources.Languages;
global using System.Globalization;
//global using Microsoft.AppCenter.Crashes;

namespace CalendarEvents
{
    //// Global variables and methods
    internal static class Globals
    {
        //// Global variables
        public static string cTheme;
        public static string cDateFormatSelect;
        public static string cDateFormatDatePicker;
        public static string cDateFormat;
        public static string cTimeFormat;   
        public static string cAddDaysToStart;
        public static string cAddDaysToEnd;
        public static int nSelectedCalendar;
        public static string cLanguage;
        public static bool bLanguageChanged = false;
        public static string cLanguageSpeech;
        public static string[] cLanguageLocales;
        public static bool bLanguageLocalesExist = false;
        public static bool bTextToSpeechIsBusy = false;
        public static CancellationTokenSource cts;
        public static Dictionary<string, string> calendarDictionary = [];
        public static bool bLicense;

        //// Global methods

        /// <summary>
        /// Set the theme 
        /// </summary>
        public static void SetTheme()
        {
            Application.Current.UserAppTheme = cTheme switch
            {
                "Light" => AppTheme.Light,
                "Dark" => AppTheme.Dark,
                _ => AppTheme.Unspecified,
            };
        }

        /// <summary>
        /// Set the current UI culture of the selected language 
        /// </summary>
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
}