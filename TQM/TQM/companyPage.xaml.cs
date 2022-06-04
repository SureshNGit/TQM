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
                conn.CreateTable<CompanyModel>();
                var company = conn.Table<CompanyModel>().FirstOrDefault(CompanyModel => CompanyModel.id == 1);
                entry_companyName.Text = company.Name;
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
                var company = conn.Table<CompanyModel>().FirstOrDefault(CompanyModel => CompanyModel.id == 1);
                int row = 0;
                if (company == null)
                {
                    row = conn.Insert(companymodel);
                }
                else
                {
                    companymodel.id = 1;
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