namespace CalendarEvents
{
    internal static class ClassEntryMethods
    {
        // Global variables
        public static bool bKeyboardCustom = true;
        public static string cNumDecimalDigits = string.Empty;
        public static string cPercDecimalDigits = string.Empty;
        public static string cRoundNumber = string.Empty;
        public static bool bColorNumber = true;
        public static bool bShowFormattedNumber;
        public static string cNumGroupSeparator = string.Empty;
        public static string cNumDecimalSeparator = string.Empty;
        public static string cNumNegativeSign = string.Empty;
        public static string cNumNativeDigits = string.Empty;

        // Local variables
        private static string cDecimalCharacters = string.Empty;
        private static readonly string cHexadecimalCharacters = "0123456789ABCDEFabcdef";
        private static string cColorNegNumber = string.Empty;
        private static string cColorPosNumber = string.Empty;

        /// <summary>
        /// Initialize the number format settings based on the current culture
        /// </summary>
        public static void InitializeNumberFormat()
        {
            // Get the current culture's number format
            // For initializing number formatting settings, use CurrentCulture
            // It ensures that the number format aligns with the user's regional settings, which is the intended behavior in the InitializeNumberFormat method
            NumberFormatInfo numberFormatInfo = System.Globalization.CultureInfo.CurrentCulture.NumberFormat;

            // Set the number properties based on the current culture
            cNumGroupSeparator = numberFormatInfo.NumberGroupSeparator;                 // Use numberFormatInfo.Number...
            cNumDecimalSeparator = numberFormatInfo.NumberDecimalSeparator;             // Use numberFormatInfo.Number...
            cNumNegativeSign = numberFormatInfo.NegativeSign;
            cNumNativeDigits = string.Join("", numberFormatInfo.NativeDigits);          // Get the native digits as a string from the array

            if (string.IsNullOrEmpty(cNumDecimalDigits))
            {
                cNumDecimalDigits = numberFormatInfo.CurrencyDecimalDigits.ToString();  // Use numberFormatInfo.Currency...
                Preferences.Default.Set("SettingNumDecimalDigits", cNumDecimalDigits);
            }

            if (string.IsNullOrEmpty(cPercDecimalDigits))
            {
                cPercDecimalDigits = numberFormatInfo.PercentDecimalDigits.ToString();  // Use numberFormatInfo.Percent...
                Preferences.Default.Set("SettingPercDecimalDigits", cPercDecimalDigits);
            }

            // Set the rounding system of numbers (AwayFromZero, ToEven, ToZero)
            if (string.IsNullOrEmpty(cRoundNumber))
            {
                cRoundNumber = "AwayFromZero";
                Preferences.Default.Set("SettingRoundNumber", cRoundNumber);
            }

            Debug.WriteLine($"cNumGroupSeparator: {cNumGroupSeparator}");
            Debug.WriteLine($"cNumDecimalSeparator: {cNumDecimalSeparator}");
            Debug.WriteLine($"cNumNegativeSign: {cNumNegativeSign}");
            Debug.WriteLine($"cNumNativeDigits: {cNumNativeDigits}");
            Debug.WriteLine($"cNumDecimalDigits: {cNumDecimalDigits}");
            Debug.WriteLine($"cPercDecimalDigits: {cPercDecimalDigits}");
            Debug.WriteLine($"cRoundNumber: {cRoundNumber}");

            // Check the number settings and set default values if they are empty
            if (string.IsNullOrEmpty(cNumGroupSeparator))
            {
                cNumGroupSeparator = ",";
            }

            if (string.IsNullOrEmpty(cNumDecimalSeparator))
            {
                cNumDecimalSeparator = cNumGroupSeparator == "," ? "." : ",";
            }

            if (string.IsNullOrEmpty(cNumNegativeSign))
            {
                cNumNegativeSign = "-";
            }

            if (string.IsNullOrEmpty(cNumNativeDigits))
            {
                cNumNativeDigits = "0123456789";
            }

            if (string.IsNullOrEmpty(cNumDecimalDigits))
            {
                cNumDecimalDigits = "2";
            }

            if (string.IsNullOrEmpty(cPercDecimalDigits))
            {
                cPercDecimalDigits = "2";
            }

            // Set the allowed characters for numeric input
            cDecimalCharacters = $"{cNumDecimalSeparator}{cNumNegativeSign}{cNumNativeDigits}";
            Debug.WriteLine($"cDecimalCharacters: {cDecimalCharacters}");

            // Set the entry text color to a different color for a negative and a positive number
            SetNumberColor();
        }

