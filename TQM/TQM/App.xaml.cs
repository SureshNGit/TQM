using Xamarin.Forms;

namespace TQM
{
    public partial class App : Application
    {
        public static string DatabaseLocation = string.Empty;
        public App()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NjU0NjA4QDMyMzAyZTMxMmUzMEUvREZNRzVVcUh1WTgwVUp2K2Evdk9Fb0h0Q1lxWGp2VVhaMUhYSDhoOUk9");

            InitializeComponent();

            MainPage = new NavigationPage(new MainPage());
        }

        public App(string databasePath)
        {
            InitializeComponent();

            MainPage = new NavigationPage(new MainPage());

            DatabaseLocation = databasePath;
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
