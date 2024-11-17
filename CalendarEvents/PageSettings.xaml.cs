using System.Diagnostics;

namespace CalendarEvents
{
    public sealed partial class PageSettings : ContentPage
    {
        //// Local variables
        private readonly Stopwatch stopWatch = new();

        public PageSettings()
        {        
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
#if DEBUG
                DisplayAlert("InitializeComponent PageSettings", ex.Message, "OK");
#endif
                return;
            }
#if WINDOWS
            //// Set the margins for the controls in the title bar for Windows
            lblTitle.Margin = new Thickness(60, 10, 0, 0);
#endif
            //// Put text in the chosen language in the controls and variables
            SetLanguage();
        
            //// Set the current language in the picker
            pckLanguage.SelectedIndex = Globals.cLanguage switch
            {
                "cs" => 0,      // Čeština - Czech
                "da" => 1,      // Dansk - Danish
                "de" => 2,      // Deutsch - German
                "es" => 4,      // Español - Spanish
                "fr" => 5,      // Français - French
                "it" => 6,      // Italiano - Italian
                "hu" => 7,      // Magyar - Hungarian
                "nl" => 8,      // Nederlands - Dutch
                "nb" => 9,      // Norsk Bokmål - Norwegian Bokmål
                "pl" => 10,     // Polski - Polish
                "pt" => 11,     // Português - Portuguese
                "ro" => 12,     // Română - Romanian
                "fi" => 13,     // Suomi - Finnish
                "sv" => 14,     // Svenska - Swedish
                _ => 3,         // English
            };

            //// Fill the picker with the speech languages and set the saved language in the picker
            FillPickerWithSpeechLanguages();

            //// Set the current theme in the picker
            pckTheme.SelectedIndex = Globals.cTheme switch
            {
                "Light" => 1,       // Light
                "Dark" => 2,        // Dark
                _ => 0,             // System
            };

            //// Set radiobutton to the date format
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

            //// Set the days in the past and in the future
            entAddDaysToStart.Text = Globals.cAddDaysToStart;
            entAddDaysToEnd.Text = Globals.cAddDaysToEnd;

            //// Put the calendars from the calendarDictionary via the calendarList in the picker
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

            //// Start the stopWatch for resetting all the settings
            stopWatch.Start();
        }

