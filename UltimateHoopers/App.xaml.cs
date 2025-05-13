using System.Diagnostics;

namespace UltimateHoopers
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Start with LoginPage instead of AppShell
            MainPage = new Pages.LoginPage();
        }
    }
}