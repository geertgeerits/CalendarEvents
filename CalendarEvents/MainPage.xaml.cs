// Program .....: CalendarEvents.sln
// Author ......: Geert Geerits - E-mail: geertgeerits@gmail.com
// Copyright ...: (C) 2023-2023
// Version .....: 1.0.3
// Date ........: 2023-09-04 (YYYY-MM-DD)
// Language ....: Microsoft Visual Studio 2022: .NET 7.0 MAUI C# 11.0
// Description .: Read calendar events to share
// Dependencies : NuGet Package: Plugin.Maui.CalendarStore version 1.0.0-preview2 ; https://github.com/jfversluis/Plugin.Maui.CalendarStore
//                NuGet Package: Microsoft.AppCenter version 5.0.2 ; https://appcenter.ms/apps ; https://azure.microsoft.com/en-us/products/app-center/
//                NuGet Package: Microsoft.AppCenter.Crashes version 5.0.2 
// Thanks to ...: Gerald Versluis

using Plugin.Maui.CalendarStore;

namespace CalendarEvents;

public partial class MainPage : ContentPage
{
    // Local variables.
    private string cCopyright;
    private string cLicenseText;
    private readonly bool bLicense;

    public MainPage()
    {
        InitializeComponent();

        // Get the saved settings.License
        Globals.cTheme = Preferences.Default.Get("SettingTheme", "System");
        Globals.bDateFormatSystem = Preferences.Default.Get("SettingDateFormatSystem", true);
        Globals.cLanguage = Preferences.Default.Get("SettingLanguage", "");
        bLicense = Preferences.Default.Get("SettingLicense", false);

        // Set the theme.
        if (Globals.cTheme == "Light")
        {
            Application.Current.UserAppTheme = AppTheme.Light;
        }
        else if (Globals.cTheme == "Dark")
        {
            Application.Current.UserAppTheme = AppTheme.Dark;
        }
        else
        {
            Application.Current.UserAppTheme = AppTheme.Unspecified;
        }

        // Get the system date format and set the date format.
        Globals.cSysDateFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;

        if (Globals.bDateFormatSystem == true)
        {
            Globals.cDateFormat = Globals.cSysDateFormat;
        }
        else
        {
            Globals.cDateFormat = "yyyy-MM-dd";
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

        SetTextLanguage();
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

    // Go to the next field when the return key have been pressed.
    private void GoToNextField(object sender, EventArgs e)
    {
        if (sender == entSearchWord)
        {
            entNumDaysPast.Focus();
        }
        else if (sender == entNumDaysPast)
        {
            entNumDaysFuture.Focus();
        }
        else if (sender == entNumDaysFuture)
        {
            btnGetEvents.Focus();
        }
    }

    // Get calendar events.
    private async void OnGetEventsClicked(object sender, EventArgs e)
    {
        // Validate input values.
        bool bIsNumber = int.TryParse(entNumDaysPast.Text, out int nNumDaysPast);
        if (bIsNumber == false || nNumDaysPast < 0 || nNumDaysPast > 4000)
        {
            entNumDaysPast.Text = "";
            entNumDaysPast.Focus();
            return;
        }

         bIsNumber = int.TryParse(entNumDaysFuture.Text, out int nNumDaysFuture);
        if (bIsNumber == false || nNumDaysFuture < 0 || nNumDaysFuture > 4000)
        {
            entNumDaysFuture.Text = "";
            entNumDaysFuture.Focus();
            return;
        }

        // Close the keyboard.
        entNumDaysFuture.IsEnabled = false;
        entNumDaysFuture.IsEnabled = true;

        // Get the UTC offset.
        //string cUtcOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).ToString()[..5];
        //double nUtcOffset = Convert.ToDouble(TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalHours);

        // Get all the calendars from the device.
        var calendars = await CalendarStore.Default.GetCalendars();

        //DisplayAlert("", Convert.ToString(calendars.Length), "OK");
        //string[,] calendarArray = new string[calendars.Length, 2];

        //for (int i = 0; i < calendars.Length; i++)
        //{
        //    calendarArray[i, 0] = calendars[i].Name;
        //    calendarArray[i, 1] = calendars[i].Id.ToString();
        //}

        string cCalendarNames = "Calendars: ";

        foreach (var calendar in calendars)
        {
            cCalendarNames = $"{cCalendarNames} {calendar.Name} ({calendar.Id}), ";
        }
        
        lblCalendarNames.Text = cCalendarNames;
        //brdCalendarNames.IsVisible = true;
        //lblCalendarNames.IsVisible = true;

        // Get (all) the events from the calendar.
        var events = await CalendarStore.Default.GetEvents(startDate: DateTimeOffset.UtcNow.AddDays(-nNumDaysPast), endDate: DateTimeOffset.UtcNow.AddDays(nNumDaysFuture));

        string cCalendarEvents = "";
        //string cStartDate;
        //DateTime dStartDate;
        string cNewLine = "\n\n";

        if (entSearchWord.Text is null or "")
        {
            foreach (CalendarEvent ev in events)
            {
                //cCalendarEvents = $"{cCalendarEvents}{ev.StartDate}, {ev.Title}\n";
                cCalendarEvents = $"{cCalendarEvents}{ev.StartDate.ToString(Globals.cDateFormat + "  HH:mm")}  {ev.Title}{cNewLine}";
            }

            //foreach (CalendarEvent ev in events)
            //{
            //    if (Convert.ToString(ev.StartDate).Contains("+00:00"))
            //    {
            //        dStartDate = DateTime.Parse(Convert.ToString(ev.StartDate));
            //        //cStartDate = dStartDate.ToString(Globals.cDateFormat + " HH:mm zzz");
            //        cStartDate = dStartDate.ToString(Globals.cDateFormat + "  HH:mm");
            //        cCalendarEvents = $"{cCalendarEvents}{cStartDate}  {ev.Title}{cNewLine}";
            //    }
            //    else
            //    {
            //        //cCalendarEvents = $"{cCalendarEvents}{ev.StartDate}, {ev.Title}\n";
            //        cCalendarEvents = $"{cCalendarEvents}{ev.StartDate.ToString(Globals.cDateFormat + "  HH:mm")}  {ev.Title}{cNewLine}";
            //    }
            //}
        }
        else
        {
            string cSearchWord = entSearchWord.Text.ToLower().Trim();

            foreach (CalendarEvent ev in events)
            {
                if (ev.Title.ToLower().Contains(cSearchWord))
                {
                    //cCalendarEvents = $"{cCalendarEvents}{ev.StartDate}, {ev.Title}\n";
                    cCalendarEvents = $"{cCalendarEvents}{ev.StartDate.ToString(Globals.cDateFormat + "  HH:mm")}  {ev.Title}{cNewLine}";

                    //if (Convert.ToString(ev.StartDate).Contains("+00:00"))
                    //{
                    //    dStartDate = DateTime.Parse(Convert.ToString(ev.StartDate));
                    //    //cStartDate = dStartDate.ToString(Globals.cDateFormat + " HH:mm zzz");
                    //    cStartDate = dStartDate.ToString(Globals.cDateFormat + "  HH:mm");
                    //    cCalendarEvents = $"{cCalendarEvents}{cStartDate}  {ev.Title}{cNewLine}";
                    //}
                    //else
                    //{
                    //    //cCalendarEvents = $"{cCalendarEvents}{ev.StartDate}, {ev.Title}\n";
                    //    cCalendarEvents = $"{cCalendarEvents}{ev.StartDate.ToString(Globals.cDateFormat + "  HH:mm")}  {ev.Title}{cNewLine}";
                    //}
                }
            }
        }

        lblCalendarEvents.Text = cCalendarEvents;
    }

    // Clear the calendar events.
    private void OnClearEventsClicked(object sender, EventArgs e)
    {
        lblCalendarNames.Text = "";
        lblCalendarEvents.Text = "";
    }

    // Copy calendar events to clipboard.
    private async void OnClipboardButtonClicked(object sender, EventArgs e) =>
        await Clipboard.Default.SetTextAsync(lblCalendarEvents.Text);   

    // Share calendar events.
    private async void OnButtonShareClicked(object sender, EventArgs e)
    {
        if (lblCalendarEvents.Text is not null and not "")
        {
            await ShareText(lblCalendarEvents.Text);
        }
    }

    // Share calendar events.
    private async Task ShareText(string cText)
    {
        await Share.Default.RequestAsync(new ShareTextRequest
        {
            Text = cText,
            Title = "Share calendar events"
        });
    }

    // Put text in the chosen language in the controls.
    private void SetTextLanguage()
    {
        // Set the CurrentUICulture.
        //Globals.cLanguage = "es";  // For testing.
        //App.Current.MainPage.DisplayAlert("Globals.cLanguage", Globals.cLanguage, "OK");  // For testing.

        Globals.SetCultureSelectedLanguage();

        cCopyright = $"{CalEventLang.Copyright_Text} © 2023-2023 Geert Geerits";
        cLicenseText = $"{CalEventLang.License_Text}\n\n{CalEventLang.LicenseMit2_Text}";

        //App.Current.MainPage.DisplayAlert(CalEventLang.ErrorTitle_Text, Globals.cLanguage, cButtonCloseText);  // For testing.
    }

    // Show license using the Loaded event of the MainPage.xaml.
    private async void OnPageLoaded(object sender, EventArgs e)
    {
        // Show license.
        if (bLicense == false)
        {
            bool bAnswer = await Application.Current.MainPage.DisplayAlert(CalEventLang.LicenseTitle_Text, $"Calendar Events\n{cCopyright}\n\n{cLicenseText}", CalEventLang.Agree_Text, CalEventLang.Disagree_Text);

            if (bAnswer)
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

    // Set language using the Appearing event of the MainPage.xaml.
    private void OnPageAppearing(object sender, EventArgs e)
    {
        if (Globals.bLanguageChanged)
        {
            SetTextLanguage();
            Globals.bLanguageChanged = false;

            //DisplayAlert("Globals.bLanguageChanged", "true", "OK");  // For testing.
        }
    }
}