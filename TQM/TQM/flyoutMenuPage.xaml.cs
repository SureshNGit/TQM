using SQLite;
using System;
using System.Collections.ObjectModel;
using TQM.Model;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TQM
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class flyoutMenuPage : ContentPage
    {
        public flyoutMenuPage()
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
                    UserModel userinfo = conn.Table<UserModel>().Where(UserModel => UserModel.isloggedIn == true).FirstOrDefault();
                    if (userinfo == null)
                    {
                        DisplayAlert("Attention", "Unable to retrieve logged in user information from database!!!", "OK");
                        return;
                    }
                    else
                    {
                        string displayName = userinfo.firstname;
                        if (userinfo.lastname != null)
                        {
                            displayName = userinfo.firstname + ", " + userinfo.lastname;
                        }
                        listview.ItemsSource = null;
                        ObservableCollection<MenuItem> flyItems = new ObservableCollection<MenuItem>();
                        if (!userinfo.isAdmin)
                        {
                            flyItems.Add(new MenuItem
                            {
                                Title = displayName,
                                ImageSource = "user.png",
                                TargetPage = null
                            });
                            flyItems.Add(new MenuItem
                            {
                                Title = "Settings",
                                ImageSource = "",
                                TargetPage = typeof(YCSettings)
                            });
                            flyItems.Add(new MenuItem
                            {
                                Title = "Count+Strength",
                                ImageSource = "",
                                TargetPage = typeof(yarnCountWithCSP)
                            });
                            flyItems.Add(new MenuItem
                            {
                                Title = "Count",
                                ImageSource = "",
                                TargetPage = typeof(yarnCount)
                            });
                            //flyItems.Add(new MenuItem
                            //{
                            //    Title = "Stretch",
                            //    ImageSource = "",
                            //    TargetPage = typeof(StretchPage)
                            //});
                            //flyItems.Add(new MenuItem
                            //{
                            //    Title = "Noils",
                            //    ImageSource = "",
                            //    TargetPage = typeof(NOILS)
                            //});
                            flyItems.Add(new MenuItem
                            {
                                Title = "Reports",
                                ImageSource = "",
                                TargetPage = typeof(Report)
                            });
                            //flyItems.Add(new MenuItem
                            //{
                            //    Title = "BackUp",
                            //    ImageSource = "",
                            //    TargetPage = typeof(DataSync)
                            //});
                            flyItems.Add(new MenuItem
                            {
                                Title = "Log Out",
                                ImageSource = "",
                                TargetPage = typeof(MainPage)
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
                                Title = "Count+Strength",
                                ImageSource = "",
                                TargetPage = typeof(yarnCountWithCSP)
                            });
                            flyItems.Add(new MenuItem
                            {
                                Title = "Count",
                                ImageSource = "",
                                TargetPage = typeof(yarnCount)
                            });
                            //flyItems.Add(new MenuItem
                            //{
                            //    Title = "Stretch",
                            //    ImageSource = "",
                            //    TargetPage = typeof(StretchPage)
                            //});
                            //flyItems.Add(new MenuItem
                            //{
                            //    Title = "Noils",
                            //    ImageSource = "",
                            //    TargetPage = typeof(NOILS)
                            //});
                            flyItems.Add(new MenuItem
                            {
                                Title = "Reports",
                                ImageSource = "",
                                TargetPage = typeof(Report)
                            });
                            //flyItems.Add(new MenuItem
                            //{
                            //    Title = "BackUp",
                            //    ImageSource = "",
                            //    TargetPage = typeof(DataSync)
                            //});
                            flyItems.Add(new MenuItem
                            {
                                Title = "Log Out",
                                ImageSource = "",
                                TargetPage = typeof(MainPage)
                            });
                        }
                        listview.ItemsSource = flyItems;
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Notice", ex.Message.ToString(), "Ok");
            }
        }
    }
}