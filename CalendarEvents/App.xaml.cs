namespace CalendarEvents
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            //MainPage = new AppShell();
            MainPage = new NavigationPage(new MainPage());
        }

        // Window dimensions and location for desktop apps.
        protected override Window CreateWindow(IActivationState activationState)
        {
            var window = base.CreateWindow(activationState);

            const int newHeight = 900;
            const int newWidth = 900;

            window.X = 200;
            window.Y = 50;

            window.Height = newHeight;
            window.Width = newWidth;

            window.MinimumHeight = 800;
            window.MinimumWidth = 900;

            return window;
        }
    }
}