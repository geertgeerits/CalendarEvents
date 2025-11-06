namespace CalendarEvents
{
    public sealed class NumericValidationTriggerAction : TriggerAction<Entry>
    {
        public decimal MinValue { get; set; }
        public decimal MaxValue { get; set; }

        protected override void Invoke(Entry entry)
        {
            // Validate the number
            bool isValid = decimal.TryParse(entry.Text, out decimal result);
            isValid = isValid && result >= MinValue && result <= MaxValue;

            // Set the border color if the input is invalid
            if (entry.Parent is Border border && Application.Current?.Resources != null)
            {
                if (isValid)
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
