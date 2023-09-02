// Program .....: CalendarEvents.sln
// Author ......: Geert Geerits - E-mail: geertgeerits@gmail.com
// Copyright ...: (C) 2023-2023
// Version .....: 1.0.1
// Date ........: 2023-09-02 (YYYY-MM-DD)
// Language ....: Microsoft Visual Studio 2022: .NET 7.0 MAUI C# 11.0
// Description .: Read calendar events to share
// Dependencies : NuGet Package: Plugin.Maui.CalendarStore by Gerald Versluis ; https://github.com/jfversluis/Plugin.Maui.CalendarStore
//                NuGet Package: Microsoft.AppCenter version 5.0.2 ; https://appcenter.ms/apps ; https://azure.microsoft.com/en-us/products/app-center/
//                NuGet Package: Microsoft.AppCenter.Crashes version 5.0.2 
// Thanks to ...: Gerald Versluis

using Plugin.Maui.CalendarStore;

namespace CalendarEvents;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    // Set focus to the first entry field (workaround for !!!BUG!!! ?).
    // Add in the header of the xaml page: 'Loaded="OnPageLoaded"'
    private void OnPageLoaded(object sender, EventArgs e)
    {
        entSearchWord.Focus();
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
            btnGetCalendarEvents.Focus();
        }
    }

    // Get calendar events.
    private async void OnGetCalendarEventsClicked(object sender, EventArgs e)
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
        string cUtcOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).ToString();
        //await DisplayAlert("", "UTC offset: " + cUtcOffset, "OK");
        int nUtcOffset = Convert.ToInt32(TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalHours);
        //await DisplayAlert("", "UTC offset: " + nUtcOffset, "OK");

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

        // Get (all) the events from the calendar.
        var events = await CalendarStore.Default.GetEvents(startDate: DateTimeOffset.UtcNow.AddDays(-nNumDaysPast), endDate: DateTimeOffset.UtcNow.AddDays(nNumDaysFuture));

        string cCalendarEvents = "";
        string cTemp;

        if (entSearchWord.Text is null or "")
        {
            foreach (CalendarEvent ev in events)
            {
                if (Convert.ToString(ev.StartDate).Contains("+00:00"))
                {
                    //cCalendarEvents = $"{cCalendarEvents}{ev.StartDate.AddHours(nUtcOffset)}, {ev.Title}\n";
                    cTemp = $"{ev.StartDate.AddHours(nUtcOffset)}";
                    cTemp = cTemp.Replace("+00:00", "+" + cUtcOffset[..5]);
                    cCalendarEvents = $"{cCalendarEvents}{cTemp}, {ev.Title}\n";
                }
                else
                {
                    cCalendarEvents = $"{cCalendarEvents}{ev.StartDate}, {ev.Title}\n";
                }
            }
        }
        else
        {
            string cSearchWord = entSearchWord.Text.ToLower().Trim();

            foreach (CalendarEvent ev in events)
            {
                if (ev.Title.ToLower().Contains(cSearchWord))
                {
                    if (Convert.ToString(ev.StartDate).Contains("+00:00"))
                    {
                        //cCalendarEvents = $"{cCalendarEvents}{ev.StartDate.AddHours(nUtcOffset)}, {ev.Title}\n";
                        cTemp = $"{ev.StartDate.AddHours(nUtcOffset)}";
                        cTemp = cTemp.Replace("+00:00", "+" + cUtcOffset[..5]);
                        cCalendarEvents = $"{cCalendarEvents}{cTemp}, {ev.Title}\n";
                    }
                    else
                    {
                        cCalendarEvents = $"{cCalendarEvents}{ev.StartDate}, {ev.Title}\n";
                    }
                }
            }
        }

        lblCalendarEvents.Text = cCalendarEvents;
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

    // Get the UTC offset.
    //private TimeSpan GetUtcOffset()
    //{
    //    TimeZoneInfo localZone = TimeZoneInfo.Local;
    //    DateTime currentDate = DateTime.Now;
    //    TimeSpan currentOffset;

    //    if (localZone.IsDaylightSavingTime(currentDate))
    //    {
    //        currentOffset = localZone.GetUtcOffset(currentDate);
    //    }
    //    else
    //    {
    //        currentOffset = localZone.BaseUtcOffset;
    //    }

    //    return currentOffset;
    //}
}