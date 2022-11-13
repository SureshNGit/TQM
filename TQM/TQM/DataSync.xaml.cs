using Plugin.Connectivity;
using RestSharp;
using SQLite;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TQM.Model;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TQM
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DataSync : ContentPage
    {
        RunConfiguration runConfiguration = new RunConfiguration();
        public DataSync()
        {
            InitializeComponent();
        }



        private bool checkConnection()
        {
            try
            {
                if (CrossConnectivity.Current.IsConnected)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }


        private string getDateString(DateTime inputDateTime)
        {
            return inputDateTime.Year.ToString() + "-"
                    + inputDateTime.Month.ToString() + "-"
                    + inputDateTime.Day.ToString() + " "
                    + inputDateTime.Hour.ToString() + ":"
                    + inputDateTime.Minute.ToString() + ":"
                    + inputDateTime.Second.ToString();
        }

        private async void updateProgress(string msg, int width, Color lblColor)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                lbl_backupProgress.BackgroundColor = lblColor;
                lbl_backupProgress.Margin = new Thickness(100, 200, width, 0);
                lbl_backupProgress.Text = msg;
            });
        }

        private Task<bool> syncDataCompanyModel()
        {
            updateProgress("", 900, Color.White);
            if (!checkConnection())
            {
                updateProgress("No internet! Backup failed!!!", 100, Color.Red);
                return Task.FromResult(false);
            }
            var client = new RestClient("https://myconsoleerp.herokuapp.com/tqm/datasync");
            var request = new RestRequest();
            request.Method = Method.Post;
            request.Timeout = Timeout.Infinite;
            //Company Model
            using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            {
                int syncDataCount = 0;
                conn.CreateTable<CompanyModel>();
                List<CompanyModel> companyList = conn.Table<CompanyModel>().Where(CompanyModel => CompanyModel.dataSyncStatus == false).ToList();
                foreach (CompanyModel company in companyList)
                {
                    updateProgress("", 900, Color.BlueViolet);
                    if (!checkConnection())
                    {
                        updateProgress("No internet! Backup failed!!!", 100, Color.Red);
                        return Task.FromResult(false);
                    }
                    request.AddParameter("modelName", "Customer");
                    request.AddParameter("userName", runConfiguration.getTQMAppUserID());
                    request.AddParameter("GUID", company.ID);
                    request.AddParameter("Name", company.Name);
                    request.AddParameter("createdate", getDateString(company.createdate));
                    RestResponse response = client.Execute(request);
                    if (response.IsSuccessful)
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.Created ||
                            response.StatusCode == System.Net.HttpStatusCode.Accepted)
                        {
                            company.dataSyncStatus = true;
                            int row = 0;
                            row = conn.Update(company);
                            if (row > 0)
                            {
                                syncDataCount = syncDataCount + 1;
                            }
                        }
                    }
                    updateProgress("Uploading.....", 100, Color.BlueViolet);
                }
                if (syncDataCount == companyList.Count)
                {
                    updateProgress("Backup completed!!!", 100, Color.Green);
                    return Task.FromResult(true);
                }
                else if (syncDataCount == 0)
                {
                    updateProgress("Backup failed!!!", 100, Color.Red);
                    return Task.FromResult(false);
                }
                else
                {
                    updateProgress("Backup completed partially!!!", 100, Color.Yellow);
                    return Task.FromResult(true);
                }
            }
        }

        private async void backupButton_Clicked(object sender, EventArgs e)
        {
            bool companyBackup = await syncDataCompanyModel();
            if (companyBackup)
            {
                bool userModelBackup = await syncDataUserModel();
            }
        }

        private Task<bool> syncDataUserModel()
        {
            updateProgress("", 900, Color.White);
            if (!checkConnection())
            {
                updateProgress("No internet! Backup failed!!!", 100, Color.Red);
                return Task.FromResult(false);
            }
            var client = new RestClient("https://myconsoleerp.herokuapp.com/tqm/datasync");
            var request = new RestRequest();
            request.Method = Method.Post;
            request.Timeout = Timeout.Infinite;
            //Company Model
            using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            {
                int syncDataCount = 0;
                conn.CreateTable<UserModel>();
                List<UserModel> userList = conn.Table<UserModel>()
                    .Where(UserModel => UserModel.dataSyncStatus == false).ToList();
                foreach (UserModel user in userList)
                {
                    updateProgress("", 900, Color.BlueViolet);
                    if (!checkConnection())
                    {
                        updateProgress("No internet! Backup failed!!!", 100, Color.Red);
                        return Task.FromResult(false);
                    }
                    request.AddParameter("modelName", "TQMUser");
                    request.AddParameter("userName", runConfiguration.getTQMAppUserID());
                    request.AddParameter("GUID", user.ID);
                    request.AddParameter("firstname", user.firstname);
                    request.AddParameter("lastname", user.lastname);
                    request.AddParameter("userId", user.userId.ToString());
                    request.AddParameter("password", user.password);
                    request.AddParameter("isAdmin", user.isAdmin.ToString());
                    request.AddParameter("isActive", user.isActive.ToString());
                    request.AddParameter("companyID", user.companyID);
                    request.AddParameter("lastLogin", getDateString(user.lastLogin));
                    request.AddParameter("createdate", getDateString(user.createdate));
                    RestResponse response = client.Execute(request);
                    if (response.IsSuccessful)
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.Created ||
                            response.StatusCode == System.Net.HttpStatusCode.Accepted)
                        {
                            user.dataSyncStatus = true;
                            int row = 0;
                            row = conn.Update(user);
                            if (row > 0)
                            {
                                syncDataCount = syncDataCount + 1;
                            }
                        }
                    }
                    updateProgress("Uploading.....", 100, Color.BlueViolet);
                }
                if (syncDataCount == userList.Count)
                {
                    updateProgress("Backup completed!!!", 100, Color.Green);
                    return Task.FromResult(true);
                }
                else if (syncDataCount == 0)
                {
                    updateProgress("Backup failed!!!", 100, Color.Red);
                    return Task.FromResult(false);
                }
                else
                {
                    updateProgress("Backup completed partially!!!", 100, Color.Yellow);
                    return Task.FromResult(true);
                }
            }
        }
    }
}