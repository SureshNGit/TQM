using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TQM
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class yarnCount : ContentPage
    {
        public yarnCount()
        {
            InitializeComponent();
        }

        private async void testYCButton_Clicked(object sender, EventArgs e)
        {
            //Check serial port communication
            //Check initial value is zero
            //Obtain the weight from weight scale via serial port
            bool seraialCommuication = true;
            if (seraialCommuication == true)
            {
                int initialWeight = 0;
                if (initialWeight == 0)
                {
                    string testCount_str = await DisplayPromptAsync("Enter Test Count", "", initialValue: "5", maxLength: 2, keyboard: Keyboard.Numeric);
                    if (testCount_str == null || testCount_str == "")
                    {
                        await DisplayAlert("Alert", "Test count cannot be zero!!!", "OK");
                        return;
                    }
                    int testCount = int.Parse(testCount_str);
                    Random rnd = new Random();
                    for (int i = 0; i < testCount; i++)
                    {
                        double currentWeigth = rnd.NextDouble();
                        string currentWeigth_Str = currentWeigth.ToString();
                        await DisplayAlert(currentWeigth_Str, "", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Alert", "Remove weigth to ensure zero!!!", "OK");
                }
            }
            else
            {
                await DisplayAlert("Alert", "Communication Error!!!", "OK");
            }
        }

        private void addMachine_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new machinePage());
        }
    }
}