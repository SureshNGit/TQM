using Android.Widget;
using Newtonsoft.Json.Linq;
using Plugin.Connectivity;
using RestSharp;
using SQLite;
using System;
using TQM.Model;
using Xamarin.Forms;

namespace TQM
{
    public partial class MainPage : ContentPage
    {
        private RunConfiguration runConfiguration = new RunConfiguration();

        public MainPage()
        {
            InitializeComponent();
        }

        protected override bool OnBackButtonPressed()
        {
            // Returning true cancels the back navigation.
            // If you want the app to exit instead, call System.Environment.Exit(0) here.
            return true;
        }

        protected override void OnAppearing()
        {
            try
            {
                base.OnAppearing();
                entry_username.Text = "";
                entry_password.Text = "";
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    conn.CreateTable<UserModel>();
                    UserModel userinfo = conn.Table<UserModel>().FirstOrDefault();

                    if (userinfo == null)
                    {
                        bool activation_status = activation(false);
                        if (activation_status)
                        {
                            btn_newUser.IsVisible = true;
                        }
                    }
                    else
                    {
                        bool activation_status = activation(true);
                        if (activation_status)
                        {
                            btn_newUser.IsVisible = false;
                            btn_login.IsVisible = true;
                        }
                        else
                        {
                            btn_newUser.IsVisible = true;
                            btn_login.IsVisible = false;
                        }
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
                    UserModel => UserModel.userId.ToLower() == username.ToLower() &&
                    UserModel.password == passwrod &&
                    UserModel.isActive == true).FirstOrDefault();
                if (userinfo == null)
                {
                    userinfo = conn.Table<UserModel>().Where(
                    UserModel => UserModel.userId.ToLower() == username.ToLower() &&
                    UserModel.password == passwrod &&
                    UserModel.isActive == false).FirstOrDefault();
                    if (userinfo == null)
                    {
                        DisplayAlert("Attention", "Incorrect username/password!!!", "OK");
                        return;
                    }
                    else
                    {
                        DisplayAlert("Attention", "Your ID is deactivated. Please contact admin to activate!!!", "OK");
                        return;
                    }
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
            UserModel userinfo = null;
            using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            {
                conn.CreateTable<UserModel>();
                userinfo = conn.Table<UserModel>().FirstOrDefault();
            }
            if (userinfo == null)
            {
                if (btn_newUser.Text == "Register")
                {
                    Navigation.PushAsync(new companyPage());
                }
                else if (btn_newUser.Text == "Activation Pending" || btn_newUser.Text == "Activate")
                {
                    activation(false);
                }
            }
            else
            {
                bool activation_status = activation(true);
                if (activation_status)
                {
                    btn_newUser.IsVisible = false;
                    btn_login.IsVisible = true;
                }
                else
                {
                    btn_newUser.IsVisible = true;
                    btn_login.IsVisible = false;
                }
            }
        }

        private bool activation(bool alreadyActivated = false)
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                if (!alreadyActivated)
                {
                    btn_newUser.Text = "Activate";
                    btn_newUser.BackgroundColor = Color.Red;
                    btn_newUser.TextColor = Color.White;
                    DisplayAlert("Attention", "Product Registration Failed. Check your internet connection", "Ok");
                    return true;
                }
                else
                {
                    Toast.MakeText(Android.App.Application.Context, "Internet not connected", ToastLength.Long).Show();
                    return true;
                }
            }
            else
            {
                string serialNo = "";
                serialNo = Android.Provider.Settings.Secure.GetString(Android.App.Application.Context.ContentResolver, Android.Provider.Settings.Secure.AndroidId);
                var client = new RestClient("https://myconsoleerp.herokuapp.com/tqm/activate");
                var request = new RestRequest();
                request.Method = Method.Post;
                //request.Timeout = Timeout.Infinite;
                request.AddParameter("userName", runConfiguration.getTQMAppUserID());
                request.AddParameter("serialNo", serialNo);
                if (alreadyActivated)
                {
                    request.AddParameter("purpose", "verify");
                }
                else
                {
                    request.AddParameter("purpose", "new");
                }
                RestResponse response = client.Execute(request);
                if (response.IsSuccessful)
                {
                    JObject jResponse = JObject.Parse(response.Content);
                    if (jResponse != null)
                    {
                        if (jResponse["status"].ToString() == "Activation Pending")
                        {
                            if (!alreadyActivated)
                            {
                                btn_newUser.Text = "Activation Pending";
                                btn_newUser.BackgroundColor = Color.Yellow;
                                btn_newUser.TextColor = Color.Black;
                                DisplayAlert("Attention", "Product activation initiated!!! Contact manufacturer for approval", "Ok");
                                return true;
                            }
                            else
                            {
                                Toast.MakeText(Android.App.Application.Context, "License expired!!! Please contact manufacturer", ToastLength.Long).Show();
                                return false;
                            }
                        }
                        else if (jResponse["status"].ToString() == "activated")
                        {
                            if (!alreadyActivated)
                            {
                                btn_newUser.Text = "Register";
                                btn_newUser.BackgroundColor = Color.Green;
                                btn_newUser.TextColor = Color.White;
                                DisplayAlert("Attention", "Product activation is successful!!!", "Ok");
                                return true;
                            }
                            else
                            {
                                return true;
                            }
                        }
                        //else if (jResponse["status"].ToString() == "re-activated")
                        //{
                        //    btn_newUser.Text = "Register";
                        //    btn_newUser.BackgroundColor = Color.Green;
                        //    btn_newUser.TextColor = Color.White;
                        //    DisplayAlert("Attention", "Product re-activation is successful!!!", "Ok");
                        //    return true;
                        //}
                        else
                        {
                            if (!alreadyActivated)
                            {
                                btn_newUser.Text = "Activate";
                                btn_newUser.BackgroundColor = Color.Red;
                                btn_newUser.TextColor = Color.White;
                                DisplayAlert("Attention", "Product Registration Failed. Contact Manufacturer for activation", "Ok");
                                return false;
                            }
                            else
                            {
                                Toast.MakeText(Android.App.Application.Context, "License expired!!! Please contact manufacturer", ToastLength.Long).Show();
                                return false;
                            }
                        }
                    }
                    else
                    {
                        if (!alreadyActivated)
                        {
                            btn_newUser.Text = "Activate";
                            btn_newUser.BackgroundColor = Color.Red;
                            btn_newUser.TextColor = Color.White;
                            DisplayAlert("Attention", "Product Registration Failed. Contact Manufacturer for activation", "Ok");
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    if (!alreadyActivated)
                    {
                        btn_newUser.Text = "Activate";
                        btn_newUser.BackgroundColor = Color.Red;
                        btn_newUser.TextColor = Color.White;
                        DisplayAlert("Attention", "Product Registration Failed. Contact Manufacturer for activation", "Ok");
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
        }
    }
}
