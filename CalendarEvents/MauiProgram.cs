using Microsoft.Extensions.Logging;
using Microsoft.AppCenter;
//using Microsoft.Maui.LifecycleEvents;

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
                });

//                .ConfigureLifecycleEvents(events =>
//                {
//#if ANDROID
//                    events.AddAndroid(android => android
//                        .OnActivityResult((activity, requestCode, resultCode, data) => LogEvent(nameof(AndroidLifecycle.OnActivityResult), requestCode.ToString()))
//                        .OnStart((activity) => LogEvent(nameof(AndroidLifecycle.OnStart)))
//                        .OnCreate((activity, bundle) => LogEvent(nameof(AndroidLifecycle.OnCreate)))
//                        .OnBackPressed((activity) => LogEvent(nameof(AndroidLifecycle.OnBackPressed)) && false)
//                        .OnStop((activity) => LogEvent(nameof(AndroidLifecycle.OnStop)))
//                        .OnPause((activity) => LogEvent(nameof(AndroidLifecycle.OnPause))));
//#endif

//#if IOS
//                     events.AddiOS(ios => ios
//                         .OnActivated((app) => LogEvent(nameof(iOSLifecycle.OnActivated)))
//                         .OnResignActivation((app) => LogEvent(nameof(iOSLifecycle.OnResignActivation)))
//                         .DidEnterBackground((app) => LogEvent(nameof(iOSLifecycle.DidEnterBackground)))
//                         .WillTerminate((app) => LogEvent(nameof(iOSLifecycle.WillTerminate))));
//#endif

//#if WINDOWS
//                    events.AddWindows(windows => windows
//                           .OnActivated((window, args) => LogEvent(nameof(WindowsLifecycle.OnActivated)))
//                           .OnClosed((window, args) => LogEvent(nameof(WindowsLifecycle.OnClosed)))
//                           .OnLaunched((window, args) => LogEvent(nameof(WindowsLifecycle.OnLaunched)))
//                           .OnLaunching((window, args) => LogEvent(nameof(WindowsLifecycle.OnLaunching)))
//                           .OnVisibilityChanged((window, args) => LogEvent(nameof(WindowsLifecycle.OnVisibilityChanged)))
//                           .OnPlatformMessage((window, args) =>
//                           {
//                               if (args.MessageId == Convert.ToUInt32("031A", 16))
//                               {
//                                   // System theme has changed
//                               }
//                           }));
//#endif

//                    static bool LogEvent(string eventName, string type = null)
//                    {
//                        System.Diagnostics.Debug.WriteLine($"Lifecycle event: {eventName}{(type == null ? string.Empty : $" ({type})")}");
//                        //MainPage.CancelTextToSpeech();
//                        return true;
//                    }
//                });

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