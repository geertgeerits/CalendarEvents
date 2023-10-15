namespace CalendarEvents;

public partial class PageWebsite : ContentPage
{
    public PageWebsite()
    {
        try
        {
            InitializeComponent();
        }
        catch (Exception ex)
        {
            Crashes.TrackError(ex);
            DisplayAlert("InitializeComponent: PageWebsite", ex.Message, "OK");
            return;
        }

        // Set WebView properties.
        wvWebpage.Source = "https://geertgeerits.wixsite.com/geertgeerits/calendar-events";
        wvWebpage.Navigating += OnNavigating;
        wvWebpage.Navigated += OnNavigated;

        // CanGoBack and CanGoForward !!!BUG!!! on Android and iOS !!!
        btnGoBack.IsEnabled = true;
        btnGoForward.IsEnabled = true;
    }

    // Navigating event that's raised when page navigation starts.
    private async void OnNavigating(object sender, WebNavigatingEventArgs e)
    {
        // If 'mailto' link in webpage then open the e-mail app.
        if (e.Url.StartsWith("mailto"))
        {
            await Launcher.TryOpenAsync(e.Url);
            e.Cancel = true;
        }
    }

    // Navigated event that's raised when page navigation completes.
    private void OnNavigated(object sender, WebNavigatedEventArgs e)
    {
        // Solved in .NET 8.  CanGoBack and CanGoForward !!!BUG!!! on Android and iOS !!!
        // Enable or disable the back and forward buttons.
        btnGoBack.IsEnabled = wvWebpage.CanGoBack;
        btnGoForward.IsEnabled = wvWebpage.CanGoForward;

        // Changes the target of all the links in _self.
        wvWebpage.EvaluateJavaScriptAsync(@"(function() {
            var links = document.getElementsByTagName('a');
            for (var i = 0; i < links.length; i++)
            {
                links[i].setAttribute('target', '_self');
            }
        })()");
    }

    // Go backwards, if allowed.
    private void OnGoBackClicked(object sender, EventArgs e)
    {
        if (wvWebpage.CanGoBack)
        {
            wvWebpage.GoBack();
        }
    }

    // Go forwards, if allowed.
    private void OnGoForwardClicked(object sender, EventArgs e)
    {
        if (wvWebpage.CanGoForward)
        {
            wvWebpage.GoForward();
        }
    }
}