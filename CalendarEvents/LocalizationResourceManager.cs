using System.ComponentModel;

namespace CalendarEvents
{
    public class LocalizationResourceManager : INotifyPropertyChanged {
        private LocalizationResourceManager()
        {
            CalEventLang.Culture = CultureInfo.CurrentCulture;
        }

        public static LocalizationResourceManager Instance { get; } = new();

        public object this[string resourceKey]
            => CalEventLang.ResourceManager.GetObject(resourceKey, CalEventLang.Culture) ?? Array.Empty<byte>();

        public event PropertyChangedEventHandler PropertyChanged;

        public void SetCulture(CultureInfo culture)
        {
            CalEventLang.Culture = culture;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }
    }
}
