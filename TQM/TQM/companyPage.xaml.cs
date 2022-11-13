using SQLite;
using System;
using TQM.Model;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TQM
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class companyPage : ContentPage
    {

        public companyPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            try
            {
                base.OnAppearing();
                SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation);
                //conn.DropTable<CompanyModel>();
                //conn.DropTable<UserModel>();
                conn.CreateTable<CompanyModel>();
                var company = conn.Table<CompanyModel>().FirstOrDefault();
                if (company != null)
                {
                    btn_Save.Text = "Update";
                    entry_companyName.Text = company.Name;
                }
                conn.Close();
            }
            catch (Exception ex)
            {
                DisplayAlert("Notice", ex.Message.ToString(), "Ok");
            }
        }

        private void btn_Save_Clicked(object sender, EventArgs e)
        {
            try
            {
                CompanyModel companymodel = new CompanyModel()
                {
                    Name = entry_companyName.Text,
                    createdate = DateTime.Now,
                    dataSyncStatus = false
                };
                string msg = "saved";
                SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation);
                conn.CreateTable<CompanyModel>();
                var company = conn.Table<CompanyModel>().FirstOrDefault();
                int row = 0;
                if (company == null)
                {
                    companymodel.ID = Guid.NewGuid();
                    row = conn.Insert(companymodel);
                    btn_Save.Text = "Udpate";
                }
                else
                {
                    msg = "update";
                    companymodel.ID = company.ID;
                    row = conn.Update(companymodel);
                }
                conn.Close();
                if (row > 0)
                {
                    DisplayAlert("Success", "Company " + msg + " successfully!!!", "OK");
                    UserModel user = null;
                    using (SQLiteConnection conn1 = new SQLiteConnection(App.DatabaseLocation))
                    {
                        conn1.CreateTable<UserModel>();
                        user = conn1.Table<UserModel>().FirstOrDefault();
                    }
                    if (user == null)
                    {
                        Navigation.PushAsync(new UserPage());
                    }
                }
                else
                {
                    DisplayAlert("Failure", "Company failed to be " + msg + "!!!", "OK");
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Notice", ex.Message.ToString(), "Ok");
            }
        }
    }
}