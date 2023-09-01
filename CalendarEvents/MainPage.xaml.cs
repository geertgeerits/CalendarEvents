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
            var calendars = await CalendarStore.Default.GetCalendars();

            foreach (var calendar in calendars)
            {
                await DisplayAlert("Calendars", $"{calendar.Name} ({calendar.Id})", "OK");
            }

            var events = await CalendarStore.Default.GetEvents(startDate: DateTimeOffset.Now.AddDays(-1), endDate: DateTimeOffset.Now.AddDays(100));

            foreach (var ev in events)
            {
                await DisplayAlert("Events", $"{ev.Title} ({ev.Id}) Start: {ev.StartDate}", "OK");
            }
        }
    }
}