using Microsoft.Extensions.Logging;
using Microsoft.AppCenter;
using Microsoft.Maui.LifecycleEvents;

namespace CalendarEvents
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })

                .ConfigureLifecycleEvents(events =>
                {
#if ANDROID
                    events.AddAndroid(android => android
                        //.OnStop((activity) => ProcessEvent(nameof(AndroidLifecycle.OnStop)))
                        .OnPause((activity) => ProcessEvent(nameof(AndroidLifecycle.OnPause))));
#endif

#if IOS
                    events.AddiOS(ios => ios
                        .OnResignActivation((app) => ProcessEvent(nameof(iOSLifecycle.OnResignActivation)))
                        .WillTerminate((app) => ProcessEvent(nameof(iOSLifecycle.WillTerminate))));
#endif

                    //#if WINDOWS
                    //                    events.AddWindows(windows => windows
                    //                        .OnClosed((window, args) => ProcessEvent(nameof(WindowsLifecycle.OnClosed))));
                    //#endif

                    static bool ProcessEvent (string eventName, string type = null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Lifecycle event: {eventName}{(type == null ? string.Empty : $" ({type})")}");

                        // Cancel speech if a cancellation token exists & hasn't been already requested.
                        if (Globals.bTextToSpeechIsBusy)
                        {
                            if (Globals.cts?.IsCancellationRequested ?? true)
                                return true;

                            Globals.cts.Cancel();
                            Globals.bTextToSpeechIsBusy = false;
                        }
                        return true;
                    }
                });

            AppCenter.Start("windowsdesktop=c5823557-6d76-44bb-a13a-40a375905c14;" +
            "android=9a9b413c-f1f3-4b6a-a78c-41ab8317b675;" +
            "ios=1b9b77a2-6260-4b72-8344-a120c1e36572;" +
            "macos={Your macOS App secret here};",
            typeof(Crashes));

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}