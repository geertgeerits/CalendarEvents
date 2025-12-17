/* Program .....: CalendarEvents.sln
 * Author ......: Geert Geerits - E-mail: geertgeerits@gmail.com
 * Copyright ...: (C) 2023-2026
 * Version .....: 1.0.10
 * Date ........: 2025-12-17 (YYYY-MM-DD)
 * Language ....: Microsoft Visual Studio 2026: .NET 10.0 MAUI C# 14.0
 * Description .: Read calendar events to share
 * Dependencies : NuGet Package: Plugin.Maui.CalendarStore version 4.0.0; https://github.com/jfversluis/Plugin.Maui.CalendarStore
 * Thanks to ...: Gerald Versluis for his video's on YouTube about .NET MAUI */

using Plugin.Maui.CalendarStore;
using System.Diagnostics;

namespace CalendarEvents
{
    public sealed partial class MainPage : ContentPage
    {
        //// Local variables
        private string cCopyright = "";
        private string cLicenseText = "";
        private string cCalendarId = "";
        private int nCalendarSelected;
        private readonly string cDicKeyAllCalendars = "000-AllCalendars-gg51";
        private IEnumerable<CalendarEvent>? events;

        public MainPage()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
#if DEBUG
                DisplayAlertAsync("InitializeComponent MainPage", ex.Message, "OK");
#endif
                return;
            }
#if WINDOWS
            //// !!!BUG!!! in Windows: Set the width for the 2e colomn of the grid because 'GridUnitType.Star' does not work in Windows if using the 'NavigationPage.TitleView'
            grdTitleView.ColumnDefinitions[1].Width = new GridLength(725);

            //// Set the margins for the controls in the title bar for Windows
            imgbtnAbout.Margin = new Thickness(20, 0, 0, 0);
            lblTitle.Margin = new Thickness(20, 10, 0, 0);
            lblNumberOfEvents.Margin = new Thickness(9, 15, 0, 0);
            lblTextToSpeech.Margin = new Thickness(0, 15, 0, 0);
#endif
#if IOS
            //// Set the scale of the activity indicator for iOS
            activityIndicator.Scale = 2;
#endif
            //// Select all the text in the entry field - works for all pages in the app
            Globals.ModifyEntrySelectAllText();

            //// Get the saved settings
            Globals.cTheme = Preferences.Default.Get("SettingTheme", "System");
            Globals.cDateFormatSelect = Preferences.Default.Get("SettingDateFormatSelect", "SystemShort");
            Globals.cAddDaysToStart = Preferences.Default.Get("SettingAddDaysToStart", "0");
            Globals.cAddDaysToEnd = Preferences.Default.Get("SettingAddDaysToEnd", "31");
            Globals.nSelectedCalendar = Preferences.Default.Get("SettingSelectedCalendar", 0);
            Globals.cLanguage = Preferences.Default.Get("SettingLanguage", "");
            Globals.cLanguageSpeech = Preferences.Default.Get("SettingLanguageSpeech", "");
            Globals.bLicense = Preferences.Default.Get("SettingLicense", false);

            //// The height of the title bar is lower when an iPhone is in horizontal position
            if (DeviceInfo.Platform == DevicePlatform.iOS && DeviceInfo.Idiom == DeviceIdiom.Phone)
            {
                imgbtnAbout.VerticalOptions = LayoutOptions.Start;
                lblTitle.VerticalOptions = LayoutOptions.Start;
                lblTitle.VerticalTextAlignment = TextAlignment.Start;
                imgbtnSettings.VerticalOptions = LayoutOptions.Start;
            }

            //// Set the theme
            Globals.SetTheme();

