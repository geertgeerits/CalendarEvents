using System.Diagnostics;

namespace CalendarEvents;

public partial class PageSettings : ContentPage
{
    // Local variables.
    private readonly Stopwatch stopWatch = new();

    public PageSettings()
    {        
        try
        {
            InitializeComponent();
        }
        catch (Exception ex)
        {
            Crashes.TrackError(ex);
#if DEBUG
            DisplayAlert("InitializeComponent PageSettings", ex.Message, "OK");
#endif
            return;
        }

        // Put text in the chosen language in the controls and variables.
        SetLanguage();
        
        // Set the current language in the picker.
        pckLanguage.SelectedIndex = Globals.cLanguage switch
        {
            // Čeština - Czech.
            "cs" => 0,

            // Dansk - Danish.
            "da" => 1,

            // Deutsch - German.
            "de" => 2,

            // Español - Spanish.
            "es" => 4,

            // Français - French.
            "fr" => 5,

            // Italiano - Italian.
            "it" => 6,

            // Magyar - Hungarian.
            "hu" => 7,

            // Nederlands - Dutch.
            "nl" => 8,

            // Norsk Bokmål - Norwegian Bokmål.
            "nb" => 9,

            // Polski - Polish.
            "pl" => 10,

            // Português - Portuguese.
            "pt" => 11,

            // Română - Romanian.
            "ro" => 12,

            // Suomi - Finnish.
            "fi" => 13,

            // Svenska - Swedish.
            "sv" => 14,

            // English.
            _ => 3,
        };

        // Fill the picker with the speech languages and set the saved language in the picker.
        FillPickerWithSpeechLanguages();

        // Set the current theme in the picker.
        pckTheme.SelectedIndex = Globals.cTheme switch
        {
            // Light.
            "Light" => 1,

            // Dark.
            "Dark" => 2,

            // System.
            _ => 0,
        };

        // Set radiobutton to the date format.
        switch (Globals.cDateFormatSelect)
        {
            case "SystemShort":
                rbnDateFormatSystemShort.IsChecked = true;
                break;
            case "SystemLong":
                rbnDateFormatSystemLong.IsChecked = true;
                break;
            default:
                rbnDateFormatISO8601.IsChecked = true;
                break;
        }

        // Set the days in the past and in the future.
        entAddDaysToStart.Text = Globals.cAddDaysToStart;
        entAddDaysToEnd.Text = Globals.cAddDaysToEnd;

        // Put the calendars from the calendarDictionary via the calendarList in the picker.
        List<string> calendarList = [.. Globals.calendarDictionary.Values];

        pckCalendars.ItemsSource = calendarList;

        if (Globals.nSelectedCalendar > calendarList.Count)
        {
            pckCalendars.SelectedIndex = 0;
            Globals.nSelectedCalendar = 0;
        }
        else
        {
            pckCalendars.SelectedIndex = Globals.nSelectedCalendar;
        }

        // Start the stopWatch for resetting all the settings.
        stopWatch.Start();
    }

    // Picker language clicked event.
    private void OnPickerLanguageChanged(object sender, EventArgs e)
    {
        string cLanguageOld = Globals.cLanguage;

        var picker = (Picker)sender;
        int selectedIndex = picker.SelectedIndex;

        if (selectedIndex != -1)
        {
            Globals.cLanguage = selectedIndex switch
            {
                // Čeština - Czech.
                0 => "cs",

                // Dansk - Danish.
                1 => "da",

                // Deutsch - German.
                2 => "de",

                // Español - Spanish.
                4 => "es",

                // Français - French.
                5 => "fr",

                // Italiano - Italian.
                6 => "it",

                // Magyar - Hungarian.
                7 => "hu",

                // Nederlands - Dutch.
                8 => "nl",

                // Norsk Bokmål - Norwegian Bokmål.
                9 => "nb",

                // Polski - Polish.
                10 => "pl",

                // Português - Portuguese.
                11 => "pt",

                // Română - Romanian.
                12 => "ro",

                // Suomi - Finnish.
                13 => "fi",

                // Svenska - Swedish.
                14 => "sv",

                // English.
                _ => "en",
            };
        }

        if (cLanguageOld != Globals.cLanguage)
        {
            Globals.bLanguageChanged = true;

            // Set the current UI culture of the selected language.
            Globals.SetCultureSelectedLanguage();

            // Put text in the chosen language in the controls and variables.
            SetLanguage();

            // Search the new language in the cLanguageLocales array and select the new speech language.
            int nTotalItems = Globals.cLanguageLocales.Length;

            for (int nItem = 0; nItem < nTotalItems; nItem++)
            {
                if (Globals.cLanguageLocales[nItem].StartsWith(Globals.cLanguage))
                {
                    pckLanguageSpeech.SelectedIndex = nItem;
                    break;
                }
            }
        }
    }

