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
                DisplayAlertAsync("InitializeComponent: PageAbout", ex.Message, "OK");
#endif
                return;
            }
#if WINDOWS
            //// Set the margins for the controls in the title bar for Windows
            lblTitle.Margin = new Thickness(86, 18, 0, 0);
#endif
            //// Put text in the chosen language in the controls
            lblVersion.Text = $"{CalEventLang.Version_Text} 1.0.11";
            lblCopyright.Text = $"{CalEventLang.Copyright_Text} © 2023-2026 Geert Geerits";
            lblPrivacyPolicy.Text = $"\n{CalEventLang.PrivacyPolicyTitle_Text} {CalEventLang.PrivacyPolicy_Text}";
            lblLicense.Text = $"{CalEventLang.LicenseTitle_Text}: {CalEventLang.License_Text}\n{CalEventLang.LicenseMit2_Text}";
            lblExplanation.Text = $"\n{CalEventLang.InfoExplanation_Text}";
            lblTrademarks.Text = $"\n{CalEventLang.Trademarks_Text}";
        }
    }

    /// <summary>
    /// Open e-mail app and open webpage (reusable hyperlink class)
    /// </summary>
    public sealed partial class HyperlinkSpan : Span
    {
        public static readonly BindableProperty UrlProperty =
            BindableProperty.Create(nameof(Url), typeof(string), typeof(HyperlinkSpan), null);

        public string Url
        {
            get { return (string)GetValue(UrlProperty); }
            set { SetValue(UrlProperty, value); }
        }

        public HyperlinkSpan()
        {
            FontFamily = "OpenSansRegular";
            FontAttributes = FontAttributes.Bold;
            FontSize = 16;
            TextDecorations = TextDecorations.Underline;

            GestureRecognizers.Add(new TapGestureRecognizer
            {
                // Launcher.OpenAsync is provided by Essentials
                //Command = new Command(async () => await Launcher.OpenAsync(Url))
                Command = new Command(async () => await OpenHyperlink(Url))
            });
        }

        /// <summary>
        /// Open the e-mail program or the website link
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static async Task OpenHyperlink(string url)
        {
            if (url.StartsWith("mailto:"))
            {
                await OpenEmailLink(url[7..]);
            }
            //else
            //{
            //    await OpenWebsiteLink(url);
            //}
        }

        /// <summary>
        /// Open the e-mail program
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static async Task OpenEmailLink(string url)
        {
            if (Email.Default.IsComposeSupported)
            {
                string subject = "Calendar Events";
                string body = "";
                string[] recipients = [url];

                var message = new EmailMessage
                {
                    Subject = subject,
                    Body = body,
                    BodyFormat = EmailBodyFormat.PlainText,
                    To = [.. recipients]
                };

                try
                {
                    await Email.Default.ComposeAsync(message);
                }
                catch (Exception ex)
                {
                    await Application.Current!.Windows[0].Page!.DisplayAlertAsync(CalEventLang.ErrorTitle_Text, ex.Message, CalEventLang.ButtonClose_Text);
                }
            }
        }

        ///// <summary>
        ///// Open the website link in the default browser
        ///// </summary>
        ///// <param name="url"></param>
        ///// <returns></returns>
        //private static async Task OpenWebsiteLink(string url)
        //{
        //    try
        //    {
        //        Uri uri = new(url);
        //        BrowserLaunchOptions options = new()
        //        {
        //            LaunchMode = BrowserLaunchMode.SystemPreferred,
        //            TitleMode = BrowserTitleMode.Show
        //        };

        //        await Browser.Default.OpenAsync(uri, options);
        //    }
        //    catch (Exception ex)
        //    {
        //        await Application.Current!.Windows[0].Page!.DisplayAlertAsync(CalEventLang.ErrorTitle_Text, ex.Message, CalEventLang.ButtonClose_Text);
        //    }
        //}
    }
}