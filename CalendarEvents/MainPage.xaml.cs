// Program .....: CalendarEvents.sln
// Author ......: Geert Geerits - E-mail: geertgeerits@gmail.com
// Copyright ...: (C) 2023-2023
// Version .....: 1.0.1
// Date ........: 2023-09-01 (YYYY-MM-DD)
// Language ....: Microsoft Visual Studio 2022: .NET 7.0 MAUI C# 11.0
// Description .: Read calendar events to share
// Dependencies : NuGet Package: Plugin.Maui.CalendarStore by Gerald Versluis ; https://github.com/jfversluis/Plugin.Maui.CalendarStore
//                NuGet Package: Microsoft.AppCenter version 5.0.2 ; https://appcenter.ms/apps ; https://azure.microsoft.com/en-us/products/app-center/
//                NuGet Package: Microsoft.AppCenter.Crashes version 5.0.2 
// Thanks to ...: Gerald Versluis

using Plugin.Maui.CalendarStore;
using System.Numerics;

namespace CalendarEvents
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        // Get calendar events.
        private async void OnGetCalendarEventsClicked(object sender, EventArgs e)
        {
            // Validate input values.
            bool bIsNumber = int.TryParse(entNumDaysPast.Text, out int nNumDaysPast);
            if (bIsNumber == false || nNumDaysPast < 0 || nNumDaysPast > 400)
            {
                entNumDaysPast.Text = "";
                entNumDaysPast.Focus();
                return;
            }

             bIsNumber = int.TryParse(entNumDaysFuture.Text, out int nNumDaysFuture);
            if (bIsNumber == false || nNumDaysFuture < 0 || nNumDaysFuture > 400)
            {
                entNumDaysFuture.Text = "";
                entNumDaysFuture.Focus();
                return;
            }


            // Get all the calendars from the device.
            var calendars = await CalendarStore.Default.GetCalendars();

            //foreach (var calendar in calendars)
            //{
            //    await DisplayAlert("Calendars", $"{calendar.Name} ({calendar.Id})", "OK");
            //}

            // Get (all) the events from the calendar.
            var events = await CalendarStore.Default.GetEvents(startDate: DateTimeOffset.Now.AddDays(- nNumDaysPast), endDate: DateTimeOffset.Now.AddDays(nNumDaysFuture));
            
            string cCalendarEvents = "";

            if (entSearchWord.Text is null or "")
            {
                foreach (CalendarEvent ev in events)
                {
                    cCalendarEvents = $"{cCalendarEvents}\n{ev.StartDate}, {ev.Title}";
                }
            }
            else
            {
                foreach (CalendarEvent ev in events)
                {
                    if (ev.Title.ToLower().Contains(entSearchWord.Text.ToLower()))
                    {
                        cCalendarEvents = $"{cCalendarEvents}{ev.StartDate}, {ev.Title}\n";
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
    }
}