using SQLite;
using System;
using System.Collections.Generic;
using TQM.Model;
using TQM.ModelView;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TQM
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UserPage : ContentPage
    {

        public UserPage()
        {
            InitializeComponent();
            SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation);
            conn.CreateTable<CompanyModel>();
            List<CompanyModel> companieslist = conn.Table<CompanyModel>().Where(CompanyModel => CompanyModel.id == 1).ToList();
            picker_companyname.ItemsSource = companieslist;
            conn.Close();
        }

        private void btn_save_Clicked(object sender, EventArgs e)
        {
            var selectedItem = picker_companyname.SelectedItem as CompanyModel;
            SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation);
            conn.CreateTable<CompanyModel>();
            List<CompanyModel> selectedCompany = conn.Table<CompanyModel>().Where(CompanyModel => CompanyModel.id == selectedItem.id).ToList();
            conn.Close();
            conn = new SQLiteConnection(App.DatabaseLocation);
            conn.CreateTable<UserModel>();
            List<UserModel> existingUserList = conn.Table<UserModel>().Where(UserModel =>
                            ((UserModel.firstname.ToLower().Contains(entry_firstname.Text.ToLower())) &&
                            (UserModel.lastname.ToLower().Contains(entry_lastname.Text.ToLower()))) ||
                            (UserModel.userId.ToLower().Contains(entry_userid.Text.ToLower()))
                            ).ToList();
            conn.Close();
            if (existingUserList.Count > 0)
            {
                DisplayAlert("Notice", "User already exists!!!", "OK");
                return;
            }
            UserModel usermodel = new UserModel()
            {
                companies = selectedCompany,
                firstname = entry_firstname.Text,
                lastname = entry_lastname.Text,
                userId = entry_userid.Text,
                password = entry_password.Text,
                isAdmin = switch_admin.IsEnabled,
                isActive = switch_active.IsEnabled,
                createdate = DateTime.Now,
                lastLogin = DateTime.Now
            };
            conn = new SQLiteConnection(App.DatabaseLocation);
            conn.CreateTable<UserModel>();
            int row = conn.Insert(usermodel);
            if (row > 0)
            {
                reset();
                DisplayAlert("Success", "User saved successfully!!!", "OK");
            }
            else
            {
                DisplayAlert("Failure", "User failed to be saved!!!", "OK");
            }
        }

        private void reset()
        {
            picker_companyname.SelectedItem = "";
            entry_firstname.Text = "";
            entry_lastname.Text = "";
            entry_userid.Text = "";
            entry_password.Text = "";
            switch_admin.IsEnabled = false;
            switch_active.IsEnabled = false;
        }

        //private void picker_companyname_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    var item = sender as Picker;
        //    var selectedItem = item.SelectedItem as CompanyModel;
        //    DisplayAlert("Notice", selectedItem.id.ToString(), "OK");
        //}

        private void btn_usersearch_Clicked(object sender, EventArgs e)
        {
            SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation);
            conn.CreateTable<UserModel>();
            List<UserModel> usersearchlist = conn.Table<UserModel>().Where(UserModel =>
                            (UserModel.firstname.ToLower().Contains(entry_usersearch.Text.ToLower()) ||
                            UserModel.lastname.ToLower().Contains(entry_usersearch.Text.ToLower()))).ToList();
            List<UserModelView> usersearchlistmodified = new List<UserModelView>();

            foreach (UserModel user in usersearchlist)
            {
                string dn = "";
                if (user.lastname.ToString() != "")
                {
                    dn = user.firstname + ", " + user.lastname + " [" + user.userId + "]";
                }
                else
                {
                    dn = user.firstname + " [" + user.userId + "]";
                }
                UserModelView userModelView = new UserModelView()
                {
                    id = user.id,
                    firstname = user.firstname,
                    lastname = user.lastname,
                    displayname = dn,
                    userId = user.userId,
                    password = user.password,
                    isAdmin = user.isAdmin,
                    isActive = user.isActive,
                    companies = user.companies
                };
                usersearchlistmodified.Add(userModelView);
            }

            conn.Close();
            if (usersearchlist != null)
            {
                Navigation.PushAsync(new UserSearch(usersearchlistmodified));
            }
            else
            {
                DisplayAlert("Notice", "No record found!!!", "Ok");
            }
        }
    }
}