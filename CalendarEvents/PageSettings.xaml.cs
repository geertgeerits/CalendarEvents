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
            DisplayAlert("InitializeComponent PageSettings", ex.Message, "OK");
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
        if (Globals.bDateFormatSystem == true)
        {
            rbnDateFormatSystem.IsChecked = true;
        }
        else
        {
            rbnDateFormatISO8601.IsChecked = true;
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

#if IOS
        // Workaround for !!!BUG!!! in IOS RadioButton: Add a space before the content text.
        rbnDateFormatSystem.Content = $" {CalEventLang.System_Text}";
        rbnDateFormatISO8601.Content = $" {CalEventLang.DateISO8601_Text}";
#endif
    }

    // Radio button date format clicked event.
    private void OnDateFormatRadioButtonCheckedChanged(object sender, EventArgs e)
    {
        if (rbnDateFormatSystem.IsChecked)
        {
            Globals.bDateFormatSystem = true;
            Globals.cDateFormat = Globals.cSysDateFormat;
        }
        else if (rbnDateFormatISO8601.IsChecked)
        {
            Globals.bDateFormatSystem = false;
            Globals.cDateFormat = "yyyy-MM-dd";
        }
    }

    // Button save settings clicked event.
    private static void OnSettingsSaveClicked(object sender, EventArgs e)
    {
        Preferences.Default.Set("SettingTheme", Globals.cTheme);
        Preferences.Default.Set("SettingDateFormatSystem", Globals.bDateFormatSystem);
        Preferences.Default.Set("SettingLanguage", Globals.cLanguage);

        // Wait 500 milliseconds otherwise the settings are not saved in Android.
        Task.Delay(500).Wait();

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
            Preferences.Default.Remove("SettingDateFormatSystem");
            Preferences.Default.Remove("SettingLanguage");
        }

        // Wait 500 milliseconds otherwise the settings are not saved in Android.
        Task.Delay(500).Wait();

        // Restart the application.
        //Application.Current.MainPage = new AppShell();
        Application.Current.MainPage = new NavigationPage(new MainPage());
    }
}