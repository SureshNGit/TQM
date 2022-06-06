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
            var item = e.SelectedItem as flyoutItemPage;


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

                    ObservableCollection<flyoutItemPage> flyItems = new ObservableCollection<flyoutItemPage>();

                    flyout.listview.ItemsSource = flyItems;


                    flyItems.Add(new flyoutItemPage
                    {
                        Title = "Company",
                        ImageSource = "company.jpg",
                        TargetPage = typeof(companyPage)
                    });
                    flyItems.Add(new flyoutItemPage
                    {
                        Title = "User Management",
                        ImageSource = "user.png",
                        TargetPage = typeof(UserPage)
                    });
                    flyItems.Add(new flyoutItemPage
                    {
                        Title = "Yarn Count Settings",
                        ImageSource = "ycsettings.png",
                        TargetPage = typeof(YCSettings)
                    });
                    flyItems.Add(new flyoutItemPage
                    {
                        Title = "Yarn Count",
                        ImageSource = "yarn.jpeg",
                        TargetPage = typeof(yarnCount)
                    });
                    flyItems.Add(new flyoutItemPage
                    {
                        Title = "Exit",
                        ImageSource = "logout.png",
                        TargetPage = typeof(MainPage)
                    });
                }
                else if (item.Title == "User Management")
                {

                    ObservableCollection<flyoutItemPage> flyItems = new ObservableCollection<flyoutItemPage>();

                    flyout.listview.ItemsSource = flyItems;

                    flyItems.Add(new flyoutItemPage
                    {
                        Title = "Company",
                        ImageSource = "company.jpg",
                        TargetPage = typeof(companyPage)
                    });
                    flyItems.Add(new flyoutItemPage
                    {
                        Title = "Add Machine",
                        ImageSource = "machine.png",
                        TargetPage = typeof(machinePage)
                    });
                    flyItems.Add(new flyoutItemPage
                    {
                        Title = "Yarn Count Settings",
                        ImageSource = "ycsettings.png",
                        TargetPage = typeof(YCSettings)
                    });
                    flyItems.Add(new flyoutItemPage
                    {
                        Title = "Yarn Count",
                        ImageSource = "yarn.jpeg",
                        TargetPage = typeof(yarnCount)
                    });
                    flyItems.Add(new flyoutItemPage
                    {
                        Title = "Exit",
                        ImageSource = "logout.png",
                        TargetPage = typeof(MainPage)
                    });
                }
                else if (item.Title == "Company")
                {

                    ObservableCollection<flyoutItemPage> flyItems = new ObservableCollection<flyoutItemPage>();

                    flyout.listview.ItemsSource = flyItems;

                    flyItems.Add(new flyoutItemPage
                    {
                        Title = "User Management",
                        ImageSource = "user.png",
                        TargetPage = typeof(UserPage)
                    });
                    flyItems.Add(new flyoutItemPage
                    {
                        Title = "Add Machine",
                        ImageSource = "machine.png",
                        TargetPage = typeof(machinePage)
                    });
                    flyItems.Add(new flyoutItemPage
                    {
                        Title = "Yarn Count Settings",
                        ImageSource = "ycsettings.png",
                        TargetPage = typeof(YCSettings)
                    });
                    flyItems.Add(new flyoutItemPage
                    {
                        Title = "Yarn Count",
                        ImageSource = "yarn.jpeg",
                        TargetPage = typeof(yarnCount)
                    });
                    flyItems.Add(new flyoutItemPage
                    {
                        Title = "Exit",
                        ImageSource = "logout.png",
                        TargetPage = typeof(MainPage)
                    });
                }
                else if (item.Title == "Yarn Count Settings")
                {

                    ObservableCollection<flyoutItemPage> flyItems = new ObservableCollection<flyoutItemPage>();

                    flyout.listview.ItemsSource = flyItems;

                    flyItems.Add(new flyoutItemPage
                    {
                        Title = "Company",
                        ImageSource = "company.jpg",
                        TargetPage = typeof(companyPage)
                    });
                    flyItems.Add(new flyoutItemPage
                    {
                        Title = "User Management",
                        ImageSource = "user.png",
                        TargetPage = typeof(UserPage)
                    });
                    flyItems.Add(new flyoutItemPage
                    {
                        Title = "Add Machine",
                        ImageSource = "machine.png",
                        TargetPage = typeof(machinePage)
                    });
                    flyItems.Add(new flyoutItemPage
                    {
                        Title = "Yarn Count",
                        ImageSource = "yarn.jpeg",
                        TargetPage = typeof(yarnCount)
                    });
                    flyItems.Add(new flyoutItemPage
                    {
                        Title = "Exit",
                        ImageSource = "logout.png",
                        TargetPage = typeof(MainPage)
                    });
                }

                if (item.Title == "Yarn Count")
                {

                    ObservableCollection<flyoutItemPage> flyItems = new ObservableCollection<flyoutItemPage>();

                    flyout.listview.ItemsSource = flyItems;

                    flyItems.Add(new flyoutItemPage
                    {
                        Title = "Company",
                        ImageSource = "company.jpg",
                        TargetPage = typeof(companyPage)
                    });
                    flyItems.Add(new flyoutItemPage
                    {
                        Title = "User Management",
                        ImageSource = "user.png",
                        TargetPage = typeof(UserPage)
                    });
                    flyItems.Add(new flyoutItemPage
                    {
                        Title = "Add Machine",
                        ImageSource = "machine.png",
                        TargetPage = typeof(machinePage)
                    });
                    flyItems.Add(new flyoutItemPage
                    {
                        Title = "Yarn Count Settings",
                        ImageSource = "ycsettings.png",
                        TargetPage = typeof(YCSettings)
                    });
                    flyItems.Add(new flyoutItemPage
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