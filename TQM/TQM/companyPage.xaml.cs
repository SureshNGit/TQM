using SQLite;
using System;
using System.Threading.Tasks;
using TQM.Model;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TQM
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class companyPage : ContentPage
    {
        private RunConfiguration runConfiguration = new RunConfiguration();
        public companyPage()
        {
            InitializeComponent();
           
            if (runConfiguration.getMoveReportToCloud())
            {
                lbl_ftpipaddress.IsVisible = false;
                entry_ftpipaddress.IsVisible = false;

                lbl_username.IsVisible = false;
                entry_username.IsVisible = false;

                lbl_password.IsVisible = false;
                entry_password.IsVisible = false;
            }
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
                    entry_ftpipaddress.Text = company.ftpIpAddress;
                    entry_username.Text = company.username;
                    entry_password.Text = company.password;
                }
                conn.Close();
            }
            catch (Exception ex)
            {
                DisplayAlert("Notice", ex.Message.ToString(), "Ok");
            }
        }

        private async void btn_Save_Clicked(object sender, EventArgs e)
        {
            try
            {
                String companyName = "";
                String ftpIPAddress = "";
                String username = "";
                String password = "";
                
                if (entry_companyName.Text.Trim() == "")
                {
                    await DisplayAlert("Attention", "Company Name cannot be blank!!!", "Ok");
                    return;
                }
                if (!runConfiguration.getMoveReportToCloud())
                {
                    if (entry_ftpipaddress.Text.Trim() == "")
                    {
                        await DisplayAlert("Attention", "FTP server IP address cannot be blank!!!", "Ok");
                        return;
                    }
                }
                companyName = entry_companyName.Text;
                ftpIPAddress = entry_ftpipaddress.Text;
                username = entry_username.Text;
                password = entry_password.Text;

                CompanyModel companymodel = new CompanyModel()
                {
                    Name = companyName,
                    ftpIpAddress = ftpIPAddress,
                    username = username,
                    password = password,
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
                    _ = DisplayAlert("Success", "Company " + msg + " successfully!!!", "OK");
                    UserModel user = null;
                    using (SQLiteConnection conn1 = new SQLiteConnection(App.DatabaseLocation))
                    {
                        conn1.CreateTable<UserModel>();
                        user = conn1.Table<UserModel>().FirstOrDefault();
                    }
                    if (user == null)
                    {
                        _ = Navigation.PushAsync(new UserPage());
                    }
                }
                else
                {
                    _ = DisplayAlert("Failure", "Company failed to be " + msg + "!!!", "OK");
                }
            }
            catch (Exception ex)
            {
                _ = DisplayAlert("Notice", ex.Message.ToString(), "Ok");
            }
        }
    }
}