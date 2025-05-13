namespace UltimateHoopers
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new AppShell());

            // You can set window size here
            // window.Width = 400;
            // window.Height = 600;

            return window;
        }
    }
}