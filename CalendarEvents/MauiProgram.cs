using Plugin.Maui.CalendarStore;
using Microsoft.Extensions.Logging;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Crashes;

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