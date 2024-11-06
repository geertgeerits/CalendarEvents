namespace CalendarEvents
{
    public sealed partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Window dimensions and location for desktop apps
        /// </summary>
        /// <param name="activationState"></param>
        /// <returns></returns>
        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Microsoft.Maui.Controls.Window(new NavigationPage(new MainPage()))
            {
                X = 200,
                Y = 50,
                Height = 950,
                Width = 950,
                MinimumHeight = 800,
                MinimumWidth = 950,
                MaximumHeight = 1000,
                MaximumWidth = 950
            };

            return window;
        }
    }
}