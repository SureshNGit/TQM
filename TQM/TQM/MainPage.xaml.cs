using SQLite;
using System;
using TQM.Model;
using Xamarin.Forms;

namespace TQM
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

        }


        protected override void OnAppearing()
        {
            try
            {
                base.OnAppearing();
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    conn.CreateTable<UserModel>();
                    UserModel userinfo = conn.Table<UserModel>().FirstOrDefault();
                    if (userinfo == null)
                    {
                        btn_newUser.IsVisible = true;
                    }
                    else
                    {
                        btn_newUser.IsVisible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Notice", ex.Message.ToString(), "Ok");
            }
        }


        private void btn_login_Clicked(object sender, System.EventArgs e)
        {
            string username = entry_username.Text.Trim();
            string passwrod = entry_password.Text.Trim();
            using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            {
                conn.CreateTable<UserModel>();
                UserModel userinfo = conn.Table<UserModel>().Where(
                    UserModel => UserModel.userId == username &&
                    UserModel.password == passwrod &&
                    UserModel.isActive == true).FirstOrDefault();
                if (userinfo == null)
                {
                    DisplayAlert("Attention", "Incorrect username/password!!!", "OK");
                    return;
                }
                else
                {
                    userinfo.isloggedIn = true;
                    userinfo.lastLogin = System.DateTime.Now;
                    int row = conn.Update(userinfo);
                    if (row == 0)
                    {
                        DisplayAlert("Attention", "Unable to update logged out user information to database!!!", "OK");
                        return;
                    }
                }
                //conn.DropTable<YCTestModel>();
            }
            Navigation.PushAsync(new flyoutNavigation());
        }

        private void btn_newUser_Clicked(object sender, System.EventArgs e)
        {
            Navigation.PushAsync(new companyPage());
        }
    }
}