            //// Get the system date and time format and set the date and time format       
            switch (Globals.cDateFormatSelect)
            {
                case "SystemShort":
                    Globals.cDateFormatDatePicker = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
                    Globals.cDateFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
                    Globals.cTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;
                    break;
                case "SystemLong":
                    Globals.cDateFormatDatePicker = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
                    Globals.cDateFormat = CultureInfo.CurrentCulture.DateTimeFormat.LongDatePattern;
                    Globals.cTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;
                    break;
                default:
                    Globals.cDateFormatDatePicker = "yyyy-MM-dd";
                    Globals.cDateFormat = "yyyy-MM-dd";
                    Globals.cTimeFormat = "HH:mm";
                    break;
            }

            //// Set the date properties for the DatePickers
            dtpDateStart.MinimumDate = new DateTime(1583, 1, 1);
            dtpDateStart.MaximumDate = new DateTime(3000, 1, 1);
            dtpDateEnd.MinimumDate = new DateTime(1583, 1, 1);
            dtpDateEnd.MaximumDate = new DateTime(3000, 1, 1);

            //// Get and set the user interface language after a first start or reset of the application
            try
            {
                if (string.IsNullOrEmpty(Globals.cLanguage))
                {
                    Globals.cLanguage = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;

                    // Chinese needs the language code as zh-CN and zh-TW
                    if (Globals.cLanguage == "zh")
                    {
                        Globals.cLanguage = Thread.CurrentThread.CurrentUICulture.Name;
                    }
                }
            }
            catch (Exception)
            {
                Globals.cLanguage = "en";
            }
            finally
            {
                // Save the UI language
                Preferences.Default.Set("SettingLanguage", Globals.cLanguage);
                Debug.WriteLine("MainPage - Globals.cLanguage: " + Globals.cLanguage);
            }

            //// Set the text language
            SetTextLanguage();

            //// Initialize text to speech and get and set the speech language
            InitializeTextToSpeechAsync();

