
using System.Collections.Generic;
using TQM.ModelView;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TQM
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UserSearch : ContentPage
    {
        public UserSearch()
        {
            InitializeComponent();
        }

        public UserSearch(List<UserModelView> usersearchlist)
        {
            InitializeComponent();
            userSearchResultView.ItemsSource = usersearchlist;
        }

        private void userSearchResultView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var selectedItem = userSearchResultView.SelectedItem as UserModelView;
            if (selectedItem != null)
            {
                Navigation.PushAsync(new UserPage(selectedItem));
            }
        }
    }
}