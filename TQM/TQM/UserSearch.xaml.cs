
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
            this.userSearchResultView.ItemsSource = usersearchlist;
        }
    }
}