            //// Get all the calendars from the device and put them in the picker
            GetCalendars();
        }

        /// <summary>
        /// Initialize text to speech and get and set the speech language
        /// Must be called in the constructor of the MainPage and not in the ClassSpeech.cs
        /// The InitializeTextToSpeechAsync method is called asynchronously after the UI components are initialized
        /// Once the asynchronous operation completes, the Globals.bTextToSpeechAvailable value is checked, and the UI is updated accordingly
        /// </summary>
        private async void InitializeTextToSpeechAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(Globals.cLanguageSpeech))
                {
                    Globals.cLanguageSpeech = Thread.CurrentThread.CurrentUICulture.Name;
                }
            }
            catch (Exception)
            {
                Globals.cLanguageSpeech = "en-US";
            }

            // Initialize text to speech
            Globals.bTextToSpeechAvailable = await ClassSpeech.InitializeTextToSpeechAsync();

            if (Globals.bTextToSpeechAvailable)
            {
                lblTextToSpeech.IsVisible = true;
                imgbtnTextToSpeech.IsVisible = true;
                lblTextToSpeech.Text = GetIsoLanguageCode();

                // Search the selected language in the cLanguageLocales array
                ClassSpeech.SearchArrayWithSpeechLanguages(Globals.cLanguageSpeech);

                // Save the speech language
                Preferences.Default.Set("SettingLanguageSpeech", Globals.cLanguageSpeech);
            }

            Debug.WriteLine("MainPage - Globals.bTextToSpeechAvailable: " + Globals.bTextToSpeechAvailable);
            Debug.WriteLine("MainPage - Globals.cLanguageSpeech: " + Globals.cLanguageSpeech);
        }

        //// TitleView buttons clicked events
        private async void OnPageAboutClicked(object sender, EventArgs e)
        {
            ClassSpeech.CancelTextToSpeech();
            await Navigation.PushAsync(new PageAbout());
        }

        private async void OnPageSettingsClicked(object sender, EventArgs e)
        {
            ClassSpeech.CancelTextToSpeech();
            await Navigation.PushAsync(new PageSettings());
        }

        /// <summary>
        /// Event calendar picker changed 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPickerCalendarChanged(object sender, EventArgs e)
        {
            ClassSpeech.CancelTextToSpeech();

            lblNumberOfEvents.Text = "";
            lblCalendarEvents.Text = "";

            int nSelectedIndex = pckCalendars.SelectedIndex;
        
            // All calendars
            if (nSelectedIndex != -1)
            {
                nCalendarSelected = nSelectedIndex;
            }

            // One calendar
            if (nSelectedIndex > 0)
            {
                cCalendarId = Globals.calendarDictionary.Keys.ElementAt(nSelectedIndex);
            }
        }

        /// <summary>
        /// Get all the calendars from the device and put them in a picker 
        /// </summary>
        private async void GetCalendars()
        {
            await LoadCalendarsAsync();
        }

        /// <summary>
        /// Get all the calendars from the device and put them in a picker 
        /// </summary>
        /// <returns></returns>
        private async Task LoadCalendarsAsync()
        {
#if ANDROID
            // Permissions for Calendar read - Sometimes permission is not given in Android
            _ = await CheckAndRequestCalendarReadAsync();
#endif
            // Declare a temporary dictionary used to sort the calendars on name
            Dictionary<string, string> calendarDictionaryTemp = [];

            // Declare variable for the number op retries for asking for permission to read the calendar
            int nRetries = 0;

        Start:
            try
            {
                // Get all the calendars from the device.
                var calendars = await CalendarStore.Default.GetCalendars();

                if (!calendars.Any())
                {
                    pckCalendars.IsEnabled = false;
                    btnGetEvents.IsEnabled = false;

                    await DisplayAlertAsync("", CalEventLang.MessageNoCalendars_Text, CalEventLang.ButtonClose_Text);
                    Globals.calendarDictionary.Add(cDicKeyAllCalendars, CalEventLang.AllCalendars_Text);
                    return;
                }

                // Local language name for 'All calendars' (first item in the calendarDictionary and list of calendars)
                // After using the save button in the settings page the calendarDictionary is not empty
                if (Globals.calendarDictionary.Count > 0)
                {
                    Globals.calendarDictionary.Clear();
                }

                Globals.calendarDictionary.Add(cDicKeyAllCalendars, CalEventLang.AllCalendars_Text);

                // Put the calendars in the calendarDictionaryTemp
                foreach (var calendar in calendars)
                {
                    calendarDictionaryTemp.Add(calendar.Id, calendar.Name);
                }

                // Sort the calendarDictionaryTemp by value (calendar name)
                calendarDictionaryTemp = calendarDictionaryTemp.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

                // Add the sorted calendarDictionaryTemp to the calendarDictionary - 'All calendars' stays this way on the first place
                foreach (var item in calendarDictionaryTemp)
                {
                    if (!Globals.calendarDictionary.ContainsKey(item.Key))
                    {
                        Globals.calendarDictionary.Add(item.Key, item.Value);
                    }
                }

                // Put the calendars from the calendarDictionary via the calendarList in the picker
                List<string> calendarList = [.. Globals.calendarDictionary.Values];
            
                pckCalendars.ItemsSource = calendarList;

                if (nCalendarSelected > calendarList.Count - 1 || Globals.nSelectedCalendar > calendarList.Count - 1)
                {
                    pckCalendars.SelectedIndex = 0;
                    nCalendarSelected = 0;
                    Globals.nSelectedCalendar = 0;
                }
                else
                {
                    pckCalendars.SelectedIndex = nCalendarSelected;
                }
            }
            catch (Exception ex) when (ex is ArgumentException)
            {
                // ArgumentException: Value does not fall within the expected range
                // The Add method throws an exception if the new key is already in the dictionary
                //await DisplayAlertAsync(CalEventLang.ErrorTitle_Text, ex.Message, CalEventLang.ButtonClose_Text);
            }
            catch (Exception ex) when (ex is NullReferenceException)
            {
                // NullReferenceException: Object reference not set to an instance of an object
                // Permissions for CalendarRead - Sometimes permission is not given
                if (nRetries < 4)
                {
                    nRetries++;
                    _ = await CheckAndRequestCalendarReadAsync();
                    goto Start;
                }
            }
            catch (Exception ex)
            {
                _ = DisplayAlertAsync(CalEventLang.ErrorTitle_Text, ex.Message, CalEventLang.ButtonClose_Text);
            }
        }

        /// <summary>
        /// Get calendar events 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnGetEventsClicked(object sender, EventArgs e)
        {
            // Validate the date values
            if (dtpDateStart.Date > dtpDateEnd.Date)
            {
                // Swap the two dates
                (dtpDateStart.Date, dtpDateEnd.Date) = (dtpDateEnd.Date, dtpDateStart.Date);
            }

            // Close the keyboard
            entSearchWord.IsEnabled = false;
            entSearchWord.IsEnabled = true;

            // Cancel the text to speech
            ClassSpeech.CancelTextToSpeech();

            // Clear the calendar events
            lblNumberOfEvents.Text = "";
            lblCalendarEvents.Text = "";

            // Get calendar events. !!!BUG!!!? activityIndicator is only working after adding a Task.Delay()
            activityIndicator.IsRunning = true;
            await Task.Delay(200);

            await LoadEventsAsync();

            activityIndicator.IsRunning = false;
        }

        /// <summary>
        /// Get calendar events 
        /// </summary>
        /// <returns></returns>
        private async Task LoadEventsAsync()
        {
            // Declare a list for the calendar events
            List<string> lCalendarEvents = [];

            try
            {
                // All calendars
                if (pckCalendars.SelectedIndex == 0)
                {
                    events = await CalendarStore.Default.GetEvents(
                        startDate: dtpDateStart.Date!.Value,
                        endDate: dtpDateEnd.Date!.Value.AddDays(1)
                    );
                }
                // One calendar
                else
                {
                    events = await CalendarStore.Default.GetEvents(
                        calendarId: cCalendarId,
                        startDate: dtpDateStart.Date!.Value,
                        endDate: dtpDateEnd.Date!.Value.AddDays(1)
                    );
                }

                if (entSearchWord.Text is null or "")
                {
                    foreach (CalendarEvent ev in events)
                    {
                        lCalendarEvents.Add($"{ev.StartDate.ToString(format: $"{Globals.cDateFormat}  {Globals.cTimeFormat}")}   {ev.Title}\n\n");
                    }
                }
                else
                {
                    string cSearchWord = entSearchWord.Text.ToLowerInvariant().Trim();

                    foreach (CalendarEvent ev in events)
                    {
                        if (ev.Title is null or "")
                        {
                            continue;
                        }

                        if (ev.Title.Contains(cSearchWord, StringComparison.InvariantCultureIgnoreCase))
                        {
                            lCalendarEvents.Add($"{ev.StartDate.ToString(format: $"{Globals.cDateFormat}  {Globals.cTimeFormat}")}   {ev.Title}\n\n");
                        }
                    }
                }

                if (lCalendarEvents.Count == 0)
                {
                    lblCalendarEvents.Text = CalEventLang.MessageNoEvents_Text;
                }
                else
                {
                    // Remove duplicates
                    List<string> lCalendarEventsNoDupes = [.. lCalendarEvents.Distinct()];

                    // Put the events in the label
                    foreach (string cItem in lCalendarEventsNoDupes)
                    {
                        lblCalendarEvents.Text += cItem;
                        Debug.WriteLine(cItem);  // For testing
                    }

                    // Set the number of events in the label
                    lblNumberOfEvents.Text = lCalendarEventsNoDupes.Count.ToString("N0");
                }
            }
            catch (Exception ex) when (ex is ObjectDisposedException)
            {
                // ObjectDisposedException: Cannot access a disposed object
                // Happens when there are no events in the selected calendar or between the startDate and endDate
                lblCalendarEvents.Text = CalEventLang.MessageNoEvents_Text;
                Debug.WriteLine("Error ObjectDisposedException: " + ex.Message);
            }
            catch (Exception ex) when (ex is ArgumentException)
            {
                // System.ArgumentException: Handle must be valid. Arg_ParamName_Name, type
                // Happens when there are no events in the selected calendar or between the startDate and endDate
                lblCalendarEvents.Text = CalEventLang.MessageNoEvents_Text;
                Debug.WriteLine("Error ArgumentException: " + ex.Message);
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync(CalEventLang.ErrorTitle_Text, $"{CalEventLang.ErrorCalendar_Text}\n\n{ex.Message}", CalEventLang.ButtonClose_Text);
            }
        }

        /// <summary>
        /// Clear the calendar events 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClearEventsClicked(object sender, EventArgs e)
        {
            ClassSpeech.CancelTextToSpeech();

            lblNumberOfEvents.Text = "";
            lblCalendarEvents.Text = "";

            _ = entSearchWord.Focus();
        }

        /// <summary>
        /// Copy calendar events to clipboard 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnClipboardButtonClicked(object sender, EventArgs e)
        {
            if (lblCalendarEvents.Text is not null and not "")
            {
                await Clipboard.Default.SetTextAsync(lblCalendarEvents.Text);
            }
        }

        /// <summary>
        /// Share calendar events 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnButtonShareClicked(object sender, EventArgs e)
        {
            if (lblCalendarEvents.Text is not null and not "")
            {
                await ShareTextAsync(lblCalendarEvents.Text);
            }
        }

        /// <summary>
        /// Share calendar events 
        /// </summary>
        /// <param name="cText"></param>
        /// <returns></returns>
        private static async Task ShareTextAsync(string cText)
        {
            await Share.Default.RequestAsync(new ShareTextRequest
            {
                Text = cText,
                Title = CalEventLang.NameProgramLocal_Text
            });
        }

        /// <summary>
        /// Put text in the chosen language in the controls 
        /// </summary>
        private void SetTextLanguage()
        {
            // Set the CurrentUICulture
            Globals.SetCultureSelectedLanguage(Globals.cLanguage);

            cCopyright = $"{CalEventLang.Copyright_Text} © 2023-2025 Geert Geerits";
            cLicenseText = $"{CalEventLang.License_Text}\n\n{CalEventLang.LicenseMit2_Text}";

            // Local name for 'All calendars' in calendarDictionary and calendar picker
            if (Globals.bLanguageChanged)
            {
                // Local language name for 'All calendars' (first item in the calendarDictionary, calendarList and calendar picker)
                Globals.calendarDictionary[cDicKeyAllCalendars] = CalEventLang.AllCalendars_Text;

                // Put the calendars from the calendarDictionary via the calendarList in the picker
                int nSelectedIndex = pckCalendars.SelectedIndex;

                List<string> calendarList = [.. Globals.calendarDictionary.Values];
                pckCalendars.ItemsSource = calendarList;

                pckCalendars.SelectedIndex = nSelectedIndex;
            }
        }

        /// <summary>
        /// Show license using the Loaded event of the MainPage.xaml 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnPageLoaded(object sender, EventArgs e)
        {
            // Show license.
            if (Globals.bLicense == false)
            {
                Globals.bLicense = await Application.Current!.Windows[0].Page!.DisplayAlertAsync(CalEventLang.LicenseTitle_Text, $"Calendar Events\n{cCopyright}\n\n{cLicenseText}", CalEventLang.Agree_Text, CalEventLang.Disagree_Text);

                if (Globals.bLicense)
                {
                    Preferences.Default.Set("SettingLicense", true);
                }
                else
                {
#if IOS
                    //Thread.CurrentThread.Abort();  // Not allowed in iOS.
                    imgbtnAbout.IsEnabled = false;
                    imgbtnSettings.IsEnabled= false;
                    btnGetEvents.IsEnabled = false;
                    btnClearEvents.IsEnabled = false;
                    btnCopyEvents.IsEnabled = false;
                    btnShareEvents.IsEnabled = false;
                    imgbtnTextToSpeech.IsEnabled = false;

                    await DisplayAlertAsync(CalEventLang.LicenseTitle_Text, CalEventLang.CloseApplication_Text, CalEventLang.ButtonClose_Text);
#else
                    Application.Current.Quit();
#endif
                }
            }

            // Set focus to the first entry field
            Task.Delay(500).Wait();
            entSearchWord.Focus();
        }

        /// <summary>
        /// Set language and dates using the Appearing event of the MainPage.xaml 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPageAppearing(object sender, EventArgs e)
        {
            // Set language.
            if (Globals.bLanguageChanged)
            {
                SetTextLanguage();
                Globals.bLanguageChanged = false;
            }

            // Set the date format property
            dtpDateStart.Format = Globals.cDateFormatDatePicker;
            dtpDateEnd.Format = Globals.cDateFormatDatePicker;

            // Set the calendar days in the past and in the future
            dtpDateStart.Date = DateTime.Today.Date.AddDays(Convert.ToInt32(Globals.cAddDaysToStart));
            dtpDateEnd.Date = DateTime.Today.Date.AddDays(Convert.ToInt32(Globals.cAddDaysToEnd));

            // Set the language ISO code of the text to speech in the label
            lblTextToSpeech.Text = GetIsoLanguageCode();

            // Set the selected calendar in the picker
            try
            {
                pckCalendars.SelectedIndex = Globals.nSelectedCalendar;
            }
            catch (Exception)
            {
                Globals.nSelectedCalendar = 0;
                pckCalendars.SelectedIndex = 0;
            }

            // Set focus to the first entry field
            _ = entSearchWord.Focus();

            // Cancel the text to speech
            ClassSpeech.CancelTextToSpeech();
        }

        /// <summary>
        ///  On page disappearing event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnPageDisappearing(object sender, EventArgs e)
        {
            // Hide the soft keyboard when the page disappears
            if (entSearchWord.IsSoftInputShowing())
            {
                await entSearchWord.HideSoftInputAsync(System.Threading.CancellationToken.None);
            }
        }

        /// <summary>
        /// Permissions for CalendarRead - Sometimes permission is not given in Android (not yet tested in iOS) 
        /// </summary>
        /// <returns></returns>
        private async Task<PermissionStatus> CheckAndRequestCalendarReadAsync()
        {
            PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.CalendarRead>();
        
            if (status == PermissionStatus.Granted)
                return status;

            // !!!BUG!!! in Android?: does not works without the DisplayAlert
            await DisplayAlertAsync("", CalEventLang.CalendarPermission_Text, CalEventLang.ButtonClose_Text);

            status = await Permissions.RequestAsync<Permissions.CalendarRead>();
            //await DisplayAlertAsync("CheckAndRequestCalendarRead", status.ToString(), "OK");  // For testing

            return status;
        }

        /// <summary>
        /// Button text to speech event - Convert text to speech
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTextToSpeechClicked(object sender, EventArgs e)
        {
            // Cancel the text to speech
            if (Globals.bTextToSpeechIsBusy)
            {
                imgbtnTextToSpeech.Source = ClassSpeech.CancelTextToSpeech();
                return;
            }

            // Convert the text to speech
            _ = ClassSpeech.ConvertTextToSpeechAsync(imgbtnTextToSpeech, lblCalendarEvents.Text);
        }

        /// <summary>
        /// Get ISO language (and country) code from locales 
        /// </summary>
        /// <returns></returns>
        private static string GetIsoLanguageCode()
        {
            // Split before first space and remove last character '-' if there
            string cLanguageIso = Globals.cLanguageSpeech.Split(' ').First();

            if (cLanguageIso.EndsWith('-'))
            {
                cLanguageIso = cLanguageIso[..^1];
            }

            return cLanguageIso;
        }
    }
}