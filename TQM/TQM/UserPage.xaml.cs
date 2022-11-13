using SQLite;
using SQLiteNetExtensions.Extensions;
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
        private Guid currentID;
        public UserPage()
        {
            try
            {
                InitializeComponent();
                SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation);
                //conn.DropTable<UserModel>();
                conn.CreateTable<CompanyModel>();
                List<CompanyModel> companieslist = conn.Table<CompanyModel>().ToList();
                picker_companyname.ItemsSource = companieslist;
                conn.Close();
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", "Error occurred!!! " + ex.Message.ToString(), "OK");
            }
        }

        protected override void OnAppearing()
        {
            try
            {
                base.OnAppearing();
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    conn.CreateTable<UserModel>();
                    UserModel userinfo = conn.Table<UserModel>().FirstOrDefault();
                    if (userinfo == null)
                    {
                        grid_searchuser.IsVisible = false;
                    }
                    else
                    {
                        grid_searchuser.IsVisible = true;
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Notice", ex.Message.ToString(), "Ok");
            }
        }

        public UserPage(UserModelView selectedItem)
        {
            try
            {
                InitializeComponent();
                SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation);
                conn.CreateTable<CompanyModel>();
                List<CompanyModel> companieslist = conn.Table<CompanyModel>().ToList();
                picker_companyname.ItemsSource = companieslist;
                conn.Close();
                picker_companyname.SelectedIndex = 1;
                entry_firstname.Text = selectedItem.firstname;
                entry_lastname.Text = selectedItem.lastname;
                entry_userid.Text = selectedItem.userId;
                entry_password.Text = selectedItem.password;
                switch_admin.IsToggled = selectedItem.isAdmin;
                switch_active.IsToggled = selectedItem.isActive;
                currentID = Guid.Empty;
                currentID = selectedItem.ID;
                btn_save.Text = "Update";
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", "Error occurred!!! " + ex.Message.ToString(), "OK");
            }
        }

        private void btn_save_Clicked(object sender, EventArgs e)
        {
            try
            {
                bool isFirstUser = false;
                var selectedItem = picker_companyname.SelectedItem as CompanyModel;
                if (selectedItem == null)
                {
                    DisplayAlert("Notice", "Please select company name!!!", "OK");
                    return;
                }
                if (entry_firstname.Text.Trim() == "" ||
                    entry_userid.Text.Trim() == "" ||
                    entry_password.Text.Trim() == "")
                {
                    DisplayAlert("Attention", "Please fill firstname, userid and password to proceed!!!", "OK");
                    return;
                }
                SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation);
                conn.CreateTable<UserModel>();
                var user = conn.Table<UserModel>().FirstOrDefault();
                if (user == null)
                {
                    isFirstUser = true;
                    if (!switch_admin.IsToggled)
                    {
                        conn.Close();
                        DisplayAlert("Attention", "First user should be admin. So enable 'Admin' button!!!", "OK");
                        return;
                    }
                    if (!switch_active.IsToggled)
                    {
                        conn.Close();
                        DisplayAlert("Attention", "First user should be active. So enable 'Active' button!!!", "OK");
                        return;
                    }
                }
                conn.CreateTable<CompanyModel>();
                List<CompanyModel> selectedCompany = conn.Table<CompanyModel>().Where(CompanyModel => CompanyModel.ID == selectedItem.ID).ToList();
                conn.Close();
                conn = new SQLiteConnection(App.DatabaseLocation);
                conn.CreateTable<UserModel>();
                List<UserModel> existingUserList = conn.GetAllWithChildren<UserModel>().FindAll(UserModel =>
                                ((UserModel.firstname.ToLower().Contains(entry_firstname.Text.ToLower())) &&
                                (UserModel.lastname.ToLower().Contains(entry_lastname.Text.ToLower()))) ||
                                (UserModel.userId.ToLower().Contains(entry_userid.Text.ToLower())));
                conn.Close();
                conn.Dispose();
                bool isloggenin = false;
                if (existingUserList.Count > 0)
                {
                    if (btn_save.Text == "Save")
                    {
                        DisplayAlert("Notice", "User already exists!!!", "OK");
                        return;
                    }
                    else if (btn_save.Text == "Update")
                    {
                        if (existingUserList.Count == 1 && existingUserList[0].ID != currentID)
                        {
                            DisplayAlert("Notice", "User already exists!!!", "OK");
                            return;
                        }
                        using (SQLiteConnection cxn = new SQLiteConnection(App.DatabaseLocation))
                        {
                            cxn.CreateTable<UserModel>();
                            UserModel currentuser = cxn.Table<UserModel>().Where(UserModel => UserModel.ID == currentID).FirstOrDefault();
                            if (currentuser.isloggedIn) { isloggenin = true; }
                        }
                    }

                }
                conn = new SQLiteConnection(App.DatabaseLocation);
                conn.CreateTable<UserModel>();
                List<UserModel> adminUserList = conn.GetAllWithChildren<UserModel>().FindAll(UserModel => UserModel.isAdmin == true);
                conn.Close();
                conn.Dispose();
                conn = new SQLiteConnection(App.DatabaseLocation);
                conn.CreateTable<UserModel>();
                string enteredUserID = entry_userid.Text.Trim();
                List<UserModel> existingUserIDList = conn.GetAllWithChildren<UserModel>().FindAll(UserModel => UserModel.userId.ToLower() == enteredUserID.ToLower());
                conn.Close();
                conn.Dispose();
                if (existingUserIDList.Count > 0 && btn_save.Text == "Save")
                {
                    DisplayAlert("Notice", "UserID already exists!!!", "OK");
                    return;
                }
                if (existingUserIDList.Count > 0 && btn_save.Text == "Update")
                {
                    if (existingUserIDList.Count == 1 && existingUserIDList[0].ID != currentID)
                    {
                        DisplayAlert("Notice", "UserID already exists!!!", "OK");
                        return;
                    }
                    else if (existingUserIDList.Count > 1)
                    {
                        DisplayAlert("Notice", "UserID already exists!!!", "OK");
                        return;
                    }
                }
                UserModel usermodel = new UserModel()
                {
                    companyID = selectedCompany[0].ID,
                    firstname = entry_firstname.Text,
                    lastname = entry_lastname.Text,
                    userId = entry_userid.Text,
                    password = entry_password.Text,
                    isAdmin = switch_admin.IsToggled,
                    isActive = switch_active.IsToggled,
                    isloggedIn = isloggenin,
                    createdate = DateTime.Now,
                    lastLogin = DateTime.Now,
                    dataSyncStatus = false
                };
                SQLiteConnection conn1 = new SQLiteConnection(App.DatabaseLocation);
                conn1.CreateTable<UserModel>();
                string msg = "saved";
                if (btn_save.Text.ToLower() == "update") { msg = "updated"; }
                int row = 0;
                if (btn_save.Text == "Save")
                {
                    usermodel.ID = Guid.NewGuid();
                    row = conn1.Insert(usermodel);
                }
                else if (btn_save.Text == "Update")
                {
                    if (currentID == Guid.Empty) { DisplayAlert("Failure", "User failed to be " + msg + "!!!", "OK"); return; }
                    if (adminUserList.Count == 1)
                    {
                        if (adminUserList[0].ID == currentID && switch_admin.IsToggled == false)
                        {
                            DisplayAlert("Failure", "Primary admin cannot be removed. Please add another admin user to remove admin role for this user!!!", "OK"); return;
                        }
                        if (adminUserList[0].ID == currentID && switch_active.IsToggled == false)
                        {
                            DisplayAlert("Failure", "Primary admin cannot be deactivated. Please add another admin user to deactivate this user!!!", "OK"); return;
                        }
                    }
                    usermodel.ID = currentID;
                    row = conn1.Update(usermodel);
                }
                if (row > 0)
                {
                    reset();
                    DisplayAlert("Success", "User " + msg + " successfully!!!", "OK");
                    if (isFirstUser)
                    {
                        Navigation.PushAsync(new MainPage());
                    }
                }
                else
                {
                    DisplayAlert("Failure", "User failed to be " + msg + "!!!", "OK");
                }
                conn1.Close();
                conn1.Dispose();
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", "Error occurred!!! " + ex.Message.ToString(), "OK");
            }
        }

        private void reset()
        {
            picker_companyname.SelectedItem = "";
            entry_firstname.Text = "";
            entry_lastname.Text = "";
            entry_userid.Text = "";
            entry_password.Text = "";
            switch_admin.IsToggled = false;
            switch_active.IsToggled = false;
            btn_save.Text = "Save";
            currentID = Guid.Empty;
        }


        private void btn_usersearch_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (entry_usersearch.Text == null)
                {
                    DisplayAlert("Attention", "Please enter user name to search!!!", "OK");
                    return;
                }

                if (entry_usersearch.Text.Trim() == "")
                {
                    DisplayAlert("Attention", "Please enter user name to search!!!", "OK");
                    return;
                }
                SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation);
                conn.CreateTable<UserModel>();
                List<UserModel> usersearchlist = conn.GetAllWithChildren<UserModel>().FindAll(UserModel =>
                                (UserModel.firstname.ToLower().Contains(entry_usersearch.Text.ToLower()) ||
                                UserModel.lastname.ToLower().Contains(entry_usersearch.Text.ToLower()) ||
                                UserModel.userId.ToLower().Contains(entry_usersearch.Text.ToLower())));
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
                    if (user.companyID == Guid.Empty)
                    {
                        DisplayAlert("Notice", "Invalid record!!!", "Ok");
                        return;
                    }
                    UserModelView userModelView = new UserModelView()
                    {
                        ID = user.ID,
                        firstname = user.firstname,
                        lastname = user.lastname,
                        displayname = dn,
                        userId = user.userId,
                        password = user.password,
                        isAdmin = user.isAdmin,
                        isActive = user.isActive,
                        companyID = user.companyID
                    };
                    usersearchlistmodified.Add(userModelView);
                }
                conn.Close();
                if (usersearchlist.Count > 0)
                {
                    Navigation.PushAsync(new UserSearch(usersearchlistmodified));
                }
                else
                {
                    DisplayAlert("Notice", "No record found!!!", "Ok");
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Notice", "Search failed!!! Error: " + ex.Message.ToString(), "Ok");
            }
        }

        private void btn_viewall_Clicked(object sender, EventArgs e)
        {
            try
            {
                SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation);
                conn.CreateTable<UserModel>();
                List<UserModel> usersearchlist = conn.GetAllWithChildren<UserModel>().FindAll(UserModel => UserModel.ID != Guid.Empty);
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
                    if (user.companyID == Guid.Empty)
                    {
                        DisplayAlert("Notice", "Invalid record!!!", "Ok");
                        return;
                    }
                    UserModelView userModelView = new UserModelView()
                    {
                        ID = user.ID,
                        firstname = user.firstname,
                        lastname = user.lastname,
                        displayname = dn,
                        userId = user.userId,
                        password = user.password,
                        isAdmin = user.isAdmin,
                        isActive = user.isActive,
                        companyID = user.companyID
                    };
                    usersearchlistmodified.Add(userModelView);
                }
                conn.Close();
                conn.Dispose();
                if (usersearchlist.Count > 0)
                {
                    Navigation.PushAsync(new UserSearch(usersearchlistmodified));
                }
                else
                {
                    DisplayAlert("Notice", "No record found!!!", "Ok");
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Notice", "Search failed!!! Error: " + ex.Message.ToString(), "Ok");
            }
        }

        private void btn_reset_Clicked(object sender, EventArgs e)
        {
            reset();
        }
    }
}