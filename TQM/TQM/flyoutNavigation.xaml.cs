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

        private void OnSelectedItem(object sender, SelectedItemChangedEventArgs e)
        {
            var item = e.SelectedItem as flyoutItemPage;

            if (item != null)
            {
                Detail = new NavigationPage((Page)Activator.CreateInstance(item.TargetPage));
                flyout.listview.SelectedItem = null;
                IsPresented = false;

                if (item.Title == "Add Machine")
                {

                    ObservableCollection<flyoutItemPage> flyItems = new ObservableCollection<flyoutItemPage>();

                    flyout.listview.ItemsSource = flyItems;

                    flyItems.Add(new flyoutItemPage
                    {
                        Title = "Yarn Count",
                        ImageSource = "yarn.jpeg",
                        TargetPage = typeof(yarnCount)
                    });

                }
                else if (item.Title == "Bluetooth Settings")
                {

                    ObservableCollection<flyoutItemPage> flyItems = new ObservableCollection<flyoutItemPage>();

                    flyout.listview.ItemsSource = flyItems;

                    flyItems.Add(new flyoutItemPage
                    {
                        Title = "Yarn Count",
                        ImageSource = "yarn.jpeg",
                        TargetPage = typeof(yarnCount)
                    });

                }

                if (item.Title == "Yarn Count")
                {

                    ObservableCollection<flyoutItemPage> flyItems = new ObservableCollection<flyoutItemPage>();

                    flyout.listview.ItemsSource = flyItems;

                    flyItems.Add(new flyoutItemPage
                    {
                        Title = "Add Machine",
                        ImageSource = "machine.png",
                        TargetPage = typeof(machinePage)
                    });

                }


            }
        }
    }
}