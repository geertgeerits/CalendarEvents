﻿// Program .....: CalendarEvents.sln
// Author ......: Geert Geerits - E-mail: geertgeerits@gmail.com
// Copyright ...: (C) 2023-2023
// Version .....: 1.0.5
// Date ........: 2023-10-25 (YYYY-MM-DD)
// Language ....: Microsoft Visual Studio 2022: .NET 8.0 MAUI C# 12.0
// Description .: Read calendar events to share
// Dependencies : NuGet Package: Plugin.Maui.CalendarStore version 1.0.1 ; https://github.com/jfversluis/Plugin.Maui.CalendarStore
//                NuGet Package: Microsoft.AppCenter version 5.0.3 ; https://appcenter.ms/apps ; https://azure.microsoft.com/en-us/products/app-center/
//                NuGet Package: Microsoft.AppCenter.Crashes version 5.0.3 
// Thanks to ...: Gerald Versluis

using Plugin.Maui.CalendarStore;

namespace CalendarEvents;

public partial class MainPage : ContentPage
{
    // Local variables.
    private string cCopyright;
    private string cLicenseText;
    private readonly bool bLogAlwaysSend;
    private string cCalendarId;
    private readonly string cDicKeyAllCalendars = "000-AllCalendars-gg51";
    private readonly Dictionary<string, string> calendarDictionary = new();
    private IEnumerable<CalendarEvent> events;
    private IEnumerable<Locale> locales;
    private CancellationTokenSource cts;
    private bool bTextToSpeechIsBusy = false;

    public MainPage()
    {
        try
        {
            InitializeComponent();
        }
        catch (Exception ex)
        {
            Crashes.TrackError(ex);
            DisplayAlert("InitializeComponent MainPage", ex.Message, "OK");
            return;
        }

        // Get the saved settings.
        Globals.cTheme = Preferences.Default.Get("SettingTheme", "System");
        Globals.cDateFormatSelect = Preferences.Default.Get("SettingDateFormatSelect", "SystemShort");
        Globals.cAddDaysToStart = Preferences.Default.Get("SettingAddDaysToStart", "0");
        Globals.cAddDaysToEnd = Preferences.Default.Get("SettingAddDaysToEnd", "31");
        Globals.cLanguage = Preferences.Default.Get("SettingLanguage", "");
        Globals.cLanguageSpeech = Preferences.Default.Get("SettingLanguageSpeech", "");
        Globals.bLicense = Preferences.Default.Get("SettingLicense", false);
        bLogAlwaysSend = Preferences.Default.Get("SettingLogAlwaysSend", false);

        // Crash log confirmation.
        if (!bLogAlwaysSend)
        {
            Crashes.ShouldAwaitUserConfirmation = () =>
            {
                // Return true if you built a UI for user consent and are waiting for user input on that custom UI, otherwise false.
                ConfirmationSendCrashLog();
                return true;
            };
        }

#if IOS
        // The height of the title bar is lower when an iPhone is in horizontal position.
        imgbtnAbout.VerticalOptions = LayoutOptions.Start;
        lblTitle.VerticalOptions = LayoutOptions.Start;
        imgbtnSettings.VerticalOptions = LayoutOptions.Start;
#endif

        // Set the theme.
        switch (Globals.cTheme)
        {
            case "Light":
                Application.Current.UserAppTheme = AppTheme.Light;
                break;
            case "Dark":
                Application.Current.UserAppTheme = AppTheme.Dark;
                break;
            default:
                Application.Current.UserAppTheme = AppTheme.Unspecified;
                break;
        }

        // Get the system date and time format and set the date and time format.       
        switch (Globals.cDateFormatSelect)
        {
            case "SystemShort":
                Globals.cDateFormatDatePicker = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
                Globals.cDateFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
                Globals.cTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;
                break;
            case "SystemLong":
                Globals.cDateFormatDatePicker = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
                Globals.cDateFormat = CultureInfo.CurrentCulture.DateTimeFormat.LongDatePattern;
                Globals.cTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;
                break;
            default:
                Globals.cDateFormatDatePicker = "yyyy-MM-dd";
                Globals.cDateFormat = "yyyy-MM-dd";
                Globals.cTimeFormat = "HH:mm";
                break;
        }

        // Get and set the system OS user language.
        try
        {
            if (string.IsNullOrEmpty(Globals.cLanguage))
            {
                Globals.cLanguage = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
            }
        }
        catch (Exception)
        {
            Globals.cLanguage = "en";
        }

        // Set the date properties for the DatePickers.
        dtpDateStart.MinimumDate = new DateTime(1583, 1, 1);
        dtpDateStart.MaximumDate = new DateTime(3000, 1, 1);
        dtpDateEnd.MinimumDate = new DateTime(1583, 1, 1);
        dtpDateEnd.MaximumDate = new DateTime(3000, 1, 1);

        // Set the text language.
        SetTextLanguage();

        // Initialize text to speech and get and set the speech language.
        string cCultureName = "";

        try
        {
            if (string.IsNullOrEmpty(Globals.cLanguageSpeech))
            {
                cCultureName = Thread.CurrentThread.CurrentCulture.Name;
            }
        }
        catch (Exception)
        {
            cCultureName = "en-US";
        }
        //DisplayAlert("cCultureName", $"*{cCultureName}*", "OK");  // For testing.

        InitializeTextToSpeech(cCultureName);

        // Get all the calendars from the device and put them in a picker.
        GetCalendars();
    }   

