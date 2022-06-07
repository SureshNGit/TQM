using System;
using System.Collections.ObjectModel;
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


            if (item != null)
            {
                if (item.Title == "Exit")
                {
                    bool answer = await DisplayAlert("Attention", "Would you like to exit???", "Yes", "No");
                    if (answer) { System.Diagnostics.Process.GetCurrentProcess().Kill(); } else { flyout.listview.SelectedItem = null; return; }
                }

                Detail = new NavigationPage((Page)Activator.CreateInstance(item.TargetPage));
                flyout.listview.SelectedItem = null;
                IsPresented = false;

                if (item.Title == "Add Machine")
                {

                    ObservableCollection<MenuItem> flyItems = new ObservableCollection<MenuItem>();

                    flyout.listview.ItemsSource = flyItems;


                    flyItems.Add(new MenuItem
                    {
                        Title = "Company",
                        ImageSource = "company.jpg",
                        TargetPage = typeof(companyPage)
                    });
                    flyItems.Add(new MenuItem
                    {
                        Title = "User Management",
                        ImageSource = "user.png",
                        TargetPage = typeof(UserPage)
                    });
                    flyItems.Add(new MenuItem
                    {
                        Title = "Yarn Count Settings",
                        ImageSource = "ycsettings.png",
                        TargetPage = typeof(YCSettings)
                    });
                    flyItems.Add(new MenuItem
                    {
                        Title = "Yarn Count",
                        ImageSource = "yarn.jpeg",
                        TargetPage = typeof(yarnCount)
                    });
                    flyItems.Add(new MenuItem
                    {
                        Title = "Exit",
                        ImageSource = "logout.png",
                        TargetPage = typeof(MainPage)
                    });
                }
                else if (item.Title == "User Management")
                {

                    ObservableCollection<MenuItem> flyItems = new ObservableCollection<MenuItem>();

                    flyout.listview.ItemsSource = flyItems;

                    flyItems.Add(new MenuItem
                    {
                        Title = "Company",
                        ImageSource = "company.jpg",
                        TargetPage = typeof(companyPage)
                    });
                    flyItems.Add(new MenuItem
                    {
                        Title = "Add Machine",
                        ImageSource = "machine.png",
                        TargetPage = typeof(machinePage)
                    });
                    flyItems.Add(new MenuItem
                    {
                        Title = "Yarn Count Settings",
                        ImageSource = "ycsettings.png",
                        TargetPage = typeof(YCSettings)
                    });
                    flyItems.Add(new MenuItem
                    {
                        Title = "Yarn Count",
                        ImageSource = "yarn.jpeg",
                        TargetPage = typeof(yarnCount)
                    });
                    flyItems.Add(new MenuItem
                    {
                        Title = "Exit",
                        ImageSource = "logout.png",
                        TargetPage = typeof(MainPage)
                    });
                }
                else if (item.Title == "Company")
                {

                    ObservableCollection<MenuItem> flyItems = new ObservableCollection<MenuItem>();

                    flyout.listview.ItemsSource = flyItems;

                    flyItems.Add(new MenuItem
                    {
                        Title = "User Management",
                        ImageSource = "user.png",
                        TargetPage = typeof(UserPage)
                    });
                    flyItems.Add(new MenuItem
                    {
                        Title = "Add Machine",
                        ImageSource = "machine.png",
                        TargetPage = typeof(machinePage)
                    });
                    flyItems.Add(new MenuItem
                    {
                        Title = "Yarn Count Settings",
                        ImageSource = "ycsettings.png",
                        TargetPage = typeof(YCSettings)
                    });
                    flyItems.Add(new MenuItem
                    {
                        Title = "Yarn Count",
                        ImageSource = "yarn.jpeg",
                        TargetPage = typeof(yarnCount)
                    });
                    flyItems.Add(new MenuItem
                    {
                        Title = "Exit",
                        ImageSource = "logout.png",
                        TargetPage = typeof(MainPage)
                    });
                }
                else if (item.Title == "Yarn Count Settings")
                {

                    ObservableCollection<MenuItem> flyItems = new ObservableCollection<MenuItem>();

                    flyout.listview.ItemsSource = flyItems;

                    flyItems.Add(new MenuItem
                    {
                        Title = "Company",
                        ImageSource = "company.jpg",
                        TargetPage = typeof(companyPage)
                    });
                    flyItems.Add(new MenuItem
                    {
                        Title = "User Management",
                        ImageSource = "user.png",
                        TargetPage = typeof(UserPage)
                    });
                    flyItems.Add(new MenuItem
                    {
                        Title = "Add Machine",
                        ImageSource = "machine.png",
                        TargetPage = typeof(machinePage)
                    });
                    flyItems.Add(new MenuItem
                    {
                        Title = "Yarn Count",
                        ImageSource = "yarn.jpeg",
                        TargetPage = typeof(yarnCount)
                    });
                    flyItems.Add(new MenuItem
                    {
                        Title = "Exit",
                        ImageSource = "logout.png",
                        TargetPage = typeof(MainPage)
                    });
                }

                if (item.Title == "Yarn Count")
                {

                    ObservableCollection<MenuItem> flyItems = new ObservableCollection<MenuItem>();

                    flyout.listview.ItemsSource = flyItems;

                    flyItems.Add(new MenuItem
                    {
                        Title = "Company",
                        ImageSource = "company.jpg",
                        TargetPage = typeof(companyPage)
                    });
                    flyItems.Add(new MenuItem
                    {
                        Title = "User Management",
                        ImageSource = "user.png",
                        TargetPage = typeof(UserPage)
                    });
                    flyItems.Add(new MenuItem
                    {
                        Title = "Add Machine",
                        ImageSource = "machine.png",
                        TargetPage = typeof(machinePage)
                    });
                    flyItems.Add(new MenuItem
                    {
                        Title = "Yarn Count Settings",
                        ImageSource = "ycsettings.png",
                        TargetPage = typeof(YCSettings)
                    });
                    flyItems.Add(new MenuItem
                    {
                        Title = "Exit",
                        ImageSource = "logout.png",
                        TargetPage = typeof(MainPage)
                    });


                }


            }
        }
    }
}