        /// <summary>
        /// Set the placeholder text for the entry fields if the Placeholder property is empty or null
        /// and the ValidationTriggerActionDecimal MinValue and MaxValue are set
        /// </summary>
        /// <param name="entry"></param>
        public static void SetNumberEntryProperties(Entry entry, string cNumberOfDecimals = "-1")
        {
            if (!int.TryParse(cNumberOfDecimals, out int nNumberOfDecimals))
            {
                return;
            }

            // Find the ValidationTriggerActionDecimal attached to the Entry and return its MinValue, MaxValue and MaxDecimalPlaces
            (decimal nMinValue, decimal nMaxValue, _) = EntryFindValidationTriggerActionDecimal(entry);

            // Construct the placeholder text based on MinValue, MaxValue and number of decimals
            string cValueFrom = MakeEntryPlaceholder(nMinValue.ToString(), nNumberOfDecimals);
            string cValueTo = MakeEntryPlaceholder(nMaxValue.ToString(), nNumberOfDecimals);

            // Set the Placeholder for the entry field
            entry.Placeholder = $"{cValueFrom} - {cValueTo}";
        }

        /// <summary>
        /// Construct the placeholder text based on MinValue, MaxValue and number of decimals
        /// </summary>
        /// <param name="cValue"></param>
        /// <param name="nNumberOfDecimals"></param>
        /// <returns></returns>
        private static string MakeEntryPlaceholder(string cValue, int nNumberOfDecimals)
        {
            string[] cParts;

            if (nNumberOfDecimals == 0)
            {
                if (cValue.Contains(cNumDecimalSeparator))
                {
                    cParts = cValue.Split(cNumDecimalSeparator);
                    return cParts[0];
                }
            }
            else if (nNumberOfDecimals > 0)
            {
                if (cValue.Contains(cNumDecimalSeparator))
                {
                    cParts = cValue.Split(cNumDecimalSeparator);
                    string cBeforeDecimalPoint = cParts[0];
                    string cAfterDecimalPoint = cParts.Length > 1 ? cParts[1] : "9";
                    string cLastDigit = cValue[^1].ToString();

                    if (cParts[1].Length != nNumberOfDecimals)
                    {
                        cAfterDecimalPoint = string.Concat(Enumerable.Repeat(cLastDigit, nNumberOfDecimals));
                    }

                    return $"{cBeforeDecimalPoint}{cNumDecimalSeparator}{cAfterDecimalPoint}";
                }
            }

            return cValue;
        }

        /// <summary>
        /// Check if the text is a decimal number
        /// </summary>
        /// <param name="cText"></param>
        /// <returns></returns>
        public static bool IsDecimalNumber(Entry entry, string cText)
        {
            if (bShowFormattedNumber)
            {
                return true;
            }

            if (string.IsNullOrEmpty(cText))
            {
                return true;
            }

            // Check the text for invalid characters
            foreach (char c in cText)
            {
                // Check if the character is allowed
                if (!cDecimalCharacters.Contains(c))
                {
                    return false;
                }

                // Check if the character is a negative sign and at the first position (index 0), or there is no more than one negative sign
                if ((c == cNumNegativeSign[0] && !cText.StartsWith(c)) || cText.Count(static ch => ch == cNumNegativeSign[0]) > 1)
                {
                    return false;
                }

                // Check if the decimal separator is already in the string
                if (c == cNumDecimalSeparator[0] && cText.IndexOf(c) != cText.LastIndexOf(c))
                {
                    return false;
                }
            }

            // Get the number of decimals allowed after the decimal separator
            // The method 'IsDecimalNumber' is called before the 'ValidationTriggerActionDecimal' class, so the properties of the 'ValidationTriggerActionDecimal' class cannot be accessed directly
            int nDecimals = -1;

            // Find the ValidationTriggerActionDecimal attached to the Entry and return its MinValue, MaxValue and MaxDecimalPlaces
            (decimal nMinValue, decimal nMaxValue, nDecimals) = EntryFindValidationTriggerActionDecimal(entry);

            if (nDecimals == -1)
            {
                /* If the number of decimal places for percentages differs from that of regular numbers, make sure the `AutomationId`
                   is set for any "percentage" entry field. The `AutomationId` must include the word "Percentage" — for example:  
                   AutomationId="Percentage"` or `AutomationId="xxx-Percentage"`.  
                   Alternatively, ensure the validation trigger is configured appropriately for each entry field. Example:  
                   <local:Validation TriggerAction="Decimal" MinValue="-999999.999" MaxValue="999999.999" MaxDecimalPlaces="3"/>
                */
                nDecimals = !string.IsNullOrEmpty(entry.AutomationId) && entry.AutomationId.Contains("Percentage")
                    ? int.Parse(cPercDecimalDigits)
                    : int.Parse(cNumDecimalDigits);
            }
            Debug.WriteLine($"IsDecimalNumber - nDecimals: {nDecimals}");

            // Check if the decimal separator is allowed
            if (cText.Contains(cNumDecimalSeparator) && nDecimals == 0)
            {
                return false;
            }

            // Check if the number of decimals is valid
            if (cText.Contains(cNumDecimalSeparator) && cText.Length - cText.IndexOf(cNumDecimalSeparator[0]) > nDecimals + 1)
            {
                return false;
            }

            // Validate the number and set the text color
            SetEntryNumberColor(entry);

            return true;
        }

