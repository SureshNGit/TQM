using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TQM
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class homePage : ContentPage
    {
        public homePage()
        {
            InitializeComponent();
        }

        private void yarnCount_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new flyoutNavigation());
        }
    }
}