    // Fill the picker with the speech languages from the array.
    // .Country = KR ; .Id = ''  ; .Language = ko ; .Name = Korean (South Korea) ; 
    private void FillPickerWithSpeechLanguages()
    {
        // If there are no locales then return.
        bool bIsSetSelectedIndex = false;

        if (!Globals.bLanguageLocalesExist)
        {
            pckLanguageSpeech.IsEnabled = false;
            return;
        }

        // Put the sorted locales from the array in the picker and select the saved language.
        int nTotalItems = Globals.cLanguageLocales.Length;

        for (int nItem = 0; nItem < nTotalItems; nItem++)
        {
            pckLanguageSpeech.Items.Add(Globals.cLanguageLocales[nItem]);

            if (Globals.cLanguageSpeech == Globals.cLanguageLocales[nItem])
            {
                pckLanguageSpeech.SelectedIndex = nItem;
                bIsSetSelectedIndex = true;
            }
        }

        // If the language is not found set the picker to the first item.
        if (!bIsSetSelectedIndex)
        {
            pckLanguageSpeech.SelectedIndex = 0;
        }
    }

    // Picker speech language clicked event.
    private void OnPickerLanguageSpeechChanged(object sender, EventArgs e)
    {
        var picker = (Picker)sender;
        int selectedIndex = picker.SelectedIndex;

        if (selectedIndex != -1)
        {
            Globals.cLanguageSpeech = picker.Items[selectedIndex];
        }
    }

    // Picker theme clicked event.
    private void OnPickerThemeChanged(object sender, EventArgs e)
    {
        var picker = (Picker)sender;
        int selectedIndex = picker.SelectedIndex;

        if (selectedIndex != -1)
        {
            Globals.cTheme = selectedIndex switch
            {
                // Light.
                1 => "Light",

                // Dark.
                2 => "Dark",

                // System.
                _ => "System",
            };

            // Set the theme.
            Globals.SetTheme();
        }
    }

    // Select all the text in the entry field.
    private void EntryFocused(object sender, EventArgs e)
    {
        var entry = (Entry)sender;

        entry.CursorPosition = entry.Text.Length;
        entry.CursorPosition = 0;
        entry.SelectionLength = entry.Text.Length;
    }

    // Put text in the chosen language in the controls and variables.
    private void SetLanguage()
    {
        var ThemeList = new List<string>
        {
            CalEventLang.System_Text,
            CalEventLang.Light_Text,
            CalEventLang.Dark_Text
        };
        pckTheme.ItemsSource = ThemeList;

        // Set the current theme in the picker.
        pckTheme.SelectedIndex = Globals.cTheme switch
        {
            // Light.
            "Light" => 1,

            // Dark.
            "Dark" => 2,

            // System.
            _ => 0,
        };
    }

    // Radio button date format clicked event.
    private void OnDateFormatRadioButtonCheckedChanged(object sender, EventArgs e)
    {       
        if (rbnDateFormatSystemShort.IsChecked)
        {
            Globals.cDateFormatSelect = "SystemShort";
            Globals.cDateFormatDatePicker = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
            Globals.cDateFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
            Globals.cTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;
        }
        else if (rbnDateFormatSystemLong.IsChecked)
        {
            Globals.cDateFormatSelect = "SystemLong";
            Globals.cDateFormatDatePicker = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
            Globals.cDateFormat = CultureInfo.CurrentCulture.DateTimeFormat.LongDatePattern;
            Globals.cTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;
        }
        else if (rbnDateFormatISO8601.IsChecked)
        {
            Globals.cDateFormatSelect = "ISO8601";
            Globals.cDateFormatDatePicker = "yyyy-MM-dd";
            Globals.cDateFormat = "yyyy-MM-dd";
            Globals.cTimeFormat = "HH:mm";            
        }
    }

