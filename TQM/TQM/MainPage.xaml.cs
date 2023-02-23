using Newtonsoft.Json.Linq;
using Plugin.Connectivity;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using RestSharp;
using SQLite;
using System;
using System.Diagnostics;
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

            //CheckLocationPermissions();

            //CheckStoragePermissions();

            //CheckNetworkStatePermissions();
        }


        private async void showAlert(string msg)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                DisplayAlert("Notice", msg, "Ok");
            });
        }


        private async void CheckLocationPermissions()
        {
            try
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync<LocationPermission>();
                if (status != PermissionStatus.Granted)
                {
                    //bool ret = false;
                    status = await CrossPermissions.Current.RequestPermissionAsync<LocationPermission>();
                    if (status == PermissionStatus.Granted)
                    {
                        //ret = true;
                    }
                    else if (status != PermissionStatus.Unknown)
                    {
                        //ret = false;
                        showAlert("Location permission was not granted!!!");
                    }
                    //return ret;
                }
                else
                {
                    //return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Location Permission Exception: " + ex.ToString());
                showAlert("Location permission was not granted!!!");
                //return false;
            }
        }

        private async void CheckStoragePermissions()
        {
            try
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync<StoragePermission>();
                if (status != PermissionStatus.Granted)
                {
                    //bool ret = false;
                    status = await CrossPermissions.Current.RequestPermissionAsync<StoragePermission>();
                    if (status == PermissionStatus.Granted)
                    {
                        //ret = true;
                    }
                    else if (status != PermissionStatus.Unknown)
                    {
                        //ret = false;
                        showAlert("Storage permission was not granted!!!");
                    }
                    //return ret;
                }
                else
                {
                    //return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Storage Permission Exception: " + ex.ToString());
                showAlert("Storage permission was not granted!!!");
                //return false;
            }
        }

        private async void CheckNetworkStatePermissions()
        {
            try
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync<PhonePermission>();
                if (status != PermissionStatus.Granted)
                {
                    //bool ret = false;
                    status = await CrossPermissions.Current.RequestPermissionAsync<PhonePermission>();
                    if (status == PermissionStatus.Granted)
                    {
                        //ret = true;
                    }
                    else if (status != PermissionStatus.Unknown)
                    {
                        //ret = false;
                        showAlert("Phone permission was not granted!!!");
                    }
                    //return ret;
                }
                else
                {
                    //return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Storage Permission Exception: " + ex.ToString());
                showAlert("Storage permission was not granted!!!");
                //return false;
            }
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
                        bool activation_status = activation();
                        if (activation_status)
                        {
                            btn_newUser.IsVisible = true;
                        }
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
            if (btn_newUser.Text == "Register")
            {
                Navigation.PushAsync(new companyPage());
            }
            else if (btn_newUser.Text == "Activation Pending" || btn_newUser.Text == "Activate")
            {
                activation();
            }
        }

        private bool activation()
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                btn_newUser.Text = "Activate";
                btn_newUser.BackgroundColor = Color.Red;
                btn_newUser.TextColor = Color.White;
                DisplayAlert("Attention", "Product Registration Failed. Check your internet connection", "Ok");
                return true;
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
                RestResponse response = client.Execute(request);
                if (response.IsSuccessful)
                {
                    JObject jResponse = JObject.Parse(response.Content);
                    if (jResponse != null)
                    {
                        if (jResponse["status"].ToString() == "Activation Pending")
                        {
                            btn_newUser.Text = "Activation Pending";
                            btn_newUser.BackgroundColor = Color.Yellow;
                            btn_newUser.TextColor = Color.Black;
                            DisplayAlert("Attention", "Product activation initiated!!! Contact manufacturer for approval", "Ok");
                            return true;
                        }
                        else if (jResponse["status"].ToString() == "activated")
                        {
                            btn_newUser.Text = "Register";
                            btn_newUser.BackgroundColor = Color.Green;
                            btn_newUser.TextColor = Color.White;
                            DisplayAlert("Attention", "Product activation is successful!!!", "Ok");
                            return true;
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
                            btn_newUser.Text = "Activate";
                            btn_newUser.BackgroundColor = Color.Red;
                            btn_newUser.TextColor = Color.White;
                            DisplayAlert("Attention", "Product Registration Failed. Contact Manufacturer for activation", "Ok");
                            return false;
                        }
                    }
                    else
                    {
                        btn_newUser.Text = "Activate";
                        btn_newUser.BackgroundColor = Color.Red;
                        btn_newUser.TextColor = Color.White;
                        DisplayAlert("Attention", "Product Registration Failed. Contact Manufacturer for activation", "Ok");
                        return false;
                    }
                }
                else
                {
                    btn_newUser.Text = "Activate";
                    btn_newUser.BackgroundColor = Color.Red;
                    btn_newUser.TextColor = Color.White;
                    DisplayAlert("Attention", "Product Registration Failed. Contact Manufacturer for activation", "Ok");
                    return false;
                }
            }
        }
    }
}
