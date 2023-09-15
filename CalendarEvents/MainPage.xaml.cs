﻿// Program .....: CalendarEvents.sln
// Author ......: Geert Geerits - E-mail: geertgeerits@gmail.com
// Copyright ...: (C) 2023-2023
// Version .....: 1.0.3
// Date ........: 2023-09-15 (YYYY-MM-DD)
// Language ....: Microsoft Visual Studio 2022: .NET 7.0 MAUI C# 11.0
// Description .: Read calendar events to share
// Dependencies : NuGet Package: Plugin.Maui.CalendarStore version 1.0.0-preview4 ; https://github.com/jfversluis/Plugin.Maui.CalendarStore
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
    private readonly bool bLogAlwaysSend;

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
        Globals.bDateFormatSystem = Preferences.Default.Get("SettingDateFormatSystem", true);
        Globals.cAddDaysToStart = Preferences.Default.Get("SettingAddDaysToStart", "0");
        Globals.cAddDaysToEnd = Preferences.Default.Get("SettingAddDaysToEnd", "31");
        Globals.cLanguage = Preferences.Default.Get("SettingLanguage", "");
        bLicense = Preferences.Default.Get("SettingLicense", false);
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

        // Set the date properties for the DatePicker.
        dtpDateStart.MinimumDate = new DateTime(1583, 1, 1);
        dtpDateStart.MaximumDate = new DateTime(3000, 1, 1);
        dtpDateEnd.MinimumDate = new DateTime(1583, 1, 1);
        dtpDateEnd.MaximumDate = new DateTime(3000, 1, 1);

        // Set the text language.
        SetTextLanguage();

        // Set up the grid for the different platforms due a
        // !!!BUG!!! in Windows with the grid style on the MainPage.xaml: there is only 1 column.
#if ANDROID || IOS
        var grid = new Grid()
        {
            Style = (Style)Application.Current.Resources["gridStyleEvents"],
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
            },           
        };
        grdEvents.Style = grid.Style;
        grdEvents.RowDefinitions = grid.RowDefinitions;        
#else
        var grid = new Grid()
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(400) },
                new ColumnDefinition { Width = new GridLength(400) }
            },
            HorizontalOptions = LayoutOptions.Center,
            ColumnSpacing = 15,
            RowSpacing = 4,
            Margin = new Thickness(10,10,10,10)
        };
        grdEvents.RowDefinitions = grid.RowDefinitions;
        grdEvents.HorizontalOptions = grid.HorizontalOptions;
        grdEvents.ColumnDefinitions = grid.ColumnDefinitions;
        grdEvents.ColumnSpacing = grid.ColumnSpacing;
        grdEvents.RowSpacing = grid.RowSpacing;
        grdEvents.Margin = grid.Margin;
