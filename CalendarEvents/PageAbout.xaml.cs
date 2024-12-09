namespace CalendarEvents
{
    public sealed partial class PageAbout : ContentPage
    {
        public PageAbout()
    	{
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
#if DEBUG
                DisplayAlert("InitializeComponent: PageAbout", ex.Message, "OK");
#endif
                return;
            }
#if WINDOWS
            //// Set the margins for the controls in the title bar for Windows
            lblTitle.Margin = new Thickness(86, 18, 0, 0);
#endif
            //// Put text in the chosen language in the controls
            lblVersion.Text = $"{CalEventLang.Version_Text} 1.0.9";
            lblCopyright.Text = $"{CalEventLang.Copyright_Text} © 2023-2025 Geert Geerits";
            lblEmail.Text = $"{CalEventLang.Email_Text} geertgeerits@gmail.com";
            lblWebsite.Text = $"{CalEventLang.Website_Text}: ../calendarevents";
            lblPrivacyPolicy.Text = $"\n{CalEventLang.PrivacyPolicyTitle_Text} {CalEventLang.PrivacyPolicy_Text}";
            lblLicense.Text = $"\n{CalEventLang.LicenseTitle_Text}: {CalEventLang.License_Text}\n{CalEventLang.LicenseMit2_Text}";
            lblExplanation.Text = $"\n{CalEventLang.InfoExplanation_Text}";
        }

        /// <summary>
        /// Open the e-mail program 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Open the website link in the default browser 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnBtnWebsiteLinkClicked(object sender, EventArgs e)
        {
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
        }
    }
}