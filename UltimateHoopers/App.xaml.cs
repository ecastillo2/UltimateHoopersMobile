using System.Diagnostics;
using Microsoft.Maui.Controls;

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

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}