#endif
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

    // Get calendar events.
    private async void OnGetEventsClicked(object sender, EventArgs e)
    {
        // Validate the date values.
        if (dtpDateStart.Date > dtpDateEnd.Date)
        {
            await DisplayAlert(CalEventLang.ErrorTitle_Text, CalEventLang.ErrorDate_Text, CalEventLang.ButtonClose_Text);
            _ = dtpDateStart.Focus();
            return;
        }

        // Close the keyboard.
        entSearchWord.IsEnabled = false;
        entSearchWord.IsEnabled = true;

        // Get all the calendars from the device.
        string cCalendarNames;
        
        try
        {
            // For testing crashes - DivideByZeroException.
            //int divByZero = 51 / int.Parse("0");

            var calendars = await CalendarStore.Default.GetCalendars();

            cCalendarNames = CalEventLang.Calendars_Text;

            foreach (var calendar in calendars)
            {
                //cCalendarNames = $"{cCalendarNames} {calendar.Name} ({calendar.Id}), ";
                cCalendarNames = $"{cCalendarNames} {calendar.Name}, ";
            }
        }
        catch (Exception ex)
        {
            var properties = new Dictionary<string, string> {
                { "File:", "MainPage.xaml.cs" },
                { "Method:", "OnGetEventsClicked" },
                { "CalendarStore:", "GetCalendars" },
                { "AppLanguage:", Globals.cLanguage }                
            };
            Crashes.TrackError(ex, properties);

            await DisplayAlert(CalEventLang.ErrorTitle_Text, ex.Message, CalEventLang.ButtonClose_Text);
            return;
        }
        
        lblCalendarNames.Text = cCalendarNames;

        // !!!BUG!!! Workaround for timezone not added to the datetime. Solved with 'Plugin.Maui.CalendarStore version 1.0.0-preview4'.
        //GetEventsTimezone(sender, e);
        //return;

        // Get (all) the events from the calendar.
        string cCalendarEvents = "";

        try
        {
            var events = await CalendarStore.Default.GetEvents(startDate: dtpDateStart.Date, endDate: dtpDateEnd.Date.AddDays(1));

            if (entSearchWord.Text is null or "")
            {
                foreach (CalendarEvent ev in events)
                {
                    cCalendarEvents = $"{cCalendarEvents}{ev.StartDate.ToString(Globals.cDateFormat + "  HH:mm")}  {ev.Title}\n\n";
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
                        cCalendarEvents = $"{cCalendarEvents}{ev.StartDate.ToString(Globals.cDateFormat + "  HH:mm")}  {ev.Title}\n\n";
                    }
                }
            }
        }
        catch (Exception ex)
        {
            var properties = new Dictionary<string, string> {
                { "File:", "MainPage.xaml.cs" },
                { "Method:", "OnGetEventsClicked" },
                { "CalendarStore:", "GetEvents" },
                { "AppLanguage:", Globals.cLanguage }
            };
            Crashes.TrackError(ex, properties);

            await DisplayAlert(CalEventLang.ErrorTitle_Text, ex.Message, CalEventLang.ButtonClose_Text);
            return;
        }

        lblCalendarEvents.Text = cCalendarEvents;
    }

    // Clear the calendar events.
    private void OnClearEventsClicked(object sender, EventArgs e)
    {
        lblCalendarNames.Text = "";
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
        dtpDateStart.Format = Globals.cDateFormat;
        dtpDateEnd.Format = Globals.cDateFormat;

        // Set the calendar days in the past and in the future.
        dtpDateStart.Date = DateTime.Today.Date.AddDays(Convert.ToInt32(Globals.cAddDaysToStart));
        dtpDateEnd.Date = DateTime.Today.Date.AddDays(Convert.ToInt32(Globals.cAddDaysToEnd));

        // Set focus to the first entry field.
        entSearchWord.Focus();
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

    // !!!BUG!!! Workaround for Workaround for timezone not added to the datetime.
    // Get(all) the events from the calendar.
    //private async void GetEventsTimezone(object sender, EventArgs e)
    //{
    //    // Get (all) the events from the calendar.
    //    string cCalendarEvents = "";
    //    DateTime dStartDate;

    //    try
    //    {
    //        var events = await CalendarStore.Default.GetEvents(startDate: dtpDateStart.Date, endDate: dtpDateEnd.Date.AddDays(1));

    //        if (entSearchWord.Text is null or "")
    //        {
    //            foreach (CalendarEvent ev in events)
    //            {
    //                if (Convert.ToString(ev.StartDate).Contains("+00:00"))
    //                {
    //                    dStartDate = DateTime.Parse(Convert.ToString(ev.StartDate));
    //                    cCalendarEvents = $"{cCalendarEvents}{dStartDate.ToString(Globals.cDateFormat + "  HH:mm")}  {ev.Title}\n\n";
    //                }
    //                else
    //                {
    //                    cCalendarEvents = $"{cCalendarEvents}{ev.StartDate.ToString(Globals.cDateFormat + "  HH:mm")}  {ev.Title}\n\n";
    //                }
    //            }
    //        }
    //        else
    //        {
    //            string cSearchWord = entSearchWord.Text.ToLowerInvariant().Trim();

    //            foreach (CalendarEvent ev in events)
    //            {
    //                if (ev.Title is null or "")
    //                {
    //                    continue;
    //                }
                    
    //                if (ev.Title.ToLowerInvariant().Contains(cSearchWord))
    //                {
    //                    if (Convert.ToString(ev.StartDate).Contains("+00:00"))
    //                    {
    //                        dStartDate = DateTime.Parse(Convert.ToString(ev.StartDate));
    //                        cCalendarEvents = $"{cCalendarEvents}{dStartDate.ToString(Globals.cDateFormat + "  HH:mm")}  {ev.Title}\n\n";
    //                    }
    //                    else
    //                    {
    //                        cCalendarEvents = $"{cCalendarEvents}{ev.StartDate.ToString(Globals.cDateFormat + "  HH:mm")}  {ev.Title}\n\n";
    //                    }
    //                }
    //            }
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        var properties = new Dictionary<string, string> {
    //            { "File:", "MainPage.xaml.cs" },
    //            { "Method:", "GetEventsTimezone" },
    //            { "CalendarStore:", "GetEvents" },
    //            { "AppLanguage:", Globals.cLanguage }
    //        };
    //        Crashes.TrackError(ex, properties);

    //        await DisplayAlert(CalEventLang.ErrorTitle_Text, ex.Message, CalEventLang.ButtonClose_Text);
    //        return;
    //    }

    //    lblCalendarEvents.Text = cCalendarEvents;
    //}
}