    // TitleView buttons clicked events.
    private async void OnPageAboutClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new PageAbout());
    }

    private async void OnPageSettingsClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new PageSettings());
    }

    // Select all the text in the entry field.
    private void EntryFocused(object sender, EventArgs e)
    {
        var entry = (Entry)sender;

        entry.CursorPosition = entry.Text.Length;
        entry.CursorPosition = 0;
        entry.SelectionLength = entry.Text.Length;
    }

    // Event calendar picker changed.
    private void OnPickerCalendarChanged(object sender, EventArgs e)
    {
        lblCalendarEvents.Text = "";
        
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

        // One calendar.
        cCalendarId = calendarDictionary.Keys.ElementAt(nSelectedIndex);
    }

    // Get all the calendars from the device and put them in a picker.
    private async void GetCalendars()
    {
        await LoadCalendars();
    }

    // Get all the calendars from the device and put them in a picker.
    private async Task LoadCalendars()
    {
#if ANDROID
        // Permissions for Calendar read - Sometimes permission is not given in Android.
        _ = await CheckAndRequestCalendarRead();
#endif
        // Declare a temporary dictionary used to sort the calendars on name.
        Dictionary<string, string> calendarDictionaryTemp = new();

        // Declare variable for the number op retries for asking for permission to read the calendar.
        int nRetries = 0;

    Start:
        try
        {
            // For testing crashes - DivideByZeroException.
            //int divByZero = 51 / int.Parse("0");

            // Get all the calendars from the device.
            var calendars = await CalendarStore.Default.GetCalendars();

            // Local language name for 'All calendars' (first item in the calendarDictionary and list of calendars).
            // After using the save button in the settings page the calendarDictionary is not empty.
            if (calendarDictionary.Count > 0)
            {
                calendarDictionary.Clear();
            }

            calendarDictionary.Add(cDicKeyAllCalendars, CalEventLang.AllCalendars_Text);

            // Put the calendars in the calendarDictionaryTemp.
            foreach (var calendar in calendars)
            {
                calendarDictionaryTemp.Add(calendar.Id, calendar.Name);
            }

            // Sort the calendarDictionaryTemp by value (calendar name).
            calendarDictionaryTemp = calendarDictionaryTemp.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

            // Add the sorted calendarDictionaryTemp to the calendarDictionary - 'All calendars' stays this way on the first place.
            foreach (var item in calendarDictionaryTemp)
            {
                if (!calendarDictionary.ContainsKey(item.Key))
                {
                    calendarDictionary.Add(item.Key, item.Value);
                }
            }

            // Put the calendars from the calendarDictionary via the calendarList in the picker.
            List<string> calendarList = calendarDictionary.Values.ToList();
            
            pckCalendars.ItemsSource = calendarList;
            pckCalendars.SelectedIndex = 0;
        }
        catch (Exception ex) when (ex is ArgumentException)
        {
            // ArgumentException: Value does not fall within the expected range.
            // The Add method throws an exception if the new key is already in the dictionary.
            //await DisplayAlert(CalEventLang.ErrorTitle_Text, ex.Message, CalEventLang.ButtonClose_Text);
        }
        catch (Exception ex) when (ex is NullReferenceException)
        {
            // NullReferenceException: Object reference not set to an instance of an object.
            // Permissions for CalendarRead - Sometimes permission is not given.
            if (nRetries < 4)
            {
                nRetries++;
                _ = await CheckAndRequestCalendarRead();
                goto Start;
            }
        }
        catch (Exception ex)
        {
            var properties = new Dictionary<string, string>
            {
                { "File:", "MainPage.xaml.cs" },
                { "Method:", "LoadCalendars" },
                { "CalendarStore:", "GetCalendars" },
                { "AppLanguage:", Globals.cLanguage }
            };
            Crashes.TrackError(ex, properties);

            _ = DisplayAlert(CalEventLang.ErrorTitle_Text, ex.Message, CalEventLang.ButtonClose_Text);
        }
    }

    // Get calendar events.
    private async void OnGetEventsClicked(object sender, EventArgs e)
    {
        // Validate the date values.
        if (dtpDateStart.Date > dtpDateEnd.Date)
        {
            // Swap the two dates.
            (dtpDateStart.Date, dtpDateEnd.Date) = (dtpDateEnd.Date, dtpDateStart.Date);
        }

        // Close the keyboard.
        entSearchWord.IsEnabled = false;
        entSearchWord.IsEnabled = true;

        // Clear the calendar events.
        lblCalendarEvents.Text = "";

        // Get calendar events. !!!BUG!!!? activityIndicator is only working after adding a Task.Delay().
        activityIndicator.IsRunning = true;
        await Task.Delay(200);

        await LoadEvents();

        activityIndicator.IsRunning = false;
    }

    // Get calendar events.
    private async Task LoadEvents()    
    {
        // Get (all) the events from the calendar.
        string cCalendarEvents = "";

        try
        {
            // For testing crashes - DivideByZeroException.
            //int divByZero = 51 / int.Parse("0");

            // All calendars.
            if (pckCalendars.SelectedIndex == 0)
            {
                events = await CalendarStore.Default.GetEvents(startDate: dtpDateStart.Date, endDate: dtpDateEnd.Date.AddDays(1));
            }
            // One calendar.
            else
            {
                events = await CalendarStore.Default.GetEvents(calendarId: cCalendarId, startDate: dtpDateStart.Date, endDate: dtpDateEnd.Date.AddDays(1));
            }

            if (entSearchWord.Text is null or "")
            {
                foreach (CalendarEvent ev in events)
                {
                    cCalendarEvents = $"{cCalendarEvents}{ev.StartDate.ToString(format: $"{Globals.cDateFormat}  {Globals.cTimeFormat}")}   {ev.Title}\n\n";
                }
            }
            else
            {
                string cSearchWord = entSearchWord.Text.ToLowerInvariant().Trim();

                foreach (CalendarEvent ev in events)
                {
                    if (ev.Title is null or "")
                    {
                        continue;
                    }
                    
                    if (ev.Title.ToLowerInvariant().Contains(cSearchWord))
                    {
                        cCalendarEvents = $"{cCalendarEvents}{ev.StartDate.ToString(format: $"{Globals.cDateFormat}  {Globals.cTimeFormat}")}   {ev.Title}\n\n";
                    }
                }
            }

            lblCalendarEvents.Text = cCalendarEvents;
        }
        catch (Exception ex) when (ex is ObjectDisposedException)
        {
            // ObjectDisposedException: Cannot access a disposed object.
            // Happens when there are no events in the selected calendar or between the startDate and endDate.
            lblCalendarEvents.Text = "";
        }
        catch (Exception ex)
        {
            var properties = new Dictionary<string, string> {
                { "File:", "MainPage.xaml.cs" },
                { "Method:", "LoadEvents" },
                { "CalendarStore:", "GetEvents" },
                { "AppLanguage:", Globals.cLanguage }
            };
            Crashes.TrackError(ex, properties);
            
            await DisplayAlert(CalEventLang.ErrorTitle_Text, $"{CalEventLang.ErrorCalendar_Text}\n\n{ex.Message}", CalEventLang.ButtonClose_Text);
        }
    }

    // Clear the calendar events.
    private void OnClearEventsClicked(object sender, EventArgs e)
    {
        lblCalendarEvents.Text = "";

        _ = entSearchWord.Focus();
    }

    // Copy calendar events to clipboard.
    private async void OnClipboardButtonClicked(object sender, EventArgs e)
    {
        if (lblCalendarEvents.Text is not null and not "")
        {
            await Clipboard.Default.SetTextAsync(lblCalendarEvents.Text);
        }
    }
        
    // Share calendar events.
    private async void OnButtonShareClicked(object sender, EventArgs e)
    {
        if (lblCalendarEvents.Text is not null and not "")
        {
            await ShareText(lblCalendarEvents.Text);
        }
    }

    // Share calendar events.
    private static async Task ShareText(string cText)
    {
        await Share.Default.RequestAsync(new ShareTextRequest
        {
            Text = cText,
            Title = CalEventLang.NameProgramLocal_Text
        });
    }

    // Put text in the chosen language in the controls.
    private void SetTextLanguage()
    {
        // Set the CurrentUICulture.
        Globals.SetCultureSelectedLanguage();

        cCopyright = $"{CalEventLang.Copyright_Text} © 2023-2023 Geert Geerits";
        cLicenseText = $"{CalEventLang.License_Text}\n\n{CalEventLang.LicenseMit2_Text}";

        // Local name for 'All calendars' in calendarDictionary and calendar picker.
        if (Globals.bLanguageChanged)
        {
            // Local language name for 'All calendars' (first item in the calendarDictionary, calendarList and calendar picker).
            calendarDictionary[cDicKeyAllCalendars] = CalEventLang.AllCalendars_Text;

            // Put the calendars from the calendarDictionary via the calendarList in the picker.
            int nSelectedIndex = pckCalendars.SelectedIndex;

            List<string> calendarList = calendarDictionary.Values.ToList();
            pckCalendars.ItemsSource = calendarList;

            pckCalendars.SelectedIndex = nSelectedIndex;
        }
    }

    // Show license using the Loaded event of the MainPage.xaml.
    private async void OnPageLoaded(object sender, EventArgs e)
    {
        // Show license.
        if (Globals.bLicense == false)
        {
            Globals.bLicense = await Application.Current.MainPage.DisplayAlert(CalEventLang.LicenseTitle_Text, $"Calendar Events\n{cCopyright}\n\n{cLicenseText}", CalEventLang.Agree_Text, CalEventLang.Disagree_Text);

            if (Globals.bLicense)
            {
                Preferences.Default.Set("SettingLicense", true);
            }
            else
            {
#if IOS
                //Thread.CurrentThread.Abort();  // Not allowed in iOS.
                imgbtnAbout.IsEnabled = false;
                imgbtnSettings.IsEnabled= false;
                btnGetEvents.IsEnabled = false;
                btnClearEvents.IsEnabled = false;
                btnCopyEvents.IsEnabled = false;
                btnShareEvents.IsEnabled = false;

                await DisplayAlert(CalEventLang.LicenseTitle_Text, CalEventLang.CloseApplication_Text, CalEventLang.ButtonClose_Text);
#else
                Application.Current.Quit();
#endif
            }
        }

        // Set focus to the first entry field (workaround for !!!BUG!!! ?).
        // Add in the header of the xaml page: 'Loaded="OnPageLoaded"'
        Task.Delay(500).Wait();
        entSearchWord.Focus();
    }

    // Set language and dates using the Appearing event of the MainPage.xaml.
    private void OnPageAppearing(object sender, EventArgs e)
    {
        // Set language.
        if (Globals.bLanguageChanged)
        {
            SetTextLanguage();
            Globals.bLanguageChanged = false;
        }

        // Set the date format property.
        dtpDateStart.Format = Globals.cDateFormatDatePicker;
        dtpDateEnd.Format = Globals.cDateFormatDatePicker;

        // Set the calendar days in the past and in the future.
        dtpDateStart.Date = DateTime.Today.Date.AddDays(Convert.ToInt32(Globals.cAddDaysToStart));
        dtpDateEnd.Date = DateTime.Today.Date.AddDays(Convert.ToInt32(Globals.cAddDaysToEnd));

        // Set the language of the text to speech in the label.
        lblTextToSpeech.Text = GetIsoLanguageCode();

        // Set focus to the first entry field.
        entSearchWord.Focus();
    }

    // Permissions for CalendarRead - Sometimes permission is not given in Android (not yet tested in iOS).
    public async Task<PermissionStatus> CheckAndRequestCalendarRead()
    {
        PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.CalendarRead>();
        
        if (status == PermissionStatus.Granted)
            return status;

        // !!!BUG!!! in Android?: does not works without the DisplayAlert.
        await DisplayAlert("", CalEventLang.CalendarPermission_Text, CalEventLang.ButtonClose_Text);

        status = await Permissions.RequestAsync<Permissions.CalendarRead>();
        //await DisplayAlert("CheckAndRequestCalendarRead", status.ToString(), "OK");  // For testing.

        return status;
    }

    // Initialize text to speech and fill the the array with the speech languages.
    // .Country = KR ; .Id = ''  ; .Language = ko ; .Name = Korean (South Korea) ; 
    private async void InitializeTextToSpeech(string cCultureName)
    {
        // Initialize text to speech.
        int nTotalItems;

        try
        {
            locales = await TextToSpeech.Default.GetLocalesAsync();

            nTotalItems = locales.Count();

            if (nTotalItems == 0)
            {
                return;
            }
        }
        catch (Exception ex)
        {
            var properties = new Dictionary<string, string> {
                { "File:", "MainPage.xaml.cs" },
                { "Method:", "InitializeTextToSpeech" },
                { "AppLanguage:", Globals.cLanguage },
                { "AppLanguageSpeech:", Globals.cLanguageSpeech }
            };
            Crashes.TrackError(ex, properties);

            await DisplayAlert(CalEventLang.ErrorTitle_Text, $"{ex.Message}\n\n{CalEventLang.TextToSpeechError_Text}", CalEventLang.ButtonClose_Text);
            return;
        }

        lblTextToSpeech.IsVisible = true;
        imgbtnTextToSpeech.IsVisible = true;
        Globals.bLanguageLocalesExist = true;

        // Put the locales in the array and sort the array.
        Globals.cLanguageLocales = new string[nTotalItems];
        int nItem = 0;

        foreach (var l in locales)
        {
            Globals.cLanguageLocales[nItem] = $"{l.Language}-{l.Country} {l.Name}";
            nItem++;
        }

        Array.Sort(Globals.cLanguageLocales);

        // Search for the language after a first start or reset of the application.
        if (string.IsNullOrEmpty(Globals.cLanguageSpeech))
        {
            SearchArrayWithSpeechLanguages(cCultureName);
        }
        //await DisplayAlert("Globals.cLanguageSpeech", Globals.cLanguageSpeech, "OK");  // For testing.

        lblTextToSpeech.Text = GetIsoLanguageCode();
    }

    // Search for the language after a first start or reset of the application.
    private void SearchArrayWithSpeechLanguages(string cCultureName)
    {
        try
        {
            int nTotalItems = Globals.cLanguageLocales.Length;

            for (int nItem = 0; nItem < nTotalItems; nItem++)
            {
                if (Globals.cLanguageLocales[nItem].StartsWith(cCultureName))
                {
                    Globals.cLanguageSpeech = Globals.cLanguageLocales[nItem];
                    break;
                }
            }

            // If the language is not found try it with the language (Globals.cLanguage) of the user setting for this app.
            if (string.IsNullOrEmpty(Globals.cLanguageSpeech))
            {
                for (int nItem = 0; nItem < nTotalItems; nItem++)
                {
                    if (Globals.cLanguageLocales[nItem].StartsWith(Globals.cLanguage))
                    {
                        Globals.cLanguageSpeech = Globals.cLanguageLocales[nItem];
                        break;
                    }
                }
            }

            // If the language is still not found use the first language in the array.
            if (string.IsNullOrEmpty(Globals.cLanguageSpeech))
            {
                Globals.cLanguageSpeech = Globals.cLanguageLocales[0];
            }
        }
        catch (Exception ex)
        {
            Crashes.TrackError(ex);
            DisplayAlert(CalEventLang.ErrorTitle_Text, ex.Message, CalEventLang.ButtonClose_Text);
        }
    }

    // Button text to speech event.
    private async void OnTextToSpeechClicked(object sender, EventArgs e)
    {
        // Cancel the text to speech.
        if (bTextToSpeechIsBusy)
        {
            if (cts?.IsCancellationRequested ?? true)
                return;

            cts.Cancel();
            imgbtnTextToSpeech.Source = "speaker_64p_blue_green.png";
            return;
        }

        // Start with the text to speech.
        //lblCalendarEvents.Text = "Test";
        if (lblCalendarEvents.Text != null && lblCalendarEvents.Text != "")
        {
            bTextToSpeechIsBusy = true;
            imgbtnTextToSpeech.Source = "speaker_cancel_64p_blue_red.png";

            try
            {
                cts = new CancellationTokenSource();

                SpeechOptions options = new()
                {
                    Locale = locales.Single(l => $"{l.Language}-{l.Country} {l.Name}" == Globals.cLanguageSpeech)
                };

                await TextToSpeech.Default.SpeakAsync(lblCalendarEvents.Text, options, cancelToken: cts.Token);
                bTextToSpeechIsBusy = false;
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
                await DisplayAlert(CalEventLang.ErrorTitle_Text, ex.Message, CalEventLang.ButtonClose_Text);
            }

            imgbtnTextToSpeech.Source = "speaker_64p_blue_green.png";
        }
    }

    // Get ISO language (and country) code from locales.
    public static string GetIsoLanguageCode()
    {
        // Split before first space and remove last character '-' if there.
        string cLanguageIso = Globals.cLanguageSpeech.Split(' ').First();

        if (cLanguageIso.EndsWith('-'))
        {
            cLanguageIso = cLanguageIso.Remove(cLanguageIso.Length - 1, 1);
        }

        return cLanguageIso;
    }

    // Crash log confirmation.
    private async void ConfirmationSendCrashLog()
    {
        // Using the DisplayActionSheet with 3 choices.
        string cAction = await DisplayActionSheet(CalEventLang.LogTitle2_Text, null, null, CalEventLang.LogSend_Text, CalEventLang.LogAlwaysSend_Text, CalEventLang.LogDontSend_Text);

        if (cAction == CalEventLang.LogSend_Text)
        {
            Crashes.NotifyUserConfirmation(UserConfirmation.Send);
        }
        else if (cAction == CalEventLang.LogAlwaysSend_Text)
        {
            Crashes.NotifyUserConfirmation(UserConfirmation.AlwaysSend);
            Preferences.Default.Set("SettingLogAlwaysSend", true);
        }
        else if (cAction == CalEventLang.LogDontSend_Text)
        {
            Crashes.NotifyUserConfirmation(UserConfirmation.DontSend);
        }

        // Using the DisplayAlert with 2 choices.
        //bool bAction = await DisplayAlert(CodeLang.LogTitle_Text, CodeLang.LogMessage_Text, CodeLang.LogSend_Text, CodeLang.LogDontSend_Text);

        //if (bAction)
        //{
        //    Crashes.NotifyUserConfirmation(UserConfirmation.Send);
        //}
        //else
        //{
        //    Crashes.NotifyUserConfirmation(UserConfirmation.DontSend);
        //}
    }
}