    // Verify the number of days in the past.
    private void VerifyAddDaysToStart(object sender, EventArgs e)
    {
        // Validate input values.
        bool bIsNumber = int.TryParse(entAddDaysToStart.Text, out int nAddDaysToStart);
        if (bIsNumber == false || nAddDaysToStart < -500 || nAddDaysToStart > 500)
        {
            entAddDaysToStart.Text = "";
            entAddDaysToStart.Focus();
            return;
        }

        Globals.cAddDaysToStart = Convert.ToString(nAddDaysToStart);

        entAddDaysToEnd.Focus();
    }

    // Verify the number of days in the future.
    private void VerifyAddDaysToEnd(object sender, EventArgs e)
    {
        bool bIsNumber = int.TryParse(entAddDaysToEnd.Text, out int nAddDaysToEnd);
        if (bIsNumber == false || nAddDaysToEnd < -500 || nAddDaysToEnd > 500)
        {
            entAddDaysToEnd.Text = "";
            entAddDaysToEnd.Focus();
            return;
        }

        Globals.cAddDaysToEnd = Convert.ToString(nAddDaysToEnd);

        // Close the keyboard.
        entAddDaysToEnd.IsEnabled = false;
        entAddDaysToEnd.IsEnabled = true;
    }

    // Event calendar picker changed.
    private void OnPickerCalendarChanged(object sender, EventArgs e)
    {
        int nSelectedIndex = pckCalendars.SelectedIndex;

        if (nSelectedIndex == -1)
        {
            nSelectedIndex = 0;
        }

        // All calendars.
        if (nSelectedIndex == 0)
        {
            return;
        }

        Globals.nSelectedCalendar= nSelectedIndex;

        // One calendar.
        //cCalendarId = Globals.calendarDictionary.Keys.ElementAt(nSelectedIndex);
    }

    // Button save settings clicked event.
    private static void OnSettingsSaveClicked(object sender, EventArgs e)
    {
        Preferences.Default.Set("SettingTheme", Globals.cTheme);
        Preferences.Default.Set("SettingDateFormatSelect", Globals.cDateFormatSelect);
        Preferences.Default.Set("SettingAddDaysToStart", Globals.cAddDaysToStart);
        Preferences.Default.Set("SettingAddDaysToEnd", Globals.cAddDaysToEnd);
        Preferences.Default.Set("SettingSelectedCalendar", Globals.nSelectedCalendar);
        Preferences.Default.Set("SettingLanguage", Globals.cLanguage);
        Preferences.Default.Set("SettingLanguageSpeech", Globals.cLanguageSpeech);

        // Give it some time to save the settings.
        Task.Delay(400).Wait();

        // Restart the application.
        //Application.Current.MainPage = new AppShell();
        Application.Current.MainPage = new NavigationPage(new MainPage());
    }

    // Button reset settings clicked event.
    private void OnSettingsResetClicked(object sender, EventArgs e)
    {
        // Get the elapsed time in milli seconds.
        stopWatch.Stop();

        if (stopWatch.ElapsedMilliseconds < 2001)
        {
            // Clear all settings after the first clicked event within the first 2 seconds after opening the setting page.
            Preferences.Default.Clear();
        }
        else
        {
            // Reset some settings.
            Preferences.Default.Remove("SettingTheme");
            Preferences.Default.Remove("SettingDateFormatSelect");
            Preferences.Default.Remove("SettingAddDaysToStart");
            Preferences.Default.Remove("SettingAddDaysToEnd");
            Preferences.Default.Remove("SettingSelectedCalendar");
            Preferences.Default.Remove("SettingLanguage");
            Preferences.Default.Remove("SettingLanguageSpeech");
        }

        // Give it some time to remove the settings.
        Task.Delay(400).Wait();

        // Restart the application.
        //Application.Current.MainPage = new AppShell();
        Application.Current.MainPage = new NavigationPage(new MainPage());
    }
}