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
using static System.Net.Mime.MediaTypeNames;

namespace CalendarEvents
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnCounterClicked(object sender, EventArgs e)
        {
            // Get all the calendars from the device.
            var calendars = await CalendarStore.Default.GetCalendars();

            foreach (var calendar in calendars)
            {
                //await DisplayAlert("Calendars", $"{calendar.Name} ({calendar.Id})", "OK");
            }

            // Get (all) the events from the calendar.
            var events = await CalendarStore.Default.GetEvents(startDate: DateTimeOffset.Now.AddDays(-1), endDate: DateTimeOffset.Now.AddDays(100));
            
            string cCalendarEvents = "";

            //await DisplayAlert("Events", $"{ev.Title} ({ev.Id}) Start: {ev.StartDate}", "OK");

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
                        cCalendarEvents = $"{cCalendarEvents}\n{ev.StartDate}, {ev.Title}";
                    }
                }
            }

            lblCalendarEvents.Text = $"{cCalendarEvents}\n";

            await Clipboard.Default.SetTextAsync(lblCalendarEvents.Text);
        }

        private async void ButtonShare_Clicked(object sender, EventArgs e)
        {
            if (lblCalendarEvents.Text is not null and not "")
            {
                await ShareText(lblCalendarEvents.Text);
            }
        }

        private async Task ShareText(string text)
        {
            await Share.Default.RequestAsync(new ShareTextRequest
            {
                Text = text,
                Title = "Share calendar events"
            });
        }

    }
}