        /// <summary>
        /// Picker language clicked event 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPickerLanguageChanged(object sender, EventArgs e)
        {
            string cLanguageOld = Globals.cLanguage;

            var picker = (Picker)sender;
            int selectedIndex = picker.SelectedIndex;

            if (selectedIndex != -1)
            {
                Globals.cLanguage = selectedIndex switch
                {
                    0 => "cs",      // Čeština - Czech
                    1 => "da",      // Dansk - Danish
                    2 => "de",      // Deutsch - German
                    4 => "es",      // Español - Spanish
                    5 => "fr",      // Français - French
                    6 => "it",      // Italiano - Italian
                    7 => "hu",      // Magyar - Hungarian
                    8 => "nl",      // Nederlands - Dutch
                    9 => "nb",      // Norsk Bokmål - Norwegian Bokmål
                    10 => "pl",     // Polski - Polish
                    11 => "pt",     // Português - Portuguese
                    12 => "ro",     // Română - Romanian
                    13 => "fi",     // Suomi - Finnish
                    14 => "sv",     // Svenska - Swedish
                    _ => "en",      // English
                };
            }

            if (cLanguageOld != Globals.cLanguage)
            {
                Globals.bLanguageChanged = true;

                // Set the current UI culture of the selected language
                Globals.SetCultureSelectedLanguage();

                // Put text in the chosen language in the controls and variables
                SetLanguage();

                // Search the new language in the cLanguageLocales array and select the new speech language
                if (Globals.cLanguageLocales is not null)
                {
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
        }

        /// <summary>
        /// Fill the picker with the speech languages from the array 
        /// .Country = KR ; .Id = ''  ; .Language = ko ; .Name = Korean (South Korea) ; 
        /// </summary>
        private void FillPickerWithSpeechLanguages()
        {
            // If there are no locales then return
            bool bIsSetSelectedIndex = false;

            if (!Globals.bLanguageLocalesExist)
            {
                pckLanguageSpeech.IsEnabled = false;
                return;
            }

            // Put the sorted locales from the array in the picker and select the saved language
            if (Globals.cLanguageLocales is not null)
            {
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
            }

            // If the language is not found set the picker to the first item
            if (!bIsSetSelectedIndex)
            {
                pckLanguageSpeech.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Picker speech language clicked event 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPickerLanguageSpeechChanged(object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            int selectedIndex = picker.SelectedIndex;

            if (selectedIndex != -1)
            {
                Globals.cLanguageSpeech = picker.Items[selectedIndex];
            }
        }

        /// <summary>
        /// Picker theme clicked event 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPickerThemeChanged(object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            int selectedIndex = picker.SelectedIndex;

            if (selectedIndex != -1)
            {
                Globals.cTheme = selectedIndex switch
                {
                    1 => "Light",       // Light
                    2 => "Dark",        // Dark
                    _ => "System",      // System
                };

                // Set the theme
                Globals.SetTheme();
            }
        }

        /// <summary>
        /// Put text in the chosen language in the controls and variables 
        /// </summary>
        private void SetLanguage()
        {
            var ThemeList = new List<string>
            {
                CalEventLang.System_Text,
                CalEventLang.Light_Text,
                CalEventLang.Dark_Text
            };
            pckTheme.ItemsSource = ThemeList;

            // Set the current theme in the picker
            pckTheme.SelectedIndex = Globals.cTheme switch
            {
                "Light" => 1,       // Light
                "Dark" => 2,        // Dark
                _ => 0,             // System
            };
        }

        /// <summary>
        /// Radio button date format clicked event 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Verify the number of days in the past 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VerifyAddDaysToStart(object sender, EventArgs e)
        {
            // Validate input values
            bool bIsNumber = int.TryParse(entAddDaysToStart.Text, out int nAddDaysToStart);
            if (bIsNumber == false || nAddDaysToStart < -500 || nAddDaysToStart > 500)
            {
                entAddDaysToStart.Text = "";
                _ = entAddDaysToStart.Focus();
                return;
            }

            Globals.cAddDaysToStart = Convert.ToString(nAddDaysToStart);

            _ = entAddDaysToEnd.Focus();
        }

        /// <summary>
        /// Verify the number of days in the future 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VerifyAddDaysToEnd(object sender, EventArgs e)
        {
            bool bIsNumber = int.TryParse(entAddDaysToEnd.Text, out int nAddDaysToEnd);
            if (bIsNumber == false || nAddDaysToEnd < -500 || nAddDaysToEnd > 500)
            {
                entAddDaysToEnd.Text = "";
                _ = entAddDaysToEnd.Focus();
                return;
            }

            Globals.cAddDaysToEnd = Convert.ToString(nAddDaysToEnd);

            // Close the keyboard
            entAddDaysToEnd.IsEnabled = false;
            entAddDaysToEnd.IsEnabled = true;
        }

        /// <summary>
        /// Event calendar picker changed 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPickerCalendarChanged(object sender, EventArgs e)
        {
            int nSelectedIndex = pckCalendars.SelectedIndex;

            if (nSelectedIndex != -1)
            {
                Globals.nSelectedCalendar = nSelectedIndex;
            }
        }

        /// <summary>
        /// Button save settings clicked event 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnSettingsSaveClicked(object sender, EventArgs e)
        {
            Preferences.Default.Set("SettingTheme", Globals.cTheme);
            Preferences.Default.Set("SettingDateFormatSelect", Globals.cDateFormatSelect);
            Preferences.Default.Set("SettingAddDaysToStart", Globals.cAddDaysToStart);
            Preferences.Default.Set("SettingAddDaysToEnd", Globals.cAddDaysToEnd);
            Preferences.Default.Set("SettingSelectedCalendar", Globals.nSelectedCalendar);
            Preferences.Default.Set("SettingLanguage", Globals.cLanguage);
            Preferences.Default.Set("SettingLanguageSpeech", Globals.cLanguageSpeech);

            // Give it some time to save the settings
            Task.Delay(400).Wait();

            // Restart the application
            //Application.Current!.Windows[0].Page = new AppShell();
            Application.Current!.Windows[0].Page = new NavigationPage(new MainPage());
        }

        /// <summary>
        /// Button reset settings clicked event 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSettingsResetClicked(object sender, EventArgs e)
        {
            // Get the elapsed time in milli seconds
            stopWatch.Stop();

            if (stopWatch.ElapsedMilliseconds < 2001)
            {
                // Clear all settings after the first clicked event within the first 2 seconds after opening the setting page
                Preferences.Default.Clear();
            }
            else
            {
                // Reset some settings
                Preferences.Default.Remove("SettingTheme");
                Preferences.Default.Remove("SettingDateFormatSelect");
                Preferences.Default.Remove("SettingAddDaysToStart");
                Preferences.Default.Remove("SettingAddDaysToEnd");
                Preferences.Default.Remove("SettingSelectedCalendar");
                Preferences.Default.Remove("SettingLanguage");
                Preferences.Default.Remove("SettingLanguageSpeech");
            }

            // Give it some time to remove the settings
            Task.Delay(400).Wait();

            // Restart the application
            //Application.Current!.Windows[0].Page = new AppShell();
            Application.Current!.Windows[0].Page = new NavigationPage(new MainPage());
        }
    }
}