        /// <summary>
        /// Handles the click event for the minus button, toggling the negative sign on the currently focused entry field
        /// </summary>
        /// <param name="entry"></param>
        public static void BtnNumericMinusSignClicked(Entry entry)
        {
            if (entry == null)
            {
                return;
            }

            if (!entry.Text.Contains(cNumNegativeSign))
            {
                entry.Text = cNumNegativeSign + entry.Text;
                entry.CursorPosition = 1;
            }
            else
            {
                entry.Text = entry.Text.Replace(cNumNegativeSign, string.Empty);
                entry.CursorPosition = 0;
            }

            // Set the text color for negative and positive numbers
            SetEntryNumberColor(entry);

            entry.Focus();
            Task.Delay(100).Wait();
            entry.SelectionLength = 0;
        }

        /// <summary>
        /// Set the color for negative and positive numbers in an entry field
        /// </summary>
        /// <param name="entry"></param>
        public static void SetEntryNumberColor(Entry entry)
        {
            if (decimal.TryParse(entry.Text, out decimal nValue))
            {
                entry.TextColor = nValue < 0 ? Color.FromArgb(cColorNegNumber) : Color.FromArgb(cColorPosNumber);
            }
        }

        /// <summary>
        /// Check if the text is a hexadecimal number
        /// </summary>
        /// <param name="cText"></param>
        /// <returns></returns>
        public static bool IsHexadecimalNumber(string cText)
        {
            if (string.IsNullOrEmpty(cText))
            {
                return true;
            }

            // Check the text for invalid characters
            foreach (char c in cText)
            {
                // Check if the character is allowed
                if (!cHexadecimalCharacters.Contains(c))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Entry focused event: format the text value for a numeric entry without the number separator and select the entire text value
        /// </summary>
        /// <param name="entry"></param>
        public static async Task FormatDecimalNumberEntryFocused(Entry entry)
        {
            // Show the keyboard if it is not already shown and no custom keyboard is used
            if (!entry.IsSoftInputShowing() && !bKeyboardCustom)
            {
                await entry.ShowSoftInputAsync(System.Threading.CancellationToken.None);
            }

            // Allow the IsDecimalNumber method to execute
            bShowFormattedNumber = false;

            if (string.IsNullOrEmpty(entry.Text))
            {
                return;
            }

            // Find the ValidationTriggerActionDecimal attached to the Entry and return its MinValue, MaxValue and MaxDecimalPlaces
            (_, _, int nDecimals) = EntryFindValidationTriggerActionDecimal(entry);

            if (decimal.TryParse(entry.Text, out decimal nValue))
            {
                if (nDecimals != -1)
                {
                    entry.Text = nValue.ToString(format: "F" + nDecimals);
                }
                else
                {
                    /* If the number of decimal places for percentages differs from that of regular numbers, make sure the `AutomationId`
                       is set for any 'percentage' entry field. The `AutomationId` must include the word "Percentage" — for example:  
                       AutomationId="Percentage"` or `AutomationId="xxx-Percentage"`.  
                       Alternatively, ensure the validation trigger is configured appropriately for each entry field. Example:  
                       <local:Validation TriggerAction="Decimal" MinValue="-999999.999" MaxValue="999999.999" MaxDecimalPlaces="3"/>
                    */
                    entry.Text = !string.IsNullOrEmpty(entry.AutomationId) && entry.AutomationId.Contains("Percentage")
                        ? nValue.ToString(format: "F" + cPercDecimalDigits)
                        : nValue.ToString(format: "F" + cNumDecimalDigits);
                }

                // Select all the text in the entry field
                entry.CursorPosition = 0;
                entry.SelectionLength = entry.Text.Length;
            }
        }

        /// <summary>
        /// Entry unfocused event: format the text value for a numeric entry field with the number separator
        /// </summary>
        /// <param name="entry"></param>
        public static void FormatDecimalNumberEntryUnfocused(Entry entry)
        {
            if (string.IsNullOrEmpty(entry.Text))
            {
                return;
            }

            // Do not allow the IsDecimalNumber method to execute
            bShowFormattedNumber = true;

            // Find the ValidationTriggerActionDecimal attached to the Entry and return its MinValue, MaxValue and MaxDecimalPlaces
            (_, _, int nDecimals) = EntryFindValidationTriggerActionDecimal(entry);

            if (decimal.TryParse(entry.Text, out decimal nValue))
            {
                if (nDecimals != -1)
                {
                    entry.Text = nValue.ToString(format: "N" + nDecimals);
                }
                else
                {
                    /* If the number of decimal places for percentages differs from that of regular numbers, make sure the `AutomationId`
                       is set for any "percentage" entry field. The `AutomationId` must include the word "Percentage" — for example:  
                       AutomationId="Percentage"` or `AutomationId="xxx-Percentage"`.  
                       Alternatively, ensure the validation trigger is configured appropriately for each entry field. Example:  
                       <local:Validation TriggerAction="Decimal" MinValue="-999999.999" MaxValue="999999.999" MaxDecimalPlaces="3"/>
                    */
                    entry.Text = !string.IsNullOrEmpty(entry.AutomationId) && entry.AutomationId.Contains("Percentage")
                    ? nValue.ToString(format: "N" + cPercDecimalDigits)
                    : nValue.ToString(format: "N" + cNumDecimalDigits);
                }
            }
            else
            {
                entry.Text = "";
                entry.Focus();
            }
        }

        /* Rounding numbers
           Round away from zero: MidpointRounding.AwayFromZero = 1-4 down ; 5-9 up
           Round half to even or banker's rounding: MidpointRounding.ToEven
           Round towards zero: MidpointRounding.ToZero 1-9 down

           Value      Default    ToEven     AwayFromZero    ToZero
            12.0       12         12         12              12
            12.1       12         12         12              12
            12.2       12         12         12              12
            12.3       12         12         12              12
            12.4       12         12         12              12
            12.5       12         12         13              12
            12.6       13         13         13              12
            12.7       13         13         13              12
            12.8       13         13         13              12
            12.9       13         13         13              12
            13.0       13         13         13              13
         
          Format specifier: "F" = 1234.56 or 1234,56 ; "N" = 1,234.56 or 1.234,56 */

        /// <summary>
        /// Rounding and formatting double number to # decimals returning number as value and as string 
        /// </summary>
        /// <param name="nNumber"></param>
        /// <param name="nNumDec"></param>
        /// <param name="cFormatSpecifier"></param>
        /// <returns></returns>
        public static string RoundToNumDecimals(ref double nNumber, int nNumDec, string cFormatSpecifier)
        {
            nNumber = cRoundNumber switch
            {
                "AwayFromZero" => Math.Round(nNumber, nNumDec, MidpointRounding.AwayFromZero),
                "ToEven" => Math.Round(nNumber, nNumDec, MidpointRounding.ToEven),
                _ => Math.Round(nNumber, nNumDec, MidpointRounding.ToZero),
            };
            return nNumber.ToString(format: cFormatSpecifier + nNumDec.ToString());
        }

        /// <summary>
        /// Rounding and formatting decimal number to # decimals returning number as value and as string 
        /// </summary>
        /// <param name="nNumber"></param>
        /// <param name="nNumDec"></param>
        /// <param name="cFormatSpecifier"></param>
        /// <returns></returns>
        public static string RoundToNumDecimals(ref decimal nNumber, int nNumDec, string cFormatSpecifier)
        {
            nNumber = cRoundNumber switch
            {
                "AwayFromZero" => Math.Round(nNumber, nNumDec, MidpointRounding.AwayFromZero),
                "ToEven" => Math.Round(nNumber, nNumDec, MidpointRounding.ToEven),
                _ => Math.Round(nNumber, nNumDec, MidpointRounding.ToZero),
            };
            return nNumber.ToString(format: cFormatSpecifier + nNumDec.ToString());
        }

        /// <summary>
        /// Set the entry text color to a different color for a negative and a positive number
        /// </summary>
        public static void SetNumberColor()
        {
            if (Microsoft.Maui.Controls.Application.Current == null)
            {
                Debug.WriteLine("Application.Current is null. Ensure the application is properly initialized.");
                return;
            }

            // Set the color for negative and positive numbers
            string cColorPosNumberLight, cColorPosNumberDark, cColorNegNumberLight, cColorNegNumberDark;

            if (Application.Current?.Resources?.TryGetValue("EntryTextPositiveNumberLight", out object? v1) == true && v1 is Color colPosLight)
            {
                cColorPosNumberLight = ColorToHex(colPosLight);
            }
            else
            {
                cColorPosNumberLight = "#000000";  // fallback
            }

            if (Application.Current?.Resources?.TryGetValue("EntryTextPositiveNumberDark", out object? v2) == true && v2 is Color colPosDark)
            {
                cColorPosNumberDark = ColorToHex(colPosDark);
            }
            else
            {
                cColorPosNumberDark = "#FFFFFF";  // fallback
            }

            if (Application.Current?.Resources?.TryGetValue("EntryTextNegativeNumberLight", out object? v3) == true && v3 is Color colNegLight)
            {
                cColorNegNumberLight = ColorToHex(colNegLight);
            }
            else
            {
                cColorNegNumberLight = "#FF0000";  // fallback
            }

            if (Application.Current?.Resources?.TryGetValue("EntryTextNegativeNumberDark", out object? v4) == true && v4 is Color colNegDark)
            {
                cColorNegNumberDark = ColorToHex(colNegDark);
            }
            else
            {
                cColorNegNumberDark = "#FFB6C1";  // fallback
            }

            // Get the current device theme
            AppTheme currentTheme = Application.Current != null ? Application.Current.RequestedTheme : AppTheme.Unspecified;

            // Set the number text color
            switch (currentTheme)
            {
                case AppTheme.Light:
                    cColorNegNumber = bColorNumber ? cColorNegNumberLight : cColorPosNumberLight;
                    cColorPosNumber = cColorPosNumberLight;
                    break;

                case AppTheme.Dark:
                    cColorNegNumber = bColorNumber ? cColorNegNumberDark : cColorPosNumberDark;
                    cColorPosNumber = cColorPosNumberDark;
                    break;

                case AppTheme.Unspecified:
                default:
                    if (currentTheme == AppTheme.Dark)
                    {
                        cColorNegNumber = bColorNumber ? cColorNegNumberDark : cColorPosNumberDark;
                        cColorPosNumber = cColorPosNumberDark;
                    }
                    else
                    {
                        cColorNegNumber = bColorNumber ? cColorNegNumberLight : cColorPosNumberLight;
                        cColorPosNumber = cColorPosNumberLight;
                    }
                    break;
            }
        }

        /// <summary>
        /// Set the label text color to a different color for a negative and a positive number
        /// </summary>
        /// <param name="label"></param>
        public static void SetLabelTextColorForNumber(Label label)
        {
            if (decimal.TryParse(label.Text, out decimal nValue))
            {
                label.TextColor = nValue < 0 ? Color.FromArgb(cColorNegNumber) : Color.FromArgb(cColorPosNumber);
            }
        }

        /// <summary>
        /// Select all the text in the entry field
        /// </summary>
        public static void ModifyEntrySelectAllText()
        {
            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("MyCustomization", (handler, view) =>
            {
#if ANDROID
            handler.PlatformView.SetSelectAllOnFocus(true);
#elif IOS || MACCATALYST
                handler.PlatformView.EditingDidBegin += (s, e) =>
                {
                    handler.PlatformView.PerformSelector(new ObjCRuntime.Selector("selectAll"), null, 0.0f);
                };
#elif WINDOWS
            handler.PlatformView.GotFocus += (s, e) =>
            {
                handler.PlatformView.SelectAll();
            };
#endif
            });
        }

        /// <summary>
        /// Hide the keyboard
        /// </summary>
        /// <param name="entry"></param>
        public static async Task HideSystemKeyboard(Entry entry)
        {
            try
            {
                if (entry.IsSoftInputShowing())
                {
#if ANDROID
                    // Android !!!BUG!!!: entry.Unfocus() must be called before HideSoftInputAsync() otherwise entry.Unfocus() is not called
                    entry.Unfocus();
#endif
                    await entry.HideSoftInputAsync(System.Threading.CancellationToken.None);
                }
            }
            catch (Exception)
            {
                entry.IsEnabled = false;
                entry.IsEnabled = true;
            }
        }

        /// <summary>
        /// Find the ValidationTriggerActionDecimal attached to the Entry and return its MinValue, MaxValue and MaxDecimalPlaces
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        private static Tuple<decimal, decimal, int> EntryFindValidationTriggerActionDecimal(Entry entry)
        {
            decimal nMinValue = 0;
            decimal nMaxValue = 0;
            int nDecimals = -1;

            // Find the ValidationTriggerActionDecimal attached to the Entry
            var trigger = entry.Triggers
                .OfType<EventTrigger>()
                .SelectMany(t => t.Actions)
                .OfType<ValidationTriggerActionDecimal>()
                .FirstOrDefault();

            if (trigger != null)
            {
                // Use these values as needed
                nMinValue = trigger.MinValue;
                nMaxValue = trigger.MaxValue;
                nDecimals = trigger.MaxDecimalPlaces;

                Debug.WriteLine($"IsDecimalNumber - MinValue: {trigger.MinValue}");
                Debug.WriteLine($"IsDecimalNumber - MaxValue: {trigger.MaxValue}");
                Debug.WriteLine($"IsDecimalNumber - MaxDecimalPlaces: {trigger.MaxDecimalPlaces}");
            }

            return Tuple.Create(nMinValue, nMaxValue, nDecimals);
        }

        /// <summary>
        /// Converts the specified color to its hexadecimal string representation
        /// </summary>
        /// <remarks>The returned string includes the alpha component only if the color is not fully
        /// opaque. Each component is represented as a two-digit hexadecimal value.</remarks>
        /// <param name="color">The color to convert to a hexadecimal string. The color's red, green, blue, and alpha components are used to
        /// generate the output.</param>
        /// <returns>A hexadecimal string representing the color. Returns a string in the format "#RRGGBB" if the color is fully
        /// opaque, or "#AARRGGBB" if the color has transparency.</returns>
        public static string ColorToHex(Color color)
        {
            int r = (int)(color.Red * 255);
            int g = (int)(color.Green * 255);
            int b = (int)(color.Blue * 255);
            int a = (int)(color.Alpha * 255);

            // If alpha is less than 255, include it; otherwise, use #RRGGBB
            return a < 255
                ? $"#{a:X2}{r:X2}{g:X2}{b:X2}"
                : $"#{r:X2}{g:X2}{b:X2}";
        }

        ///// <summary>
        ///// Test the rounding of numbers
        ///// </summary>
        //public static void TestRoundingNumbers()
        //{
        //    List<double> numbers = new List<double> { 12.0, 12.1, 12.2, 12.3, 12.4, 12.5, 12.6, 12.7, 12.8, 12.9, 13.0 };

        //    foreach (double number in numbers)
        //    {
        //        double nNumber = number;
        //        string cFormatSpecifier = "F";
        //        int nNumDec = 0;
        //        cRoundNumber = "AwayFromZero";
        //        //cRoundNumber = "ToEven";
        //        //cRoundNumber = "ToZero";

        //        string cRoundedNumber = RoundToNumDecimals(ref nNumber, nNumDec, cFormatSpecifier);

        //        Debug.WriteLine($"Original: {number} - Rounded: {cRoundedNumber}");
        //    }
        //}
    }
}
