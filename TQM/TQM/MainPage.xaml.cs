using Xamarin.Forms;

namespace TQM
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void loginButton_Clicked(object sender, System.EventArgs e)
        {
            //Navigation.PushAsync(new homePage());
            Navigation.PushAsync(new flyoutNavigation());
        }
    }
}
