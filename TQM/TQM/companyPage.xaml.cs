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
                    createdate = DateTime.Now
                };

                SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation);
                conn.CreateTable<CompanyModel>();
                var company = conn.Table<CompanyModel>().FirstOrDefault();
                int row = 0;
                if (company == null)
                {
                    companymodel.ID = Guid.NewGuid();
                    row = conn.Insert(companymodel);
                }
                else
                {
                    companymodel.ID = company.ID;
                    row = conn.Update(companymodel);
                }
                conn.Close();
                if (row > 0)
                {
                    DisplayAlert("Success", "Company saved successfully!!!", "OK");
                }
                else
                {
                    DisplayAlert("Failure", "Company failed to be saved!!!", "OK");
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Notice", ex.Message.ToString(), "Ok");
            }
        }
    }
}