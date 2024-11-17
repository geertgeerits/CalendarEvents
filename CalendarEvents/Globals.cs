//// Global usings
global using CalendarEvents.Resources.Languages;
global using System.Globalization;

namespace CalendarEvents
{
    //// Global variables and methods
    internal static class Globals
    {
        //// Global variables
        public static string cTheme = "";
        public static string cDateFormatSelect = "";
        public static string cDateFormatDatePicker = "";
        public static string cDateFormat = "";
        public static string cTimeFormat = "";   
        public static string cAddDaysToStart = "";
        public static string cAddDaysToEnd = "";
        public static int nSelectedCalendar;
        public static string cLanguage = "";
        public static bool bLanguageChanged;
        public static string cLanguageSpeech = "";
        public static string[]? cLanguageLocales;
        public static bool bLanguageLocalesExist;
        public static bool bTextToSpeechIsBusy;
        public static CancellationTokenSource? cts;
        public static Dictionary<string, string> calendarDictionary = [];
        public static bool bLicense;

        //// Global methods

        /// <summary>
        /// Set the theme 
        /// </summary>
        public static void SetTheme()
        {
            Application.Current!.UserAppTheme = cTheme switch
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

        /// <summary>
        /// Select all the text in the entry field
        /// </summary>
        public static void ModifyEntrySelectAllText()
        {
            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("MyCustomization", (handler, view) =>
            {
#if ANDROID
                handler.PlatformView.SetSelectAllOnFocus(true);
#elif IOS || MACCATALYST
                handler.PlatformView.EditingDidBegin += (s, e) =>
                {
                    handler.PlatformView.PerformSelector(new ObjCRuntime.Selector("selectAll"), null, 0.0f);
                };
#elif WINDOWS
            handler.PlatformView.GotFocus += (s, e) =>
            {
                handler.PlatformView.SelectAll();
            };
#endif
            });
        }
    }
}