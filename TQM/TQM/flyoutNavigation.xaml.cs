using SQLite;
using System;
using System.Collections.ObjectModel;
using TQM.Model;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TQM
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class flyoutNavigation : FlyoutPage
    {
        public flyoutNavigation()
        {
            InitializeComponent();
            flyout.listview.ItemSelected += OnSelectedItem;
        }


        private async void OnSelectedItem(object sender, SelectedItemChangedEventArgs e)
        {
            var item = e.SelectedItem as MenuItem;
            UserModel userinfo = null;

            if (item != null)
            {
                if (item.TargetPage == null) { return; }
                if (item.Title == "Log Out")
                {
                    bool answer = await DisplayAlert("Attention", "Would you like to exit???", "Yes", "No");
                    if (answer)
                    {
                        using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                        {
                            conn.CreateTable<UserModel>();
                            userinfo = conn.Table<UserModel>().Where(UserModel => UserModel.isloggedIn == true).FirstOrDefault();
                            if (userinfo == null)
                            {
                                await DisplayAlert("Attention", "Unable to retrieve logged in user information from database!!!", "OK");
                                flyout.listview.SelectedItem = null;
                                return;
                            }
                            else
                            {

                                userinfo.isloggedIn = false;
                                int row = conn.Update(userinfo);
                                if (row == 0)
                                {
                                    await DisplayAlert("Attention", "Unable to update logged out user information to database!!!", "OK");
                                    flyout.listview.SelectedItem = null;
                                    return;
                                }
                            }
                        }
                        //System.Diagnostics.Process.GetCurrentProcess().Kill();
                        await Navigation.PushAsync(new MainPage());
                        return;
                    }
                    else
                    {
                        flyout.listview.SelectedItem = null;
                        return;
                    }
                }

                Detail = new NavigationPage((Page)Activator.CreateInstance(item.TargetPage));
                flyout.listview.SelectedItem = null;
                IsPresented = false;
                bool isAdmin = false;
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    conn.CreateTable<UserModel>();
                    userinfo = conn.Table<UserModel>().Where(UserModel => UserModel.isloggedIn == true).FirstOrDefault();
                    if (userinfo == null)
                    {
                        await DisplayAlert("Attention", "Unable to retrieve logged in user information from database!!!", "OK");
                        flyout.listview.SelectedItem = null;
                        return;
                    }
                    else
                    {

                        if (userinfo.isAdmin) { isAdmin = true; }
                    }
                }

                ObservableCollection<MenuItem> flyItems = new ObservableCollection<MenuItem>();
                flyout.listview.ItemsSource = flyItems;
                string displayName = userinfo.firstname;
                if (userinfo.lastname != null)
                {
                    displayName = userinfo.firstname + ", " + userinfo.lastname;
                }
                if (isAdmin)
                {
                    flyItems.Add(new MenuItem
                    {

                        Title = displayName,
                        ImageSource = "user.png",
                        TargetPage = null
                    });
                    flyItems.Add(new MenuItem
                    {
                        Title = "Company",
                        ImageSource = "",
                        TargetPage = typeof(companyPage)
                    });
                    flyItems.Add(new MenuItem
                    {
                        Title = "User Management",
                        ImageSource = "",
                        TargetPage = typeof(UserPage)
                    });
                    flyItems.Add(new MenuItem
                    {
                        Title = "Add Machine",
                        ImageSource = "",
                        TargetPage = typeof(machinePage)
                    });
                    flyItems.Add(new MenuItem
                    {
                        Title = "Settings",
                        ImageSource = "",
                        TargetPage = typeof(YCSettings)
                    });
                    flyItems.Add(new MenuItem
                    {
                        Title = "Strength Test",
                        ImageSource = "",
                        TargetPage = typeof(StrengthAnalyzer)
                    });
                    flyItems.Add(new MenuItem
                    {
                        Title = "Reports",
                        ImageSource = "",
                        TargetPage = typeof(Report)
                    });
                    flyItems.Add(new MenuItem
                    {
                        Title = "Log Out",
                        ImageSource = "",
                        TargetPage = typeof(MainPage)
                    });
                    flyItems.Add(new MenuItem
                    {
                        Title = "Version: 1.0",
                        ImageSource = "",
                        TargetPage = null
                    });
                }
                else
                {
                    flyItems.Add(new MenuItem
                    {

                        Title = displayName,
                        ImageSource = "user.png",
                        TargetPage = null
                    });
                    flyItems.Add(new MenuItem
                    {
                        Title = "Strength Test",
                        ImageSource = "",
                        TargetPage = typeof(StrengthAnalyzer)
                    });
                    flyItems.Add(new MenuItem
                    {
                        Title = "Reports",
                        ImageSource = "",
                        TargetPage = typeof(Report)
                    });
                    flyItems.Add(new MenuItem
                    {
                        Title = "Log Out",
                        ImageSource = "",
                        TargetPage = typeof(MainPage)
                    });
                    flyItems.Add(new MenuItem
                    {
                        Title = "Version: 1.0",
                        ImageSource = "",
                        TargetPage = null
                    });
                }
            }
        }
    }
}