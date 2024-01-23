namespace CalendarEvents;

public partial class PageAbout : ContentPage
{
    public PageAbout()
	{
        try
        {
            InitializeComponent();
        }
        catch (Exception ex)
        {
            Crashes.TrackError(ex);
#if DEBUG
            DisplayAlert("InitializeComponent: PageAbout", ex.Message, "OK");
#endif
            return;
        }

        // Put text in the chosen language in the controls.
        lblVersion.Text = $"{CalEventLang.Version_Text} 1.0.9";
        lblCopyright.Text = $"{CalEventLang.Copyright_Text} © 2023-2024 Geert Geerits";
        lblEmail.Text = $"{CalEventLang.Email_Text} geertgeerits@gmail.com";
        lblWebsite.Text = $"{CalEventLang.Website_Text}: ../calendarevents";
        lblPrivacyPolicy.Text = $"\n{CalEventLang.PrivacyPolicyTitle_Text} {CalEventLang.PrivacyPolicy_Text}";
        lblCrashErrorReport.Text = $"\n{CalEventLang.CrashErrorReport_Text}";
        lblLicense.Text = $"\n{CalEventLang.LicenseTitle_Text}: {CalEventLang.License_Text}\n{CalEventLang.LicenseMit2_Text}";
        lblExplanation.Text = $"\n{CalEventLang.InfoExplanation_Text}";
    }

    // Open the e-mail program.
    private async void OnBtnEmailLinkClicked(object sender, EventArgs e)
    {
        if (Email.Default.IsComposeSupported)
        {
            string subject = "Calendar events";
            string body = "";
            string[] recipients = ["geertgeerits@gmail.com"];

            var message = new EmailMessage
            {
                Subject = subject,
                Body = body,
                BodyFormat = EmailBodyFormat.PlainText,
                To = new List<string>(recipients)
            };

            try
            {
                await Email.Default.ComposeAsync(message);
            }
            catch (Exception ex)
            {
                await DisplayAlert(CalEventLang.ErrorTitle_Text, ex.Message, CalEventLang.ButtonClose_Text);
            }
        }
    }

    // Open the page 'PageWebsite' to open the website in the WebView control.
    // !!!BUG!!! in Android: the WebView control gives an error when opening a link to the Google Play Console.
    private async void OnBtnWebsiteLinkClicked(object sender, EventArgs e)
    {
#if ANDROID
        try
        {
            Uri uri = new("https://geertgeerits.wixsite.com/geertgeerits/calendar-events");
            BrowserLaunchOptions options = new()
            {
                LaunchMode = BrowserLaunchMode.SystemPreferred,
                TitleMode = BrowserTitleMode.Show
            };

            await Browser.Default.OpenAsync(uri, options);
        }
        catch (Exception ex)
        {
            await DisplayAlert(CalEventLang.ErrorTitle_Text, ex.Message, CalEventLang.ButtonClose_Text);
        }
#else
        await Navigation.PushAsync(new PageWebsite());
#endif
    }
}