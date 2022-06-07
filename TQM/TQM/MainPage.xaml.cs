using Xamarin.Forms;

namespace TQM
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }


        private void btn_addcompany_Clicked(object sender, System.EventArgs e)
        {
            Navigation.PushAsync(new companyPage());
        }

        private void btn_adduser_Clicked(object sender, System.EventArgs e)
        {
            Navigation.PushAsync(new UserPage());
        }

        private void btn_login_Clicked(object sender, System.EventArgs e)
        {
            Navigation.PushAsync(new flyoutNavigation());
        }
    }
}
