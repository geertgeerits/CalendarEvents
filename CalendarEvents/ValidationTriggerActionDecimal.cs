namespace CalendarEvents
{
    public sealed class ValidationTriggerActionDecimal : TriggerAction<Entry>
    {
        public decimal MinValue { get; set; }
        public decimal MaxValue { get; set; }
        public int MaxDecimalPlaces { get; set; } = -1;

        protected override void Invoke(Entry entry)
        {
            // Validate the number of decimal places
            bool isDecimalPlacesValid = true;

            if (MaxDecimalPlaces > -1)
            {
                // Get the current culture's decimal separator
                string separator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

                int decimalCount = 0;
                if (!string.IsNullOrEmpty(entry.Text))
                {
                    string[] parts = entry.Text.Split(separator);
                    if (parts.Length == 2)
                    {
                        decimalCount = parts[1].Length;
                    }
                }

                isDecimalPlacesValid = decimalCount <= MaxDecimalPlaces;
            }

            // Validate the value of the number
            bool isValid = decimal.TryParse(entry.Text, out decimal result);
            isValid = isValid && result >= MinValue && result <= MaxValue;

            // Set the border color if the input is invalid
            if (entry.Parent is Border border && Application.Current?.Resources != null)
            {
                if (isValid && isDecimalPlacesValid)
                {
                    if (Application.Current.Resources.TryGetValue("EntryValidNumber", out var validColor) && validColor is Color validColorValue)
                    {
                        border.Stroke = validColorValue;
                    }
                }
                else
                {
                    if (Application.Current.Resources.TryGetValue("EntryInvalidNumber", out var InvalidColor) && InvalidColor is Color InvalidColorValue)
                    {
                        border.Stroke = InvalidColorValue;
                    }
                }
            }
        }
    }
}
