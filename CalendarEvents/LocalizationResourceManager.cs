using System.ComponentModel;

namespace CalendarEvents
{
    public class LocalizationResourceManager : INotifyPropertyChanged {
        private LocalizationResourceManager()
        {
            FinLang.Culture = CultureInfo.CurrentCulture;
        }

        public static LocalizationResourceManager Instance { get; } = new();

        public object this[string resourceKey]
            => FinLang.ResourceManager.GetObject(resourceKey, FinLang.Culture) ?? Array.Empty<byte>();

        public event PropertyChangedEventHandler PropertyChanged;

        public void SetCulture(CultureInfo culture)
        {
            FinLang.Culture = culture;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }
    }
}
