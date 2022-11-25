using Android.Widget;
using Plugin.Connectivity;
using RestSharp;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using TQM.Model;
using Xamarin.Forms;

namespace TQM
{
    public partial class App : Application
    {
        public static string DatabaseLocation = string.Empty;
        private static Stopwatch stopWatch = new Stopwatch();
        private const int defaultTimespan = 30;
        private bool backupStatus = false;
        RunConfiguration runConfiguration = new RunConfiguration();
        public App()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NjU0NjA4QDMyMzAyZTMxMmUzMEUvREZNRzVVcUh1WTgwVUp2K2Evdk9Fb0h0Q1lxWGp2VVhaMUhYSDhoOUk9");

            InitializeComponent();

            MainPage = new NavigationPage(new MainPage());
        }

        public App(string databasePath)
        {
            InitializeComponent();

            MainPage = new NavigationPage(new MainPage());

            DatabaseLocation = databasePath;
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

        private async void showAlert(string title, string msg, string buttonText)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (title.ToLower().Contains("fail"))
                {
                    Toast.MakeText(Android.App.Application.Context, msg, ToastLength.Short).Show();
                }
            });
        }

        private async void showAlert(string msg)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Toast.MakeText(Android.App.Application.Context, msg, ToastLength.Short).Show();
            });
        }

        protected override void OnStart()
        {
            // On start runs when your application launches from a closed state, 

            if (!stopWatch.IsRunning)
            {
                stopWatch.Start();
            }

            Device.StartTimer(new TimeSpan(0, 0, 1), () =>
            {
                // Logic for logging out if the device is inactive for a period of time.

                if (stopWatch.IsRunning && stopWatch.Elapsed.Seconds >= defaultTimespan && backupStatus == false)
                {
                    //prepare to perform your data pull here as we have hit the 1 minute mark   

                    // Perform your long running operations here.

                    //Device.BeginInvokeOnMainThread(() =>
                    //{
                    //    // If you need to do anything with your UI, you need to wrap it in this.
                    //});

                    if (!checkConnection())
                    {
                        showAlert("Backup failed!!!", "Please connect to Internet for Back-up", "OK");
                    }
                    else
                    {
                        CancellationTokenSource src = new CancellationTokenSource();
                        CancellationToken ct = src.Token;
                        ct.Register(() => Debug.WriteLine("ConnectBluetoothToken"));
                        Task.Run(async () => await syncDataCompanyModel(), ct);
                        src.Cancel();
                    }

                    stopWatch.Restart();
                }

                // Always return true as to keep our device timer running.
                return true;
            });
        }

        protected override void OnSleep()
        {
            // Ensure our stopwatch is reset so the elapsed time is 0.
            backupStatus = false;
            stopWatch.Reset();
        }

        protected override void OnResume()
        {
            // App enters the foreground so start our stopwatch again.
            backupStatus = false;
            stopWatch.Start();
        }

        private async Task syncDataCompanyModel()
        {
            try
            {
                //toggleBackUpButton(false);
                backupStatus = true;
                bool status = false;
                bool poorInternet = false;
                //updateProgress("", 900, Color.White);
                if (!checkConnection())
                {
                    //updateProgress("No internet! Backup failed!!!", 100, Color.Red);
                    //toggleLoading(false);
                    backupStatus = false;
                    showAlert("Backup failed!!!", "Please connect to Internet for Back-up", "OK");
                    return;
                }
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    int syncDataCount = 0;
                    conn.CreateTable<CompanyModel>();
                    List<CompanyModel> companyList = conn.Table<CompanyModel>().Where(CompanyModel => CompanyModel.dataSyncStatus == false).ToList();
                    foreach (CompanyModel company in companyList)
                    {
                        //updateProgress("", 900, Color.BlueViolet);
                        if (!checkConnection())
                        {
                            //updateProgress("No internet! Backup failed!!!", 100, Color.Red);
                            backupStatus = false;
                            showAlert("Backup failed!!!", "Please connect to Internet for Back-up", "OK");
                            poorInternet = true;
                            break;
                        }
                        var client = new RestClient("https://myconsoleerp.herokuapp.com/tqm/datasync");
                        var request = new RestRequest();
                        request.Method = Method.Post;
                        request.Timeout = Timeout.Infinite;
                        request.AddParameter("modelName", "TQMCustomer");
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
                        //updateProgress("Uploading.....", 100, Color.BlueViolet);
                    }

                    if (syncDataCount == companyList.Count)
                    {
                        //success
                        //updateProgress("User: Backup completed!!!", 100, Color.Green);
                        status = true;
                    }
                    else if (syncDataCount != companyList.Count)
                    {
                        if (syncDataCount > 0)
                        {
                            if (poorInternet)
                            {
                                //Poor internet but few record(s) pushed to server
                                //updateProgress("Company: Backup partially completed but failed due to poor internet connection!!!", 100, Color.Yellow);
                                showAlert("Attention", "Company: Backup partially completed but failed due to poor internet connection!!!", "OK");
                            }
                            else
                            {
                                //Internet is good but few records not pushed to server
                                if (!checkConnection())
                                {
                                    //updateProgress("Company: Backup partially completed but failed due to poor internet connection!!!", 100, Color.Yellow);
                                    showAlert("Attention", "Company: Backup partially completed but failed due to poor internet connection!!!", "OK");
                                }
                                else
                                {
                                    //updateProgress("Company: Backup completed partially but failed", 100, Color.Red);
                                    showAlert("Attention", "Company: Backup completed partially but failed", "OK");
                                }

                            }
                        }
                        else
                        {
                            if (poorInternet)
                            {
                                //Poor internet, so none of the record(s) pushed to server
                                //updateProgress("Company: Backup failed due to poor internet connection!!!", 100, Color.Yellow);
                                showAlert("Attention", "Company: Backup failed due to poor internet connection!!!", "OK");
                            }
                            else
                            {
                                //Internet is good but none of the record(s) pushed to server
                                if (!checkConnection())
                                {
                                    //updateProgress("Company: Backup failed due to poor internet connection!!!", 100, Color.Yellow);
                                    showAlert("Attention", "Company: Backup failed due to poor internet connection!!!", "OK");
                                }
                                else
                                {
                                    //updateProgress("Company: Backup failed. Please contact manufacturer!!!", 100, Color.Red);
                                    showAlert("Attention", "Company: Backup failed. Please contact manufacturer!!!", "OK");
                                }
                            }

                        }
                    }
                }
                if (status)
                {
                    CancellationTokenSource src = new CancellationTokenSource();
                    CancellationToken ct = src.Token;
                    ct.Register(() => Debug.WriteLine("ConnectBluetoothToken"));
                    await Task.Run(async () => await syncDataUserModel(), ct);
                    src.Cancel();
                }
                else
                {
                    //toggleLoading(false);
                    backupStatus = false;
                }
            }
            catch (Exception ex)
            {
                //updateProgress("Backup failed - " + ex.Message.ToString(), 100, Color.Red);
                //toggleLoading(false);
                backupStatus = false;
                showAlert("Backup Failed!!!", "Error : " + ex.Message.ToString(), "OK");

            }
        }

        private async Task syncDataUserModel()
        {
            try
            {
                bool status = false;
                bool poorInternet = false;
                //updateProgress("", 900, Color.White);
                if (!checkConnection())
                {
                    backupStatus = false;
                    showAlert("Backup failed!!!", "Please connect to Internet for Back-up", "OK");
                    //updateProgress("No internet! Backup failed!!!", 100, Color.Red);
                    ////toggleLoading(false);
                    return;
                }
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    int syncDataCount = 0;
                    conn.CreateTable<UserModel>();
                    List<UserModel> userList = conn.Table<UserModel>()
                        .Where(UserModel => UserModel.dataSyncStatus == false).ToList();
                    foreach (UserModel user in userList)
                    {
                        //updateProgress("", 900, Color.BlueViolet);
                        if (!checkConnection())
                        {
                            backupStatus = false;
                            showAlert("Backup failed!!!", "Please connect to Internet for Back-up", "OK");
                            //updateProgress("No internet! Backup failed!!!", 100, Color.Red);
                            poorInternet = true;
                            break;
                        }
                        var client = new RestClient("https://myconsoleerp.herokuapp.com/tqm/datasync");
                        var request = new RestRequest();
                        request.Method = Method.Post;
                        request.Timeout = Timeout.Infinite;
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
                        //updateProgress("Uploading.....", 100, Color.BlueViolet);
                    }

                    if (syncDataCount == userList.Count)
                    {
                        //success
                        //updateProgress("User: Backup completed!!!", 100, Color.Green);
                        status = true;
                    }
                    else if (syncDataCount != userList.Count)
                    {
                        if (syncDataCount > 0)
                        {
                            if (poorInternet)
                            {
                                //Poor internet but few record(s) pushed to server
                                //updateProgress("User: Backup partially completed but failed due to poor internet connection!!!", 100, Color.Yellow);
                                showAlert("Attention", "User: Backup partially completed but failed due to poor internet connection!!!", "OK");
                            }
                            else
                            {
                                //Internet is good but few records not pushed to server
                                if (!checkConnection())
                                {
                                    //updateProgress("User: Backup partially completed but failed due to poor internet connection!!!", 100, Color.Yellow);
                                    showAlert("Attention", "User: Backup partially completed but failed due to poor internet connection!!!", "OK");
                                }
                                else
                                {
                                    //updateProgress("User: Backup completed partially but failed", 100, Color.Red);
                                    showAlert("Attention", "User: Backup completed partially but failed", "OK");
                                }

                            }
                        }
                        else
                        {
                            if (poorInternet)
                            {
                                //Poor internet, so none of the record(s) pushed to server
                                //updateProgress("User: Backup failed due to poor internet connection!!!", 100, Color.Yellow);
                                showAlert("Attention", "User: Backup failed due to poor internet connection!!!", "OK");
                            }
                            else
                            {
                                //Internet is good but none of the record(s) pushed to server
                                if (!checkConnection())
                                {
                                    //updateProgress("User: Backup failed due to poor internet connection!!!", 100, Color.Yellow);
                                    showAlert("Attention", "User: Backup failed due to poor internet connection!!!", "OK");
                                }
                                else
                                {
                                    //updateProgress("User: Backup failed. Please contact manufacturer!!!", 100, Color.Red);
                                    showAlert("Attention", "User: Backup failed. Please contact manufacturer!!!", "OK");
                                }
                            }

                        }
                    }
                }
                if (status)
                {
                    CancellationTokenSource src = new CancellationTokenSource();
                    CancellationToken ct = src.Token;
                    ct.Register(() => Debug.WriteLine("ConnectBluetoothToken"));
                    await Task.Run(async () => await syncDataMachineModel(), ct);
                    src.Cancel();
                }
                else
                {
                    backupStatus = false;
                    ////toggleLoading(false);
                }
            }
            catch (Exception ex)
            {
                backupStatus = false;
                showAlert("Backup Failed!!!", "Error : " + ex.Message.ToString(), "OK");
                //updateProgress("Backup failed - " + ex.Message.ToString(), 100, Color.Red);
                ////toggleLoading(false);

            }
        }

        private async Task syncDataMachineModel()
        {
            try
            {
                bool status = false;
                bool poorInternet = false;
                //updateProgress("", 900, Color.White);
                if (!checkConnection())
                {
                    backupStatus = false;
                    showAlert("Backup failed!!!", "Please connect to Internet for Back-up", "OK");
                    //updateProgress("No internet! Backup failed!!!", 100, Color.Red);
                    //toggleLoading(false);
                    return;
                }
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    int syncDataCount = 0;
                    conn.CreateTable<MachineModel>();
                    List<MachineModel> machineList = conn.Table<MachineModel>()
                        .Where(MachineModel => MachineModel.dataSyncStatus == false).ToList();
                    foreach (MachineModel machine in machineList)
                    {
                        //updateProgress("", 900, Color.BlueViolet);
                        if (!checkConnection())
                        {
                            backupStatus = false;
                            showAlert("Backup failed!!!", "Please connect to Internet for Back-up", "OK");
                            //updateProgress("No internet! Backup failed!!!", 100, Color.Red);
                            poorInternet = true;
                            break;
                        }
                        var client = new RestClient("https://myconsoleerp.herokuapp.com/tqm/datasync");
                        var request = new RestRequest();
                        request.Method = Method.Post;
                        request.Timeout = Timeout.Infinite;
                        request.AddParameter("modelName", "TQMMachine");
                        request.AddParameter("userName", runConfiguration.getTQMAppUserID());
                        request.AddParameter("GUID", machine.ID);
                        request.AddParameter("machineCategory", machine.machineCategory);
                        request.AddParameter("machineName", machine.machineName);
                        request.AddParameter("createdate", getDateString(machine.createdate));
                        RestResponse response = client.Execute(request);
                        if (response.IsSuccessful)
                        {
                            if (response.StatusCode == System.Net.HttpStatusCode.Created ||
                                response.StatusCode == System.Net.HttpStatusCode.Accepted)
                            {
                                machine.dataSyncStatus = true;
                                int row = 0;
                                row = conn.Update(machine);
                                if (row > 0)
                                {
                                    syncDataCount = syncDataCount + 1;
                                }
                            }
                        }
                        //updateProgress("Uploading.....", 100, Color.BlueViolet);
                    }

                    if (syncDataCount == machineList.Count)
                    {
                        //success
                        //updateProgress("Machine: Backup completed!!!", 100, Color.Green);
                        status = true;
                    }
                    else if (syncDataCount != machineList.Count)
                    {
                        if (syncDataCount > 0)
                        {
                            if (poorInternet)
                            {
                                //Poor internet but few record(s) pushed to server
                                //updateProgress("Machine: Backup partially completed but failed due to poor internet connection!!!", 100, Color.Yellow);
                                showAlert("Attention", "Machine: Backup partially completed but failed due to poor internet connection!!!", "OK");
                            }
                            else
                            {
                                //Internet is good but few records not pushed to server
                                if (!checkConnection())
                                {
                                    //updateProgress("Machine: Backup partially completed but failed due to poor internet connection!!!", 100, Color.Yellow);
                                    showAlert("Attention", "Machine: Backup partially completed but failed due to poor internet connection!!!", "OK");
                                }
                                else
                                {
                                    //updateProgress("Machine: Backup completed partially but failed", 100, Color.Red);
                                    showAlert("Attention", "Machine: Backup completed partially but failed", "OK");
                                }

                            }
                        }
                        else
                        {
                            if (poorInternet)
                            {
                                //Poor internet, so none of the record(s) pushed to server
                                //updateProgress("Machine: Backup failed due to poor internet connection!!!", 100, Color.Yellow);
                                showAlert("Attention", "Machine: Backup failed due to poor internet connection!!!", "OK");
                            }
                            else
                            {
                                //Internet is good but none of the record(s) pushed to server
                                if (!checkConnection())
                                {
                                    //updateProgress("Machine: Backup failed due to poor internet connection!!!", 100, Color.Yellow);
                                    showAlert("Attention", "Machine: Backup failed due to poor internet connection!!!", "OK");
                                }
                                else
                                {
                                    //updateProgress("Machine: Backup failed. Please contact manufacturer!!!", 100, Color.Red);
                                    showAlert("Attention", "Machine: Backup failed. Please contact manufacturer!!!", "OK");
                                }
                            }
                        }
                    }
                }
                if (status)
                {
                    CancellationTokenSource src = new CancellationTokenSource();
                    CancellationToken ct = src.Token;
                    ct.Register(() => Debug.WriteLine("ConnectBluetoothToken"));
                    await Task.Run(async () => await syncDataSettingsModel(), ct);
                    src.Cancel();
                }
                else
                {
                    backupStatus = false;
                    //toggleLoading(false);
                }
            }
            catch (Exception ex)
            {
                backupStatus = false;
                showAlert("Backup Failed!!!", "Error : " + ex.Message.ToString(), "OK");
                //updateProgress("Backup failed - " + ex.Message.ToString(), 100, Color.Red);
                //toggleLoading(false);

            }
        }

        private async Task syncDataSettingsModel()
        {
            try
            {
                bool status = false;
                bool poorInternet = false;
                //updateProgress("", 900, Color.White);
                if (!checkConnection())
                {
                    backupStatus = false;
                    showAlert("Backup failed!!!", "Please connect to Internet for Back-up", "OK");
                    //updateProgress("No internet! Backup failed!!!", 100, Color.Red);
                    //toggleLoading(false);
                    return;
                }
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    int syncDataCount = 0;
                    conn.CreateTable<YarnCountConfigModel>();
                    List<YarnCountConfigModel> yarnCountConfigs = conn.Table<YarnCountConfigModel>()
                        .Where(YarnCountConfigModel => YarnCountConfigModel.dataSyncStatus == false).ToList();
                    foreach (YarnCountConfigModel yarnCountConfig in yarnCountConfigs)
                    {
                        //updateProgress("", 900, Color.BlueViolet);
                        if (!checkConnection())
                        {
                            backupStatus = false;
                            showAlert("Backup failed!!!", "Please connect to Internet for Back-up", "OK");
                            //updateProgress("No internet! Backup failed!!!", 100, Color.Red);
                            poorInternet = true;
                            break;
                        }
                        var client = new RestClient("https://myconsoleerp.herokuapp.com/tqm/datasync");
                        var request = new RestRequest();
                        request.Method = Method.Post;
                        request.Timeout = Timeout.Infinite;
                        request.AddParameter("modelName", "TQMSettings");
                        request.AddParameter("userName", runConfiguration.getTQMAppUserID());
                        request.AddParameter("GUID", yarnCountConfig.ID);
                        request.AddParameter("countsysname", yarnCountConfig.countsysname);
                        request.AddParameter("yarnlenunit", yarnCountConfig.yarnlenunit);
                        request.AddParameter("sliverlength", yarnCountConfig.sliverlength);
                        request.AddParameter("rovinglength", yarnCountConfig.rovinglength);
                        request.AddParameter("testcount", yarnCountConfig.testcount);
                        request.AddParameter("standardHank", yarnCountConfig.standardHank);
                        request.AddParameter("testcountApercent", yarnCountConfig.testcountApercent);
                        request.AddParameter("testcountStretch", yarnCountConfig.testcountStretch);
                        request.AddParameter("testcountNoils", yarnCountConfig.testcountNoils);

                        RestResponse response = client.Execute(request);
                        if (response.IsSuccessful)
                        {
                            if (response.StatusCode == System.Net.HttpStatusCode.Created ||
                                response.StatusCode == System.Net.HttpStatusCode.Accepted)
                            {
                                yarnCountConfig.dataSyncStatus = true;
                                int row = 0;
                                row = conn.Update(yarnCountConfig);
                                if (row > 0)
                                {
                                    syncDataCount = syncDataCount + 1;
                                }
                            }
                        }
                        //updateProgress("Uploading.....", 100, Color.BlueViolet);
                    }

                    if (syncDataCount == yarnCountConfigs.Count)
                    {
                        //success
                        //updateProgress("Settings: Backup completed!!!", 100, Color.Green);
                        status = true;
                    }
                    else if (syncDataCount != yarnCountConfigs.Count)
                    {
                        if (syncDataCount > 0)
                        {
                            if (poorInternet)
                            {
                                //Poor internet but few record(s) pushed to server
                                //updateProgress("Settings: Backup partially completed but failed due to poor internet connection!!!", 100, Color.Yellow);
                                showAlert("Attention", "Settings: Backup partially completed but failed due to poor internet connection!!!", "OK");
                            }
                            else
                            {
                                //Internet is good but few records not pushed to server
                                if (!checkConnection())
                                {
                                    //updateProgress("Settings: Backup partially completed but failed due to poor internet connection!!!", 100, Color.Yellow);
                                    showAlert("Attention", "Settings: Backup partially completed but failed due to poor internet connection!!!", "OK");
                                }
                                else
                                {
                                    //updateProgress("Settings: Backup completed partially but failed", 100, Color.Red);
                                    showAlert("Attention", "Settings: Backup completed partially but failed", "OK");
                                }

                            }
                        }
                        else
                        {
                            if (poorInternet)
                            {
                                //Poor internet, so none of the record(s) pushed to server
                                //updateProgress("Settings: Backup failed due to poor internet connection!!!", 100, Color.Yellow);
                                showAlert("Attention", "Settings: Backup failed due to poor internet connection!!!", "OK");
                            }
                            else
                            {
                                //Internet is good but none of the record(s) pushed to server
                                if (!checkConnection())
                                {
                                    //updateProgress("Settings: Backup failed due to poor internet connection!!!", 100, Color.Yellow);
                                    showAlert("Attention", "Settings: Backup failed due to poor internet connection!!!", "OK");
                                }
                                else
                                {
                                    //updateProgress("Settings: Backup failed. Please contact manufacturer!!!", 100, Color.Red);
                                    showAlert("Attention", "Settings: Backup failed. Please contact manufacturer!!!", "OK");
                                }
                            }
                        }
                    }
                }
                if (status)
                {
                    CancellationTokenSource src = new CancellationTokenSource();
                    CancellationToken ct = src.Token;
                    ct.Register(() => Debug.WriteLine("ConnectBluetoothToken"));
                    await Task.Run(async () => await syncDataTQMYCTestModel(), ct);
                    src.Cancel();
                }
                else
                {
                    backupStatus = false;
                    //toggleLoading(false);
                }
            }
            catch (Exception ex)
            {
                showAlert("Backup Failed!!!", "Error : " + ex.Message.ToString(), "OK");
                //updateProgress("Backup failed - " + ex.Message.ToString(), 100, Color.Red);
                //toggleLoading(false);

            }
        }

        private async Task syncDataTQMYCTestModel()
        {
            try
            {
                bool status = false;
                bool poorInternet = false;
                //updateProgress("", 900, Color.White);
                if (!checkConnection())
                {
                    backupStatus = false;
                    showAlert("Backup failed!!!", "Please connect to Internet for Back-up", "OK");
                    //updateProgress("No internet! Backup failed!!!", 100, Color.Red);
                    //toggleLoading(false);
                    return;
                }
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    int syncDataCount = 0;
                    conn.CreateTable<YCTestModel>();
                    List<YCTestModel> ycTestModels = conn.Table<YCTestModel>()
                        .Where(YCTestModel => YCTestModel.dataSyncStatus == false).ToList();
                    foreach (YCTestModel ycTest in ycTestModels)
                    {
                        //updateProgress("", 900, Color.BlueViolet);
                        if (!checkConnection())
                        {
                            backupStatus = false;
                            showAlert("Backup failed!!!", "Please connect to Internet for Back-up", "OK");
                            //updateProgress("No internet! Backup failed!!!", 100, Color.Red);
                            poorInternet = true;
                            break;
                        }
                        var client = new RestClient("https://myconsoleerp.herokuapp.com/tqm/datasync");
                        var request = new RestRequest();
                        request.Method = Method.Post;
                        request.Timeout = Timeout.Infinite;
                        request.AddParameter("modelName", "TQMYCTestModel");
                        request.AddParameter("userName", runConfiguration.getTQMAppUserID());
                        request.AddParameter("GUID", ycTest.ID);
                        request.AddParameter("testID", ycTest.testID);
                        request.AddParameter("userID", ycTest.userID);
                        request.AddParameter("TQMuserName", ycTest.userName);
                        request.AddParameter("machineID", ycTest.machineID);
                        request.AddParameter("machineCategory", ycTest.machineCategory);
                        request.AddParameter("machineName", ycTest.machineName);
                        request.AddParameter("shift", ycTest.shift);
                        request.AddParameter("process", ycTest.process);
                        request.AddParameter("countsysname", ycTest.countsysname);
                        request.AddParameter("yarnlenunit", ycTest.yarnlenunit);
                        request.AddParameter("yarnlength", ycTest.yarnlength);
                        request.AddParameter("totaltestcount", ycTest.totaltestcount);
                        request.AddParameter("testcount", ycTest.testcount);
                        request.AddParameter("yarnweight", ycTest.yarnweight);
                        request.AddParameter("yccalcval", ycTest.yccalcval);
                        request.AddParameter("createdate", getDateString(ycTest.createdate));


                        RestResponse response = client.Execute(request);
                        if (response.IsSuccessful)
                        {
                            if (response.StatusCode == System.Net.HttpStatusCode.Created ||
                                response.StatusCode == System.Net.HttpStatusCode.Accepted)
                            {
                                ycTest.dataSyncStatus = true;
                                int row = 0;
                                row = conn.Update(ycTest);
                                if (row > 0)
                                {
                                    syncDataCount = syncDataCount + 1;
                                }
                            }
                        }
                        //updateProgress("Uploading.....", 100, Color.BlueViolet);
                    }

                    if (syncDataCount == ycTestModels.Count)
                    {
                        //success
                        //updateProgress("Wrapping: Backup completed!!!", 100, Color.Green);
                        status = true;
                    }
                    else if (syncDataCount != ycTestModels.Count)
                    {
                        if (syncDataCount > 0)
                        {
                            if (poorInternet)
                            {
                                //Poor internet but few record(s) pushed to server
                                //updateProgress("Wrapping: Backup partially completed but failed due to poor internet connection!!!", 100, Color.Yellow);
                                showAlert("Attention", "Wrapping: Backup partially completed but failed due to poor internet connection!!!", "OK");
                            }
                            else
                            {
                                //Internet is good but few records not pushed to server
                                if (!checkConnection())
                                {
                                    //updateProgress("Wrapping: Backup partially completed but failed due to poor internet connection!!!", 100, Color.Yellow);
                                    showAlert("Attention", "Wrapping: Backup partially completed but failed due to poor internet connection!!!", "OK");
                                }
                                else
                                {
                                    //updateProgress("Wrapping: Backup completed partially but failed", 100, Color.Red);
                                    showAlert("Attention", "Wrapping: Backup completed partially but failed", "OK");
                                }

                            }
                        }
                        else
                        {
                            if (poorInternet)
                            {
                                //Poor internet, so none of the record(s) pushed to server
                                //updateProgress("Wrapping: Backup failed due to poor internet connection!!!", 100, Color.Yellow);
                                showAlert("Attention", "Wrapping: Backup failed due to poor internet connection!!!", "OK");
                            }
                            else
                            {
                                //Internet is good but none of the record(s) pushed to server
                                if (!checkConnection())
                                {
                                    //updateProgress("Wrapping: Backup failed due to poor internet connection!!!", 100, Color.Yellow);
                                    showAlert("Attention", "Wrapping: Backup failed due to poor internet connection!!!", "OK");
                                }
                                else
                                {
                                    //updateProgress("Wrapping: Backup failed. Please contact manufacturer!!!", 100, Color.Red);
                                    showAlert("Attention", "Wrapping: Backup failed. Please contact manufacturer!!!", "OK");
                                }
                            }
                        }
                    }
                }
                if (status)
                {
                    CancellationTokenSource src = new CancellationTokenSource();
                    CancellationToken ct = src.Token;
                    ct.Register(() => Debug.WriteLine("ConnectBluetoothToken"));
                    await Task.Run(async () => await syncDataTQMYCTestSummaryModel(), ct);
                    src.Cancel();
                }
                else
                {
                    backupStatus = false;
                    //toggleLoading(false);
                }
            }
            catch (Exception ex)
            {
                backupStatus = false;
                showAlert("Backup Failed!!!", "Error : " + ex.Message.ToString(), "OK");
                //updateProgress("Backup failed - " + ex.Message.ToString(), 100, Color.Red);
                //toggleLoading(false);

            }
        }

        private async Task syncDataTQMYCTestSummaryModel()
        {
            try
            {
                bool status = false;
                bool poorInternet = false;
                //updateProgress("", 900, Color.White);
                if (!checkConnection())
                {
                    backupStatus = false;
                    showAlert("Backup failed!!!", "Please connect to Internet for Back-up", "OK");
                    //updateProgress("No internet! Backup failed!!!", 100, Color.Red);
                    //toggleLoading(false);
                    return;
                }
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    int syncDataCount = 0;
                    conn.CreateTable<YCTestSummaryModel>();
                    List<YCTestSummaryModel> ycTestSummaryModels = conn.Table<YCTestSummaryModel>()
                        .Where(YCTestSummaryModel => YCTestSummaryModel.dataSyncStatus == false).ToList();
                    foreach (YCTestSummaryModel ycTestSummary in ycTestSummaryModels)
                    {
                        //updateProgress("", 900, Color.BlueViolet);
                        if (!checkConnection())
                        {
                            backupStatus = false;
                            showAlert("Backup failed!!!", "Please connect to Internet for Back-up", "OK");
                            //updateProgress("No internet! Backup failed!!!", 100, Color.Red);
                            poorInternet = true;
                            break;
                        }
                        var client = new RestClient("https://myconsoleerp.herokuapp.com/tqm/datasync");
                        var request = new RestRequest();
                        request.Method = Method.Post;
                        request.Timeout = Timeout.Infinite;
                        request.AddParameter("modelName", "TQMYCTestSummaryModel");
                        request.AddParameter("userName", runConfiguration.getTQMAppUserID());
                        request.AddParameter("GUID", ycTestSummary.ID);
                        request.AddParameter("testID", ycTestSummary.testID);
                        request.AddParameter("userID", ycTestSummary.userID);
                        request.AddParameter("TQMuserName", ycTestSummary.userName);
                        request.AddParameter("machineID", ycTestSummary.machineID);
                        request.AddParameter("machineCategory", ycTestSummary.machineCategory);
                        request.AddParameter("machineName", ycTestSummary.machineName);
                        request.AddParameter("shift", ycTestSummary.shift);
                        request.AddParameter("process", ycTestSummary.process);
                        request.AddParameter("countsysname", ycTestSummary.countsysname);
                        request.AddParameter("yarnlenunit", ycTestSummary.yarnlenunit);
                        request.AddParameter("yarnlength", ycTestSummary.yarnlength);
                        request.AddParameter("totaltestcount", ycTestSummary.totaltestcount);
                        request.AddParameter("testaverage", ycTestSummary.testaverage);
                        request.AddParameter("testsd", ycTestSummary.testsd);
                        request.AddParameter("testcv", ycTestSummary.testcv);
                        request.AddParameter("createdate", getDateString(ycTestSummary.createdate));



                        RestResponse response = client.Execute(request);
                        if (response.IsSuccessful)
                        {
                            if (response.StatusCode == System.Net.HttpStatusCode.Created ||
                                response.StatusCode == System.Net.HttpStatusCode.Accepted)
                            {
                                ycTestSummary.dataSyncStatus = true;
                                int row = 0;
                                row = conn.Update(ycTestSummary);
                                if (row > 0)
                                {
                                    syncDataCount = syncDataCount + 1;
                                }
                            }
                        }
                        //updateProgress("Uploading.....", 100, Color.BlueViolet);
                    }

                    if (syncDataCount == ycTestSummaryModels.Count)
                    {
                        //success
                        //updateProgress("Wrapping Summary: Backup completed!!!", 100, Color.Green);
                        status = true;
                    }
                    else if (syncDataCount != ycTestSummaryModels.Count)
                    {
                        if (syncDataCount > 0)
                        {
                            if (poorInternet)
                            {
                                //Poor internet but few record(s) pushed to server
                                //updateProgress("Wrapping Summary: Backup partially completed but failed due to poor internet connection!!!", 100, Color.Yellow);
                                showAlert("Attention", "Wrapping Summary: Backup partially completed but failed due to poor internet connection!!!", "OK");
                            }
                            else
                            {
                                //Internet is good but few records not pushed to server
                                if (!checkConnection())
                                {
                                    //updateProgress("Wrapping Summary: Backup partially completed but failed due to poor internet connection!!!", 100, Color.Yellow);
                                    showAlert("Attention", "Wrapping Summary: Backup partially completed but failed due to poor internet connection!!!", "OK");
                                }
                                else
                                {
                                    //updateProgress("Wrapping Summary: Backup completed partially but failed", 100, Color.Red);
                                    showAlert("Attention", "Wrapping Summary: Backup completed partially but failed", "OK");
                                }

                            }
                        }
                        else
                        {
                            if (poorInternet)
                            {
                                //Poor internet, so none of the record(s) pushed to server
                                //updateProgress("Wrapping Summary: Backup failed due to poor internet connection!!!", 100, Color.Yellow);
                                showAlert("Attention", "Wrapping Summary: Backup failed due to poor internet connection!!!", "OK");
                            }
                            else
                            {
                                //Internet is good but none of the record(s) pushed to server
                                if (!checkConnection())
                                {
                                    //updateProgress("Wrapping Summary: Backup failed due to poor internet connection!!!", 100, Color.Yellow);
                                    showAlert("Attention", "Wrapping Summary: Backup failed due to poor internet connection!!!", "OK");
                                }
                                else
                                {
                                    //updateProgress("Wrapping Summary: Backup failed. Please contact manufacturer!!!", 100, Color.Red);
                                    showAlert("Attention", "Wrapping Summary: Backup failed. Please contact manufacturer!!!", "OK");
                                }
                            }
                        }
                    }
                }
                if (status)
                {
                    CancellationTokenSource src = new CancellationTokenSource();
                    CancellationToken ct = src.Token;
                    ct.Register(() => Debug.WriteLine("ConnectBluetoothToken"));
                    await Task.Run(async () => await syncDataTQMYCTestApercentModel(), ct);
                    src.Cancel();
                }
                else
                {
                    backupStatus = false;
                    //toggleLoading(false);
                }
            }
            catch (Exception ex)
            {
                backupStatus = false;
                showAlert("Backup Failed!!!", "Error : " + ex.Message.ToString(), "OK");
                //updateProgress("Backup failed - " + ex.Message.ToString(), 100, Color.Red);
                //toggleLoading(false);

            }
        }

        private async Task syncDataTQMYCTestApercentModel()
        {
            try
            {
                bool status = false;
                bool poorInternet = false;
                //updateProgress("", 900, Color.White);
                if (!checkConnection())
                {
                    backupStatus = false;
                    showAlert("Backup failed!!!", "Please connect to Internet for Back-up", "OK");
                    //updateProgress("No internet! Backup failed!!!", 100, Color.Red);
                    //toggleLoading(false);
                    return;
                }
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    int syncDataCount = 0;
                    conn.CreateTable<YCTestApercentModel>();
                    List<YCTestApercentModel> ycTestApercentModels = conn.Table<YCTestApercentModel>()
                        .Where(YCTestApercentModel => YCTestApercentModel.dataSyncStatus == false).ToList();
                    foreach (YCTestApercentModel ycTestApercent in ycTestApercentModels)
                    {
                        //updateProgress("", 900, Color.BlueViolet);
                        if (!checkConnection())
                        {
                            backupStatus = false;
                            showAlert("Backup failed!!!", "Please connect to Internet for Back-up", "OK");
                            //updateProgress("No internet! Backup failed!!!", 100, Color.Red);
                            poorInternet = true;
                            break;
                        }
                        var client = new RestClient("https://myconsoleerp.herokuapp.com/tqm/datasync");
                        var request = new RestRequest();
                        request.Method = Method.Post;
                        request.Timeout = Timeout.Infinite;
                        request.AddParameter("modelName", "TQMYCTestApercentModel");
                        request.AddParameter("userName", runConfiguration.getTQMAppUserID());
                        request.AddParameter("GUID", ycTestApercent.ID);
                        request.AddParameter("testID", ycTestApercent.testID);
                        request.AddParameter("userID", ycTestApercent.userID);
                        request.AddParameter("TQMuserName", ycTestApercent.userName);
                        request.AddParameter("machineID", ycTestApercent.machineID);
                        request.AddParameter("machineCategory", ycTestApercent.machineCategory);
                        request.AddParameter("machineName", ycTestApercent.machineName);
                        request.AddParameter("shift", ycTestApercent.shift);
                        request.AddParameter("process", ycTestApercent.process);
                        request.AddParameter("countsysname", ycTestApercent.countsysname);
                        request.AddParameter("yarnlenunit", ycTestApercent.yarnlenunit);
                        request.AddParameter("yarnlength", ycTestApercent.yarnlength);
                        request.AddParameter("testType", ycTestApercent.testType);
                        request.AddParameter("totaltestcount", ycTestApercent.totaltestcount);
                        request.AddParameter("testcount", ycTestApercent.testcount);
                        request.AddParameter("yarnweight", ycTestApercent.yarnweight);
                        request.AddParameter("yccalcval", ycTestApercent.yccalcval);
                        request.AddParameter("status", ycTestApercent.status);
                        request.AddParameter("createdate", getDateString(ycTestApercent.createdate));

                        RestResponse response = client.Execute(request);
                        if (response.IsSuccessful)
                        {
                            if (response.StatusCode == System.Net.HttpStatusCode.Created ||
                                response.StatusCode == System.Net.HttpStatusCode.Accepted)
                            {
                                ycTestApercent.dataSyncStatus = true;
                                int row = 0;
                                row = conn.Update(ycTestApercent);
                                if (row > 0)
                                {
                                    syncDataCount = syncDataCount + 1;
                                }
                            }
                        }
                        //updateProgress("Uploading.....", 100, Color.BlueViolet);
                    }

                    if (syncDataCount == ycTestApercentModels.Count)
                    {
                        //success
                        //updateProgress("A%: Backup completed!!!", 100, Color.Green);
                        status = true;
                    }
                    else if (syncDataCount != ycTestApercentModels.Count)
                    {
                        if (syncDataCount > 0)
                        {
                            if (poorInternet)
                            {
                                //Poor internet but few record(s) pushed to server
                                //updateProgress("A%: Backup partially completed but failed due to poor internet connection!!!", 100, Color.Yellow);
                                showAlert("Attention", "A%: Backup partially completed but failed due to poor internet connection!!!", "OK");
                            }
                            else
                            {
                                //Internet is good but few records not pushed to server
                                if (!checkConnection())
                                {
                                    //updateProgress("A%: Backup partially completed but failed due to poor internet connection!!!", 100, Color.Yellow);
                                    showAlert("Attention", "A%: Backup partially completed but failed due to poor internet connection!!!", "OK");
                                }
                                else
                                {
                                    //updateProgress("A%: Backup completed partially but failed", 100, Color.Red);
                                    showAlert("Attention", "A%: Backup completed partially but failed", "OK");
                                }

                            }
                        }
                        else
                        {
                            if (poorInternet)
                            {
                                //Poor internet, so none of the record(s) pushed to server
                                //updateProgress("A%: Backup failed due to poor internet connection!!!", 100, Color.Yellow);
                                showAlert("Attention", "A%: Backup failed due to poor internet connection!!!", "OK");
                            }
                            else
                            {
                                //Internet is good but none of the record(s) pushed to server
                                if (!checkConnection())
                                {
                                    //updateProgress("A%: Backup failed due to poor internet connection!!!", 100, Color.Yellow);
                                    showAlert("Attention", "A%: Backup failed due to poor internet connection!!!", "OK");
                                }
                                else
                                {
                                    //updateProgress("A%: Backup failed. Please contact manufacturer!!!", 100, Color.Red);
                                    showAlert("Attention", "A%: Backup failed. Please contact manufacturer!!!", "OK");
                                }
                            }
                        }
                    }
                }
                if (status)
                {
                    CancellationTokenSource src = new CancellationTokenSource();
                    CancellationToken ct = src.Token;
                    ct.Register(() => Debug.WriteLine("ConnectBluetoothToken"));
                    await Task.Run(async () => await syncDataTQMYCTestApercentSummaryModel(), ct);
                    src.Cancel();
                }
                else
                {
                    backupStatus = false;
                    //toggleLoading(false);
                }
            }
            catch (Exception ex)
            {
                backupStatus = false;
                showAlert("Backup Failed!!!", "Error : " + ex.Message.ToString(), "OK");
                //updateProgress("Backup failed - " + ex.Message.ToString(), 100, Color.Red);
                //toggleLoading(false);

            }
        }

        private async Task syncDataTQMYCTestApercentSummaryModel()
        {
            try
            {
                bool status = false;
                bool poorInternet = false;
                //updateProgress("", 900, Color.White);
                if (!checkConnection())
                {
                    backupStatus = false;
                    showAlert("Backup failed!!!", "Please connect to Internet for Back-up", "OK");
                    //updateProgress("No internet! Backup failed!!!", 100, Color.Red);
                    //toggleLoading(false);
                    return;
                }
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    int syncDataCount = 0;
                    conn.CreateTable<YCTestApercentSummaryModel>();
                    List<YCTestApercentSummaryModel> ycTestApercentSummaryList = conn.Table<YCTestApercentSummaryModel>()
                        .Where(YCTestApercentSummaryModel => YCTestApercentSummaryModel.dataSyncStatus == false).ToList();
                    foreach (YCTestApercentSummaryModel ycTestApercentSummary in ycTestApercentSummaryList)
                    {
                        //updateProgress("", 900, Color.BlueViolet);
                        if (!checkConnection())
                        {
                            backupStatus = false;
                            showAlert("Backup failed!!!", "Please connect to Internet for Back-up", "OK");
                            //updateProgress("No internet! Backup failed!!!", 100, Color.Red);
                            poorInternet = true;
                            break;
                        }
                        var client = new RestClient("https://myconsoleerp.herokuapp.com/tqm/datasync");
                        var request = new RestRequest();
                        request.Method = Method.Post;
                        request.Timeout = Timeout.Infinite;
                        request.AddParameter("modelName", "TQMYCTestApercentSummaryModel");
                        request.AddParameter("userName", runConfiguration.getTQMAppUserID());
                        request.AddParameter("GUID", ycTestApercentSummary.ID);
                        request.AddParameter("testID", ycTestApercentSummary.testID);
                        request.AddParameter("userID", ycTestApercentSummary.userID);
                        request.AddParameter("TQMuserName", ycTestApercentSummary.userName);
                        request.AddParameter("machineID", ycTestApercentSummary.machineID);
                        request.AddParameter("machineCategory", ycTestApercentSummary.machineCategory);
                        request.AddParameter("machineName", ycTestApercentSummary.machineName);
                        request.AddParameter("shift", ycTestApercentSummary.shift);
                        request.AddParameter("process", ycTestApercentSummary.process);
                        request.AddParameter("countsysname", ycTestApercentSummary.countsysname);
                        request.AddParameter("yarnlenunit", ycTestApercentSummary.yarnlenunit);
                        request.AddParameter("yarnlength", ycTestApercentSummary.yarnlength);
                        request.AddParameter("testType", ycTestApercentSummary.testType);
                        request.AddParameter("totaltestcount", ycTestApercentSummary.totaltestcount);
                        request.AddParameter("avg_weight", ycTestApercentSummary.avg_weight);
                        request.AddParameter("testaverage", ycTestApercentSummary.testaverage);
                        request.AddParameter("testsd", ycTestApercentSummary.testsd);
                        request.AddParameter("testcv", ycTestApercentSummary.testcv);
                        request.AddParameter("status", ycTestApercentSummary.status);
                        request.AddParameter("createdate", getDateString(ycTestApercentSummary.createdate));

                        RestResponse response = client.Execute(request);
                        if (response.IsSuccessful)
                        {
                            if (response.StatusCode == System.Net.HttpStatusCode.Created ||
                                response.StatusCode == System.Net.HttpStatusCode.Accepted)
                            {
                                ycTestApercentSummary.dataSyncStatus = true;
                                int row = 0;
                                row = conn.Update(ycTestApercentSummary);
                                if (row > 0)
                                {
                                    syncDataCount = syncDataCount + 1;
                                }
                            }
                        }
                        //updateProgress("Uploading.....", 100, Color.BlueViolet);
                    }

                    if (syncDataCount == ycTestApercentSummaryList.Count)
                    {
                        //success
                        //updateProgress("A% Summary: Backup completed!!!", 100, Color.Green);
                        status = true;
                    }
                    else if (syncDataCount != ycTestApercentSummaryList.Count)
                    {
                        if (syncDataCount > 0)
                        {
                            if (poorInternet)
                            {
                                //Poor internet but few record(s) pushed to server
                                //updateProgress("A% Summary: Backup partially completed but failed due to poor internet connection!!!", 100, Color.Yellow);
                                showAlert("Attention", "A% Summary: Backup partially completed but failed due to poor internet connection!!!", "OK");
                            }
                            else
                            {
                                //Internet is good but few records not pushed to server
                                if (!checkConnection())
                                {
                                    //updateProgress("A% Summary: Backup partially completed but failed due to poor internet connection!!!", 100, Color.Yellow);
                                    showAlert("Attention", "A% Summary: Backup partially completed but failed due to poor internet connection!!!", "OK");
                                }
                                else
                                {
                                    //updateProgress("A% Summary: Backup completed partially but failed", 100, Color.Red);
                                    showAlert("Attention", "A% Summary: Backup completed partially but failed", "OK");
                                }

                            }
                        }
                        else
                        {
                            if (poorInternet)
                            {
                                //Poor internet, so none of the record(s) pushed to server
                                //updateProgress("A% Summary: Backup failed due to poor internet connection!!!", 100, Color.Yellow);
                                showAlert("Attention", "A% Summary: Backup failed due to poor internet connection!!!", "OK");
                            }
                            else
                            {
                                //Internet is good but none of the record(s) pushed to server
                                if (!checkConnection())
                                {
                                    //updateProgress("A% Summary: Backup failed due to poor internet connection!!!", 100, Color.Yellow);
                                    showAlert("Attention", "A% Summary: Backup failed due to poor internet connection!!!", "OK");
                                }
                                else
                                {
                                    //updateProgress("A% Summary: Backup failed. Please contact manufacturer!!!", 100, Color.Red);
                                    showAlert("Attention", "A% Summary: Backup failed. Please contact manufacturer!!!", "OK");
                                }
                            }
                        }
                    }
                }
                if (status)
                {
                    CancellationTokenSource src = new CancellationTokenSource();
                    CancellationToken ct = src.Token;
                    ct.Register(() => Debug.WriteLine("ConnectBluetoothToken"));
                    await Task.Run(async () => await syncDataTQMYCTestApercentCalculatedModel(), ct);
                    src.Cancel();
                }
                else
                {
                    backupStatus = false;
                    //toggleLoading(false);
                }
            }
            catch (Exception ex)
            {
                backupStatus = false;
                showAlert("Backup Failed!!!", "Error : " + ex.Message.ToString(), "OK");
                //updateProgress("Backup failed - " + ex.Message.ToString(), 100, Color.Red);
                //toggleLoading(false);

            }
        }

        private async Task syncDataTQMYCTestApercentCalculatedModel()
        {
            try
            {
                bool status = false;
                bool poorInternet = false;
                //updateProgress("", 900, Color.White);
                if (!checkConnection())
                {
                    backupStatus = false;
                    showAlert("Backup failed!!!", "Please connect to Internet for Back-up", "OK");
                    //updateProgress("No internet! Backup failed!!!", 100, Color.Red);
                    //toggleLoading(false);
                    return;
                }
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    int syncDataCount = 0;
                    conn.CreateTable<YCTestApercentCalculatedModel>();
                    List<YCTestApercentCalculatedModel> ycTestApercentCalculatedList = conn.Table<YCTestApercentCalculatedModel>()
                        .Where(YCTestApercentCalculatedModel => YCTestApercentCalculatedModel.dataSyncStatus == false).ToList();
                    foreach (YCTestApercentCalculatedModel ycTestApercentCalc in ycTestApercentCalculatedList)
                    {
                        //updateProgress("", 900, Color.BlueViolet);
                        if (!checkConnection())
                        {
                            backupStatus = false;
                            showAlert("Backup failed!!!", "Please connect to Internet for Back-up", "OK");
                            //updateProgress("No internet! Backup failed!!!", 100, Color.Red);
                            poorInternet = true;
                            break;
                        }
                        var client = new RestClient("https://myconsoleerp.herokuapp.com/tqm/datasync");
                        var request = new RestRequest();
                        request.Method = Method.Post;
                        request.Timeout = Timeout.Infinite;
                        request.AddParameter("modelName", "TQMYCTestApercentCalculatedModel");
                        request.AddParameter("userName", runConfiguration.getTQMAppUserID());
                        request.AddParameter("GUID", ycTestApercentCalc.ID);
                        request.AddParameter("testID", ycTestApercentCalc.testID);
                        request.AddParameter("userID", ycTestApercentCalc.userID);
                        request.AddParameter("TQMuserName", ycTestApercentCalc.userName);
                        request.AddParameter("machineID", ycTestApercentCalc.machineID);
                        request.AddParameter("machineCategory", ycTestApercentCalc.machineCategory);
                        request.AddParameter("machineName", ycTestApercentCalc.machineName);
                        request.AddParameter("shift", ycTestApercentCalc.shift);
                        request.AddParameter("process", ycTestApercentCalc.process);
                        request.AddParameter("countsysname", ycTestApercentCalc.countsysname);
                        request.AddParameter("yarnlenunit", ycTestApercentCalc.yarnlenunit);
                        request.AddParameter("yarnlength", ycTestApercentCalc.yarnlength);
                        request.AddParameter("testType", ycTestApercentCalc.testType);
                        request.AddParameter("totaltestcount", ycTestApercentCalc.totaltestcount);
                        request.AddParameter("avg_weight_nMinus1", ycTestApercentCalc.avg_weight_nMinus1);
                        request.AddParameter("testaverage_nMinus1", ycTestApercentCalc.testaverage_nMinus1);
                        request.AddParameter("testsd_nMinus1", ycTestApercentCalc.testsd_nMinus1);
                        request.AddParameter("testcv_nMinus1", ycTestApercentCalc.testcv_nMinus1);
                        request.AddParameter("max_nMinus1", ycTestApercentCalc.max_nMinus1);
                        request.AddParameter("min_nMinus1", ycTestApercentCalc.min_nMinus1);
                        request.AddParameter("range_nMinus1", ycTestApercentCalc.range_nMinus1);
                        request.AddParameter("apercent_nMinus1", ycTestApercentCalc.apercent_nMinus1);
                        request.AddParameter("avg_weight_N", ycTestApercentCalc.avg_weight_N);
                        request.AddParameter("testaverage_N", ycTestApercentCalc.testaverage_N);
                        request.AddParameter("testsd_N", ycTestApercentCalc.testsd_N);
                        request.AddParameter("testcv_N", ycTestApercentCalc.testcv_N);
                        request.AddParameter("max_N", ycTestApercentCalc.max_N);
                        request.AddParameter("min_N", ycTestApercentCalc.min_N);
                        request.AddParameter("range_N", ycTestApercentCalc.range_N);
                        request.AddParameter("avg_weight_nPlus1", ycTestApercentCalc.avg_weight_nPlus1);
                        request.AddParameter("testaverage_nPlus1", ycTestApercentCalc.testaverage_nPlus1);
                        request.AddParameter("testsd_nPlus1", ycTestApercentCalc.testsd_nPlus1);
                        request.AddParameter("testcv_nPlus1", ycTestApercentCalc.testcv_nPlus1);
                        request.AddParameter("max_nPlus1", ycTestApercentCalc.max_nPlus1);
                        request.AddParameter("min_nPlus1", ycTestApercentCalc.min_nPlus1);
                        request.AddParameter("range_nPlus1", ycTestApercentCalc.range_nPlus1);
                        request.AddParameter("apercent_nPlus1", ycTestApercentCalc.apercent_nPlus1);
                        request.AddParameter("status", ycTestApercentCalc.status);
                        request.AddParameter("createdate", getDateString(ycTestApercentCalc.createdate));


                        RestResponse response = client.Execute(request);
                        if (response.IsSuccessful)
                        {
                            if (response.StatusCode == System.Net.HttpStatusCode.Created ||
                                response.StatusCode == System.Net.HttpStatusCode.Accepted)
                            {
                                ycTestApercentCalc.dataSyncStatus = true;
                                int row = 0;
                                row = conn.Update(ycTestApercentCalc);
                                if (row > 0)
                                {
                                    syncDataCount = syncDataCount + 1;
                                }
                            }
                        }
                        //updateProgress("Uploading.....", 100, Color.BlueViolet);
                    }

                    if (syncDataCount == ycTestApercentCalculatedList.Count)
                    {
                        //success
                        //updateProgress("A% Calculation: Backup completed!!!", 100, Color.Green);
                        status = true;
                    }
                    else if (syncDataCount != ycTestApercentCalculatedList.Count)
                    {
                        if (syncDataCount > 0)
                        {
                            if (poorInternet)
                            {
                                //Poor internet but few record(s) pushed to server
                                //updateProgress("A% Calculation: Backup partially completed but failed due to poor internet connection!!!", 100, Color.Yellow);
                                showAlert("Attention", "A% Calculation: Backup partially completed but failed due to poor internet connection!!!", "OK");
                            }
                            else
                            {
                                //Internet is good but few records not pushed to server
                                if (!checkConnection())
                                {
                                    //updateProgress("A% Calculation: Backup partially completed but failed due to poor internet connection!!!", 100, Color.Yellow);
                                    showAlert("Attention", "A% Calculation: Backup partially completed but failed due to poor internet connection!!!", "OK");
                                }
                                else
                                {
                                    //updateProgress("A% Calculation: Backup completed partially but failed", 100, Color.Red);
                                    showAlert("Attention", "A% Calculation: Backup completed partially but failed", "OK");
                                }

                            }
                        }
                        else
                        {
                            if (poorInternet)
                            {
                                //Poor internet, so none of the record(s) pushed to server
                                //updateProgress("A% Calculation: Backup failed due to poor internet connection!!!", 100, Color.Yellow);
                                showAlert("Attention", "A% Calculation: Backup failed due to poor internet connection!!!", "OK");
                            }
                            else
                            {
                                //Internet is good but none of the record(s) pushed to server
                                if (!checkConnection())
                                {
                                    //updateProgress("A% Calculation: Backup failed due to poor internet connection!!!", 100, Color.Yellow);
                                    showAlert("Attention", "A% Calculation: Backup failed due to poor internet connection!!!", "OK");
                                }
                                else
                                {
                                    //updateProgress("A% Calculation: Backup failed. Please contact manufacturer!!!", 100, Color.Red);
                                    showAlert("Attention", "A% Calculation: Backup failed. Please contact manufacturer!!!", "OK");
                                }
                            }
                        }
                    }
                }
                if (status)
                {
                    CancellationTokenSource src = new CancellationTokenSource();
                    CancellationToken ct = src.Token;
                    ct.Register(() => Debug.WriteLine("ConnectBluetoothToken"));
                    await Task.Run(async () => await syncDataTQMStretchTestModel(), ct);
                    src.Cancel();
                }
                else
                {
                    backupStatus = false;
                    //toggleLoading(false);
                }
            }
            catch (Exception ex)
            {
                backupStatus = false;
                showAlert("Backup Failed!!!", "Error : " + ex.Message.ToString(), "OK");
                //updateProgress("Backup failed - " + ex.Message.ToString(), 100, Color.Red);
                //toggleLoading(false);

            }
        }

        private async Task syncDataTQMStretchTestModel()
        {
            try
            {
                bool status = false;
                bool poorInternet = false;
                //updateProgress("", 900, Color.White);
                if (!checkConnection())
                {
                    backupStatus = false;
                    showAlert("Backup failed!!!", "Please connect to Internet for Back-up", "OK");
                    //updateProgress("No internet! Backup failed!!!", 100, Color.Red);
                    //toggleLoading(false);
                    return;
                }
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    int syncDataCount = 0;
                    conn.CreateTable<StretchTestModel>();
                    List<StretchTestModel> stretchTestModelList = conn.Table<StretchTestModel>()
                        .Where(StretchTestModel => StretchTestModel.dataSyncStatus == false).ToList();
                    foreach (StretchTestModel stretchTest in stretchTestModelList)
                    {
                        //updateProgress("", 900, Color.BlueViolet);
                        if (!checkConnection())
                        {
                            backupStatus = false;
                            showAlert("Backup failed!!!", "Please connect to Internet for Back-up", "OK");
                            //updateProgress("No internet! Backup failed!!!", 100, Color.Red);
                            poorInternet = true;
                            break;
                        }
                        var client = new RestClient("https://myconsoleerp.herokuapp.com/tqm/datasync");
                        var request = new RestRequest();
                        request.Method = Method.Post;
                        request.Timeout = Timeout.Infinite;
                        request.AddParameter("modelName", "TQMStretchTestModel");
                        request.AddParameter("userName", runConfiguration.getTQMAppUserID());
                        request.AddParameter("GUID", stretchTest.ID);
                        request.AddParameter("testID", stretchTest.testID);
                        request.AddParameter("userID", stretchTest.userID);
                        request.AddParameter("TQMuserName", stretchTest.userName);
                        request.AddParameter("machineID", stretchTest.machineID);
                        request.AddParameter("machineCategory", stretchTest.machineCategory);
                        request.AddParameter("machineName", stretchTest.machineName);
                        request.AddParameter("shift", stretchTest.shift);
                        request.AddParameter("process", stretchTest.process);
                        request.AddParameter("countsysname", stretchTest.countsysname);
                        request.AddParameter("yarnlenunit", stretchTest.yarnlenunit);
                        request.AddParameter("yarnlength", stretchTest.yarnlength);
                        request.AddParameter("testType", stretchTest.testType);
                        request.AddParameter("totaltestcount", stretchTest.totaltestcount);
                        request.AddParameter("testcount", stretchTest.testcount);
                        request.AddParameter("yarnweight", stretchTest.yarnweight);
                        request.AddParameter("yccalcval", stretchTest.yccalcval);
                        request.AddParameter("status", stretchTest.status);
                        request.AddParameter("createdate", getDateString(stretchTest.createdate));

                        RestResponse response = client.Execute(request);
                        if (response.IsSuccessful)
                        {
                            if (response.StatusCode == System.Net.HttpStatusCode.Created ||
                                response.StatusCode == System.Net.HttpStatusCode.Accepted)
                            {
                                stretchTest.dataSyncStatus = true;
                                int row = 0;
                                row = conn.Update(stretchTest);
                                if (row > 0)
                                {
                                    syncDataCount = syncDataCount + 1;
                                }
                            }
                        }
                        //updateProgress("Uploading.....", 100, Color.BlueViolet);
                    }

                    if (syncDataCount == stretchTestModelList.Count)
                    {
                        //success
                        //updateProgress("Stretch : Backup completed!!!", 100, Color.Green);
                        status = true;
                    }
                    else if (syncDataCount != stretchTestModelList.Count)
                    {
                        if (syncDataCount > 0)
                        {
                            if (poorInternet)
                            {
                                //Poor internet but few record(s) pushed to server
                                //updateProgress("Stretch: Backup partially completed but failed due to poor internet connection!!!", 100, Color.Yellow);
                                showAlert("Attention", "Stretch: Backup partially completed but failed due to poor internet connection!!!", "OK");
                            }
                            else
                            {
                                //Internet is good but few records not pushed to server
                                if (!checkConnection())
                                {
                                    //updateProgress("Stretch: Backup partially completed but failed due to poor internet connection!!!", 100, Color.Yellow);
                                    showAlert("Attention", "Stretch: Backup partially completed but failed due to poor internet connection!!!", "OK");
                                }
                                else
                                {
                                    //updateProgress("Stretch: Backup completed partially but failed", 100, Color.Red);
                                    showAlert("Attention", "Stretch: Backup completed partially but failed", "OK");
                                }

                            }
                        }
                        else
                        {
                            if (poorInternet)
                            {
                                //Poor internet, so none of the record(s) pushed to server
                                //updateProgress("Stretch: Backup failed due to poor internet connection!!!", 100, Color.Yellow);
                                showAlert("Attention", "Stretch: Backup failed due to poor internet connection!!!", "OK");
                            }
                            else
                            {
                                //Internet is good but none of the record(s) pushed to server
                                if (!checkConnection())
                                {
                                    //updateProgress("Stretch: Backup failed due to poor internet connection!!!", 100, Color.Yellow);
                                    showAlert("Attention", "Stretch: Backup failed due to poor internet connection!!!", "OK");
                                }
                                else
                                {
                                    //updateProgress("Stretch: Backup failed. Please contact manufacturer!!!", 100, Color.Red);
                                    showAlert("Attention", "Stretch: Backup failed. Please contact manufacturer!!!", "OK");
                                }
                            }
                        }
                    }
                }
                if (status)
                {
                    CancellationTokenSource src = new CancellationTokenSource();
                    CancellationToken ct = src.Token;
                    ct.Register(() => Debug.WriteLine("ConnectBluetoothToken"));
                    await Task.Run(async () => await syncDataTQMStretchTestSummaryModel(), ct);
                    src.Cancel();
                }
                else
                {
                    backupStatus = false;
                    //toggleLoading(false);
                }
            }
            catch (Exception ex)
            {
                backupStatus = false;
                showAlert("Backup Failed!!!", "Error : " + ex.Message.ToString(), "OK");
                //updateProgress("Backup failed - " + ex.Message.ToString(), 100, Color.Red);
                //toggleLoading(false);

            }
        }

        private async Task syncDataTQMStretchTestSummaryModel()
        {
            try
            {
                bool status = false;
                bool poorInternet = false;
                //updateProgress("", 900, Color.White);
                if (!checkConnection())
                {
                    backupStatus = false;
                    showAlert("Backup failed!!!", "Please connect to Internet for Back-up", "OK");
                    //updateProgress("No internet! Backup failed!!!", 100, Color.Red);
                    //toggleLoading(false);
                    return;
                }
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    int syncDataCount = 0;
                    conn.CreateTable<StretchTestSummaryModel>();
                    List<StretchTestSummaryModel> stretchTestSummaryModelList = conn.Table<StretchTestSummaryModel>()
                        .Where(StretchTestSummaryModel => StretchTestSummaryModel.dataSyncStatus == false).ToList();
                    foreach (StretchTestSummaryModel stretchTestSummary in stretchTestSummaryModelList)
                    {
                        //updateProgress("", 900, Color.BlueViolet);
                        if (!checkConnection())
                        {
                            backupStatus = false;
                            showAlert("Backup failed!!!", "Please connect to Internet for Back-up", "OK");
                            //updateProgress("No internet! Backup failed!!!", 100, Color.Red);
                            poorInternet = true;
                            break;
                        }
                        var client = new RestClient("https://myconsoleerp.herokuapp.com/tqm/datasync");
                        var request = new RestRequest();
                        request.Method = Method.Post;
                        request.Timeout = Timeout.Infinite;
                        request.AddParameter("modelName", "TQMStretchTestSummaryModel");
                        request.AddParameter("userName", runConfiguration.getTQMAppUserID());
                        request.AddParameter("GUID", stretchTestSummary.ID);
                        request.AddParameter("testID", stretchTestSummary.testID);
                        request.AddParameter("userID", stretchTestSummary.userID);
                        request.AddParameter("TQMuserName", stretchTestSummary.userName);
                        request.AddParameter("machineID", stretchTestSummary.machineID);
                        request.AddParameter("machineCategory", stretchTestSummary.machineCategory);
                        request.AddParameter("machineName", stretchTestSummary.machineName);
                        request.AddParameter("shift", stretchTestSummary.shift);
                        request.AddParameter("process", stretchTestSummary.process);
                        request.AddParameter("countsysname", stretchTestSummary.countsysname);
                        request.AddParameter("yarnlenunit", stretchTestSummary.yarnlenunit);
                        request.AddParameter("yarnlength", stretchTestSummary.yarnlength);
                        request.AddParameter("testType", stretchTestSummary.testType);
                        request.AddParameter("totaltestcount", stretchTestSummary.totaltestcount);
                        request.AddParameter("avg_weight", stretchTestSummary.avg_weight);
                        request.AddParameter("testaverage", stretchTestSummary.testaverage);
                        request.AddParameter("testsd", stretchTestSummary.testsd);
                        request.AddParameter("testcv", stretchTestSummary.testcv);
                        request.AddParameter("status", stretchTestSummary.status);
                        request.AddParameter("createdate", getDateString(stretchTestSummary.createdate));


                        RestResponse response = client.Execute(request);
                        if (response.IsSuccessful)
                        {
                            if (response.StatusCode == System.Net.HttpStatusCode.Created ||
                                response.StatusCode == System.Net.HttpStatusCode.Accepted)
                            {
                                stretchTestSummary.dataSyncStatus = true;
                                int row = 0;
                                row = conn.Update(stretchTestSummary);
                                if (row > 0)
                                {
                                    syncDataCount = syncDataCount + 1;
                                }
                            }
                        }
                        //updateProgress("Uploading.....", 100, Color.BlueViolet);
                    }

                    if (syncDataCount == stretchTestSummaryModelList.Count)
                    {
                        //success
                        //updateProgress("Stretch Summary: Backup completed!!!", 100, Color.Green);
                        status = true;
                    }
                    else if (syncDataCount != stretchTestSummaryModelList.Count)
                    {
                        if (syncDataCount > 0)
                        {
                            if (poorInternet)
                            {
                                //Poor internet but few record(s) pushed to server
                                //updateProgress("Stretch Summary: Backup partially completed but failed due to poor internet connection!!!", 100, Color.Yellow);
                                showAlert("Attention", "Stretch Summary: Backup partially completed but failed due to poor internet connection!!!", "OK");
                            }
                            else
                            {
                                //Internet is good but few records not pushed to server
                                if (!checkConnection())
                                {
                                    //updateProgress("Stretch Summary: Backup partially completed but failed due to poor internet connection!!!", 100, Color.Yellow);
                                    showAlert("Attention", "Stretch Summary: Backup partially completed but failed due to poor internet connection!!!", "OK");
                                }
                                else
                                {
                                    //updateProgress("Stretch Summary: Backup completed partially but failed", 100, Color.Red);
                                    showAlert("Attention", "Stretch Summary: Backup completed partially but failed", "OK");
                                }

                            }
                        }
                        else
                        {
                            if (poorInternet)
                            {
                                //Poor internet, so none of the record(s) pushed to server
                                //updateProgress("Stretch Summary: Backup failed due to poor internet connection!!!", 100, Color.Yellow);
                                showAlert("Attention", "Stretch Summary: Backup failed due to poor internet connection!!!", "OK");
                            }
                            else
                            {
                                //Internet is good but none of the record(s) pushed to server
                                if (!checkConnection())
                                {
                                    //updateProgress("Stretch Summary: Backup failed due to poor internet connection!!!", 100, Color.Yellow);
                                    showAlert("Attention", "Stretch Summary: Backup failed due to poor internet connection!!!", "OK");
                                }
                                else
                                {
                                    //updateProgress("Stretch Summary: Backup failed. Please contact manufacturer!!!", 100, Color.Red);
                                    showAlert("Attention", "Stretch Summary: Backup failed. Please contact manufacturer!!!", "OK");
                                }
                            }
                        }
                    }
                }
                if (status)
                {
                    CancellationTokenSource src = new CancellationTokenSource();
                    CancellationToken ct = src.Token;
                    ct.Register(() => Debug.WriteLine("ConnectBluetoothToken"));
                    await Task.Run(async () => await syncDataTQMStretchTestCalculatedModel(), ct);
                    src.Cancel();
                }
                else
                {
                    backupStatus = false;
                    //toggleLoading(false);
                }
            }
            catch (Exception ex)
            {
                backupStatus = false;
                showAlert("Backup Failed!!!", "Error : " + ex.Message.ToString(), "OK");
                //updateProgress("Backup failed - " + ex.Message.ToString(), 100, Color.Red);
                //toggleLoading(false);

            }
        }

        private async Task syncDataTQMStretchTestCalculatedModel()
        {
            try
            {
                bool status = false;
                bool poorInternet = false;
                //updateProgress("", 900, Color.White);
                if (!checkConnection())
                {
                    backupStatus = false;
                    showAlert("Backup failed!!!", "Please connect to Internet for Back-up", "OK");
                    //updateProgress("No internet! Backup failed!!!", 100, Color.Red);
                    //toggleLoading(false);
                    return;
                }
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    int syncDataCount = 0;
                    conn.CreateTable<StretchTestCalculatedModel>();
                    List<StretchTestCalculatedModel> stretchTestCalculatedModelList = conn.Table<StretchTestCalculatedModel>()
                        .Where(StretchTestCalculatedModel => StretchTestCalculatedModel.dataSyncStatus == false).ToList();
                    foreach (StretchTestCalculatedModel stretchTestCalc in stretchTestCalculatedModelList)
                    {
                        //updateProgress("", 900, Color.BlueViolet);
                        if (!checkConnection())
                        {
                            backupStatus = false;
                            showAlert("Backup failed!!!", "Please connect to Internet for Back-up", "OK");
                            //updateProgress("No internet! Backup failed!!!", 100, Color.Red);
                            poorInternet = true;
                            break;
                        }
                        var client = new RestClient("https://myconsoleerp.herokuapp.com/tqm/datasync");
                        var request = new RestRequest();
                        request.Method = Method.Post;
                        request.Timeout = Timeout.Infinite;
                        request.AddParameter("modelName", "TQMStretchTestCalculatedModel");
                        request.AddParameter("userName", runConfiguration.getTQMAppUserID());
                        request.AddParameter("GUID", stretchTestCalc.ID);
                        request.AddParameter("testID", stretchTestCalc.testID);
                        request.AddParameter("userID", stretchTestCalc.userID);
                        request.AddParameter("TQMuserName", stretchTestCalc.userName);
                        request.AddParameter("machineID", stretchTestCalc.machineID);
                        request.AddParameter("machineCategory", stretchTestCalc.machineCategory);
                        request.AddParameter("machineName", stretchTestCalc.machineName);
                        request.AddParameter("shift", stretchTestCalc.shift);
                        request.AddParameter("process", stretchTestCalc.process);
                        request.AddParameter("countsysname", stretchTestCalc.countsysname);
                        request.AddParameter("yarnlenunit", stretchTestCalc.yarnlenunit);
                        request.AddParameter("yarnlength", stretchTestCalc.yarnlength);
                        request.AddParameter("testType", stretchTestCalc.testType);
                        request.AddParameter("totaltestcount", stretchTestCalc.totaltestcount);
                        request.AddParameter("avg_weight_IB", stretchTestCalc.avg_weight_IB);
                        request.AddParameter("testaverage_IB", stretchTestCalc.testaverage_IB);
                        request.AddParameter("testsd_IB", stretchTestCalc.testsd_IB);
                        request.AddParameter("testcv_IB", stretchTestCalc.testcv_IB);
                        request.AddParameter("max_IB", stretchTestCalc.max_IB);
                        request.AddParameter("min_IB", stretchTestCalc.min_IB);
                        request.AddParameter("range_IB", stretchTestCalc.range_IB);
                        request.AddParameter("avg_weight_FB", stretchTestCalc.avg_weight_FB);
                        request.AddParameter("testaverage_FB", stretchTestCalc.testaverage_FB);
                        request.AddParameter("testsd_FB", stretchTestCalc.testsd_FB);
                        request.AddParameter("testcv_FB", stretchTestCalc.testcv_FB);
                        request.AddParameter("max_FB", stretchTestCalc.max_FB);
                        request.AddParameter("min_FB", stretchTestCalc.min_FB);
                        request.AddParameter("range_FB", stretchTestCalc.range_FB);
                        request.AddParameter("stretch", stretchTestCalc.stretch);
                        request.AddParameter("status", stretchTestCalc.status);
                        request.AddParameter("createdate", getDateString(stretchTestCalc.createdate));



                        RestResponse response = client.Execute(request);
                        if (response.IsSuccessful)
                        {
                            if (response.StatusCode == System.Net.HttpStatusCode.Created ||
                                response.StatusCode == System.Net.HttpStatusCode.Accepted)
                            {
                                stretchTestCalc.dataSyncStatus = true;
                                int row = 0;
                                row = conn.Update(stretchTestCalc);
                                if (row > 0)
                                {
                                    syncDataCount = syncDataCount + 1;
                                }
                            }
                        }
                        //updateProgress("Uploading.....", 100, Color.BlueViolet);
                    }

                    if (syncDataCount == stretchTestCalculatedModelList.Count)
                    {
                        //success
                        //updateProgress("Stretch Calculation: Backup completed!!!", 100, Color.Green);
                        status = true;
                    }
                    else if (syncDataCount != stretchTestCalculatedModelList.Count)
                    {
                        if (syncDataCount > 0)
                        {
                            if (poorInternet)
                            {
                                //Poor internet but few record(s) pushed to server
                                //updateProgress("Stretch Calculation: Backup partially completed but failed due to poor internet connection!!!", 100, Color.Yellow);
                                showAlert("Attention", "Stretch Calculation: Backup partially completed but failed due to poor internet connection!!!", "OK");
                            }
                            else
                            {
                                //Internet is good but few records not pushed to server
                                if (!checkConnection())
                                {
                                    //updateProgress("Stretch Calculation: Backup partially completed but failed due to poor internet connection!!!", 100, Color.Yellow);
                                    showAlert("Attention", "Stretch Calculation: Backup partially completed but failed due to poor internet connection!!!", "OK");
                                }
                                else
                                {
                                    //updateProgress("Stretch Calculation: Backup completed partially but failed", 100, Color.Red);
                                    showAlert("Attention", "Stretch Calculation: Backup completed partially but failed", "OK");
                                }

                            }
                        }
                        else
                        {
                            if (poorInternet)
                            {
                                //Poor internet, so none of the record(s) pushed to server
                                //updateProgress("Stretch Calculation: Backup failed due to poor internet connection!!!", 100, Color.Yellow);
                                showAlert("Attention", "Stretch Calculation: Backup failed due to poor internet connection!!!", "OK");
                            }
                            else
                            {
                                //Internet is good but none of the record(s) pushed to server
                                if (!checkConnection())
                                {
                                    //updateProgress("Stretch Calculation: Backup failed due to poor internet connection!!!", 100, Color.Yellow);
                                    showAlert("Attention", "Stretch Calculation: Backup failed due to poor internet connection!!!", "OK");
                                }
                                else
                                {
                                    //updateProgress("Stretch Calculation: Backup failed. Please contact manufacturer!!!", 100, Color.Red);
                                    showAlert("Attention", "Stretch Calculation: Backup failed. Please contact manufacturer!!!", "OK");
                                }
                            }
                        }
                    }
                }
                if (status)
                {
                    CancellationTokenSource src = new CancellationTokenSource();
                    CancellationToken ct = src.Token;
                    ct.Register(() => Debug.WriteLine("ConnectBluetoothToken"));
                    await Task.Run(async () => await syncDataTQMNoilsTestModel(), ct);
                    src.Cancel();
                }
                else
                {
                    backupStatus = false;
                    //toggleLoading(false);
                }
            }
            catch (Exception ex)
            {
                backupStatus = false;
                showAlert("Backup Failed!!!", "Error : " + ex.Message.ToString(), "OK");
                //updateProgress("Backup failed - " + ex.Message.ToString(), 100, Color.Red);
                //toggleLoading(false);

            }
        }

        private async Task syncDataTQMNoilsTestModel()
        {
            try
            {
                bool status = false;
                bool poorInternet = false;
                //updateProgress("", 900, Color.White);
                if (!checkConnection())
                {
                    backupStatus = false;
                    showAlert("Backup failed!!!", "Please connect to Internet for Back-up", "OK");
                    //updateProgress("No internet! Backup failed!!!", 100, Color.Red);
                    //toggleLoading(false);
                    return;
                }
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    int syncDataCount = 0;
                    conn.CreateTable<NoilsTestModel>();
                    List<NoilsTestModel> noilsTestModelList = conn.Table<NoilsTestModel>()
                        .Where(NoilsTestModel => NoilsTestModel.dataSyncStatus == false).ToList();
                    foreach (NoilsTestModel noilsTest in noilsTestModelList)
                    {
                        //updateProgress("", 900, Color.BlueViolet);
                        if (!checkConnection())
                        {
                            backupStatus = false;
                            showAlert("Backup failed!!!", "Please connect to Internet for Back-up", "OK");
                            //updateProgress("No internet! Backup failed!!!", 100, Color.Red);
                            poorInternet = true;
                            break;
                        }
                        var client = new RestClient("https://myconsoleerp.herokuapp.com/tqm/datasync");
                        var request = new RestRequest();
                        request.Method = Method.Post;
                        request.Timeout = Timeout.Infinite;
                        request.AddParameter("modelName", "TQMNoilsTestModel");
                        request.AddParameter("userName", runConfiguration.getTQMAppUserID());
                        request.AddParameter("GUID", noilsTest.ID);
                        request.AddParameter("testID", noilsTest.testID);
                        request.AddParameter("userID", noilsTest.userID);
                        request.AddParameter("TQMuserName", noilsTest.userName);
                        request.AddParameter("machineID", noilsTest.machineID);
                        request.AddParameter("machineCategory", noilsTest.machineCategory);
                        request.AddParameter("machineName", noilsTest.machineName);
                        request.AddParameter("shift", noilsTest.shift);
                        request.AddParameter("process", noilsTest.process);
                        request.AddParameter("countsysname", noilsTest.countsysname);
                        request.AddParameter("yarnlenunit", noilsTest.yarnlenunit);
                        request.AddParameter("yarnlength", noilsTest.yarnlength);
                        request.AddParameter("testType", noilsTest.testType);
                        request.AddParameter("totaltestcount", noilsTest.totaltestcount);
                        request.AddParameter("testcount", noilsTest.testcount);
                        request.AddParameter("yarnweight", noilsTest.yarnweight);
                        request.AddParameter("yccalcval", noilsTest.yccalcval);
                        request.AddParameter("status", noilsTest.status);
                        request.AddParameter("createdate", getDateString(noilsTest.createdate));

                        RestResponse response = client.Execute(request);
                        if (response.IsSuccessful)
                        {
                            if (response.StatusCode == System.Net.HttpStatusCode.Created ||
                                response.StatusCode == System.Net.HttpStatusCode.Accepted)
                            {
                                noilsTest.dataSyncStatus = true;
                                int row = 0;
                                row = conn.Update(noilsTest);
                                if (row > 0)
                                {
                                    syncDataCount = syncDataCount + 1;
                                }
                            }
                        }
                        //updateProgress("Uploading.....", 100, Color.BlueViolet);
                    }

                    if (syncDataCount == noilsTestModelList.Count)
                    {
                        //success
                        //updateProgress("Noils: Backup completed!!!", 100, Color.Green);
                        status = true;
                    }
                    else if (syncDataCount != noilsTestModelList.Count)
                    {
                        if (syncDataCount > 0)
                        {
                            if (poorInternet)
                            {
                                //Poor internet but few record(s) pushed to server
                                //updateProgress("Noils: Backup partially completed but failed due to poor internet connection!!!", 100, Color.Yellow);
                                showAlert("Attention", "Noils: Backup partially completed but failed due to poor internet connection!!!", "OK");
                            }
                            else
                            {
                                //Internet is good but few records not pushed to server
                                if (!checkConnection())
                                {
                                    //updateProgress("Noils: Backup partially completed but failed due to poor internet connection!!!", 100, Color.Yellow);
                                    showAlert("Attention", "Noils: Backup partially completed but failed due to poor internet connection!!!", "OK");
                                }
                                else
                                {
                                    //updateProgress("Noils: Backup completed partially but failed", 100, Color.Red);
                                    showAlert("Attention", "Noils: Backup completed partially but failed", "OK");
                                }

                            }
                        }
                        else
                        {
                            if (poorInternet)
                            {
                                //Poor internet, so none of the record(s) pushed to server
                                //updateProgress("Noils: Backup failed due to poor internet connection!!!", 100, Color.Yellow);
                                showAlert("Attention", "Noils: Backup failed due to poor internet connection!!!", "OK");
                            }
                            else
                            {
                                //Internet is good but none of the record(s) pushed to server
                                if (!checkConnection())
                                {
                                    //updateProgress("Noils: Backup failed due to poor internet connection!!!", 100, Color.Yellow);
                                    showAlert("Attention", "Noils: Backup failed due to poor internet connection!!!", "OK");
                                }
                                else
                                {
                                    //updateProgress("Noils: Backup failed. Please contact manufacturer!!!", 100, Color.Red);
                                    showAlert("Attention", "Noils: Backup failed. Please contact manufacturer!!!", "OK");
                                }
                            }
                        }
                    }
                }
                if (status)
                {
                    CancellationTokenSource src = new CancellationTokenSource();
                    CancellationToken ct = src.Token;
                    ct.Register(() => Debug.WriteLine("ConnectBluetoothToken"));
                    await Task.Run(async () => await syncDataTQMNoilsTestSummaryModel(), ct);
                    src.Cancel();
                }
                else
                {
                    backupStatus = false;
                    //toggleLoading(false);
                }
            }
            catch (Exception ex)
            {
                backupStatus = false;
                showAlert("Backup Failed!!!", "Error : " + ex.Message.ToString(), "OK");
                //updateProgress("Backup failed - " + ex.Message.ToString(), 100, Color.Red);
                //toggleLoading(false);

            }
        }

        private async Task syncDataTQMNoilsTestSummaryModel()
        {
            try
            {
                bool status = false;
                bool poorInternet = false;
                //updateProgress("", 900, Color.White);
                if (!checkConnection())
                {
                    backupStatus = false;
                    showAlert("Backup failed!!!", "Please connect to Internet for Back-up", "OK");
                    //updateProgress("No internet! Backup failed!!!", 100, Color.Red);
                    //toggleLoading(false);
                    return;
                }
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    int syncDataCount = 0;
                    conn.CreateTable<NoilsTestSummaryModel>();
                    List<NoilsTestSummaryModel> noilsTestSummaryModelList = conn.Table<NoilsTestSummaryModel>()
                        .Where(NoilsTestSummaryModel => NoilsTestSummaryModel.dataSyncStatus == false).ToList();
                    foreach (NoilsTestSummaryModel noilsTestSummary in noilsTestSummaryModelList)
                    {
                        //updateProgress("", 900, Color.BlueViolet);
                        if (!checkConnection())
                        {
                            backupStatus = false;
                            showAlert("Backup failed!!!", "Please connect to Internet for Back-up", "OK");
                            //updateProgress("No internet! Backup failed!!!", 100, Color.Red);
                            poorInternet = true;
                            break;
                        }
                        var client = new RestClient("https://myconsoleerp.herokuapp.com/tqm/datasync");
                        var request = new RestRequest();
                        request.Method = Method.Post;
                        request.Timeout = Timeout.Infinite;
                        request.AddParameter("modelName", "TQMNoilsTestSummaryModel");
                        request.AddParameter("userName", runConfiguration.getTQMAppUserID());
                        request.AddParameter("GUID", noilsTestSummary.ID);
                        request.AddParameter("testID", noilsTestSummary.testID);
                        request.AddParameter("userID", noilsTestSummary.userID);
                        request.AddParameter("TQMuserName", noilsTestSummary.userName);
                        request.AddParameter("machineID", noilsTestSummary.machineID);
                        request.AddParameter("machineCategory", noilsTestSummary.machineCategory);
                        request.AddParameter("machineName", noilsTestSummary.machineName);
                        request.AddParameter("shift", noilsTestSummary.shift);
                        request.AddParameter("process", noilsTestSummary.process);
                        request.AddParameter("countsysname", noilsTestSummary.countsysname);
                        request.AddParameter("yarnlenunit", noilsTestSummary.yarnlenunit);
                        request.AddParameter("yarnlength", noilsTestSummary.yarnlength);
                        request.AddParameter("testType", noilsTestSummary.testType);
                        request.AddParameter("totaltestcount", noilsTestSummary.totaltestcount);
                        request.AddParameter("avg_weight", noilsTestSummary.avg_weight);
                        request.AddParameter("testaverage", noilsTestSummary.testaverage);
                        request.AddParameter("testsd", noilsTestSummary.testsd);
                        request.AddParameter("testcv", noilsTestSummary.testcv);
                        request.AddParameter("status", noilsTestSummary.status);
                        request.AddParameter("createdate", getDateString(noilsTestSummary.createdate));


                        RestResponse response = client.Execute(request);
                        if (response.IsSuccessful)
                        {
                            if (response.StatusCode == System.Net.HttpStatusCode.Created ||
                                response.StatusCode == System.Net.HttpStatusCode.Accepted)
                            {
                                noilsTestSummary.dataSyncStatus = true;
                                int row = 0;
                                row = conn.Update(noilsTestSummary);
                                if (row > 0)
                                {
                                    syncDataCount = syncDataCount + 1;
                                }
                            }
                        }
                        //updateProgress("Uploading.....", 100, Color.BlueViolet);
                    }

                    if (syncDataCount == noilsTestSummaryModelList.Count)
                    {
                        //success
                        //updateProgress("Noils Summary: Backup completed!!!", 100, Color.Green);
                        status = true;
                    }
                    else if (syncDataCount != noilsTestSummaryModelList.Count)
                    {
                        if (syncDataCount > 0)
                        {
                            if (poorInternet)
                            {
                                //Poor internet but few record(s) pushed to server
                                //updateProgress("Noils Summary: Backup partially completed but failed due to poor internet connection!!!", 100, Color.Yellow);
                                showAlert("Attention", "Noils Summary: Backup partially completed but failed due to poor internet connection!!!", "OK");
                            }
                            else
                            {
                                //Internet is good but few records not pushed to server
                                if (!checkConnection())
                                {
                                    //updateProgress("Noils Summary: Backup partially completed but failed due to poor internet connection!!!", 100, Color.Yellow);
                                    showAlert("Attention", "Noils Summary: Backup partially completed but failed due to poor internet connection!!!", "OK");
                                }
                                else
                                {
                                    //updateProgress("Noils Summary: Backup completed partially but failed", 100, Color.Red);
                                    showAlert("Attention", "Noils Summary: Backup completed partially but failed", "OK");
                                }

                            }
                        }
                        else
                        {
                            if (poorInternet)
                            {
                                //Poor internet, so none of the record(s) pushed to server
                                //updateProgress("Noils Summary: Backup failed due to poor internet connection!!!", 100, Color.Yellow);
                                showAlert("Attention", "Noils Summary: Backup failed due to poor internet connection!!!", "OK");
                            }
                            else
                            {
                                //Internet is good but none of the record(s) pushed to server
                                if (!checkConnection())
                                {
                                    //updateProgress("Noils Summary: Backup failed due to poor internet connection!!!", 100, Color.Yellow);
                                    showAlert("Attention", "Noils Summary: Backup failed due to poor internet connection!!!", "OK");
                                }
                                else
                                {
                                    //updateProgress("Noils Summary: Backup failed. Please contact manufacturer!!!", 100, Color.Red);
                                    showAlert("Attention", "Noils Summary: Backup failed. Please contact manufacturer!!!", "OK");
                                }
                            }
                        }
                    }
                }
                if (status)
                {
                    CancellationTokenSource src = new CancellationTokenSource();
                    CancellationToken ct = src.Token;
                    ct.Register(() => Debug.WriteLine("ConnectBluetoothToken"));
                    await Task.Run(async () => await syncDataTQMNoilsTestFinalModel(), ct);
                    src.Cancel();
                }
                else
                {
                    backupStatus = false;
                    //toggleLoading(false);
                }
            }
            catch (Exception ex)
            {
                backupStatus = false;
                showAlert("Backup Failed!!!", "Error : " + ex.Message.ToString(), "OK");
                //updateProgress("Backup failed - " + ex.Message.ToString(), 100, Color.Red);
                //toggleLoading(false);

            }
        }

        private async Task syncDataTQMNoilsTestFinalModel()
        {
            try
            {
                bool status = false;
                bool poorInternet = false;
                //updateProgress("", 900, Color.White);
                if (!checkConnection())
                {
                    backupStatus = false;
                    showAlert("Backup failed!!!", "Please connect to Internet for Back-up", "OK");
                    //updateProgress("No internet! Backup failed!!!", 100, Color.Red);
                    //toggleLoading(false);
                    return;
                }
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    int syncDataCount = 0;
                    conn.CreateTable<NoilsTestFinalModel>();
                    List<NoilsTestFinalModel> noilsTestFinalModelList = conn.Table<NoilsTestFinalModel>()
                        .Where(NoilsTestFinalModel => NoilsTestFinalModel.dataSyncStatus == false).ToList();
                    foreach (NoilsTestFinalModel noilsTestFinal in noilsTestFinalModelList)
                    {
                        //updateProgress("", 900, Color.BlueViolet);
                        if (!checkConnection())
                        {
                            backupStatus = false;
                            showAlert("Backup failed!!!", "Please connect to Internet for Back-up", "OK");
                            //updateProgress("No internet! Backup failed!!!", 100, Color.Red);
                            poorInternet = true;
                            break;
                        }
                        var client = new RestClient("https://myconsoleerp.herokuapp.com/tqm/datasync");
                        var request = new RestRequest();
                        request.Method = Method.Post;
                        request.Timeout = Timeout.Infinite;
                        request.AddParameter("modelName", "TQMNoilsTestFinalModel");
                        request.AddParameter("userName", runConfiguration.getTQMAppUserID());
                        request.AddParameter("GUID", noilsTestFinal.ID);
                        request.AddParameter("testID", noilsTestFinal.testID);
                        request.AddParameter("testcount", noilsTestFinal.testcount);
                        request.AddParameter("weigth_sliver", noilsTestFinal.weigth_sliver);
                        request.AddParameter("weigth_noils", noilsTestFinal.weigth_noils);
                        request.AddParameter("noils", noilsTestFinal.noils);
                        request.AddParameter("status", noilsTestFinal.status);
                        request.AddParameter("createdate", getDateString(noilsTestFinal.createdate));

                        RestResponse response = client.Execute(request);
                        if (response.IsSuccessful)
                        {
                            if (response.StatusCode == System.Net.HttpStatusCode.Created ||
                                response.StatusCode == System.Net.HttpStatusCode.Accepted)
                            {
                                noilsTestFinal.dataSyncStatus = true;
                                int row = 0;
                                row = conn.Update(noilsTestFinal);
                                if (row > 0)
                                {
                                    syncDataCount = syncDataCount + 1;
                                }
                            }
                        }
                        //updateProgress("Uploading.....", 100, Color.BlueViolet);
                    }

                    if (syncDataCount == noilsTestFinalModelList.Count)
                    {
                        //success
                        //updateProgress("Noils Final Test: Backup completed!!!", 100, Color.Green);
                        status = true;
                    }
                    else if (syncDataCount != noilsTestFinalModelList.Count)
                    {
                        if (syncDataCount > 0)
                        {
                            if (poorInternet)
                            {
                                //Poor internet but few record(s) pushed to server
                                //updateProgress("Noils Final Test: Backup partially completed but failed due to poor internet connection!!!", 100, Color.Yellow);
                                showAlert("Attention", "Noils Final Test: Backup partially completed but failed due to poor internet connection!!!", "OK");
                            }
                            else
                            {
                                //Internet is good but few records not pushed to server
                                if (!checkConnection())
                                {
                                    //updateProgress("Noils Final Test: Backup partially completed but failed due to poor internet connection!!!", 100, Color.Yellow);
                                    showAlert("Attention", "Noils Final Test: Backup partially completed but failed due to poor internet connection!!!", "OK");
                                }
                                else
                                {
                                    //updateProgress("Noils Final Test: Backup completed partially but failed", 100, Color.Red);
                                    showAlert("Attention", "Noils Final Test: Backup completed partially but failed", "OK");
                                }

                            }
                        }
                        else
                        {
                            if (poorInternet)
                            {
                                //Poor internet, so none of the record(s) pushed to server
                                //updateProgress("Noils Final Test: Backup failed due to poor internet connection!!!", 100, Color.Yellow);
                                showAlert("Attention", "Noils Final Test: Backup failed due to poor internet connection!!!", "OK");
                            }
                            else
                            {
                                //Internet is good but none of the record(s) pushed to server
                                if (!checkConnection())
                                {
                                    //updateProgress("Noils Final Test: Backup failed due to poor internet connection!!!", 100, Color.Yellow);
                                    showAlert("Attention", "Noils Final Test: Backup failed due to poor internet connection!!!", "OK");
                                }
                                else
                                {
                                    //updateProgress("Noils Final Test: Backup failed. Please contact manufacturer!!!", 100, Color.Red);
                                    showAlert("Attention", "Noils Final Test: Backup failed. Please contact manufacturer!!!", "OK");
                                }
                            }
                        }
                    }
                }
                if (status)
                {
                    CancellationTokenSource src = new CancellationTokenSource();
                    CancellationToken ct = src.Token;
                    ct.Register(() => Debug.WriteLine("ConnectBluetoothToken"));
                    await Task.Run(async () => await syncDataTQMNoilsTestCalculatedModel(), ct);
                    src.Cancel();
                }
                else
                {
                    backupStatus = false;
                    //toggleLoading(false);
                }
            }
            catch (Exception ex)
            {
                backupStatus = false;
                showAlert("Backup Failed!!!", "Error : " + ex.Message.ToString(), "OK");
                //updateProgress("Backup failed - " + ex.Message.ToString(), 100, Color.Red);
                //toggleLoading(false);

            }
        }

        private async Task syncDataTQMNoilsTestCalculatedModel()
        {
            try
            {
                bool status = false;
                bool poorInternet = false;
                //updateProgress("", 900, Color.White);
                if (!checkConnection())
                {
                    backupStatus = false;
                    showAlert("Backup failed!!!", "Please connect to Internet for Back-up", "OK");
                    //updateProgress("No internet! Backup failed!!!", 100, Color.Red);
                    //toggleLoading(false);
                    return;
                }
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    int syncDataCount = 0;
                    conn.CreateTable<NoilsTestCalculatedModel>();
                    List<NoilsTestCalculatedModel> noilsTestCalculatedModelList = conn.Table<NoilsTestCalculatedModel>()
                        .Where(NoilsTestCalculatedModel => NoilsTestCalculatedModel.dataSyncStatus == false).ToList();
                    foreach (NoilsTestCalculatedModel noilsTestCalc in noilsTestCalculatedModelList)
                    {
                        //updateProgress("", 900, Color.BlueViolet);
                        if (!checkConnection())
                        {
                            backupStatus = false;
                            showAlert("Backup failed!!!", "Please connect to Internet for Back-up", "OK");
                            //updateProgress("No internet! Backup failed!!!", 100, Color.Red);
                            poorInternet = true;
                            break;
                        }
                        var client = new RestClient("https://myconsoleerp.herokuapp.com/tqm/datasync");
                        var request = new RestRequest();
                        request.Method = Method.Post;
                        request.Timeout = Timeout.Infinite;
                        request.AddParameter("modelName", "TQMNoilsTestCalculatedModel");
                        request.AddParameter("userName", runConfiguration.getTQMAppUserID());
                        request.AddParameter("GUID", noilsTestCalc.ID);
                        request.AddParameter("testID", noilsTestCalc.testID);
                        request.AddParameter("userID", noilsTestCalc.userID);
                        request.AddParameter("TQMuserName", noilsTestCalc.userName);
                        request.AddParameter("machineID", noilsTestCalc.machineID);
                        request.AddParameter("machineCategory", noilsTestCalc.machineCategory);
                        request.AddParameter("machineName", noilsTestCalc.machineName);
                        request.AddParameter("shift", noilsTestCalc.shift);
                        request.AddParameter("process", noilsTestCalc.process);
                        request.AddParameter("countsysname", noilsTestCalc.countsysname);
                        request.AddParameter("yarnlenunit", noilsTestCalc.yarnlenunit);
                        request.AddParameter("yarnlength", noilsTestCalc.yarnlength);
                        request.AddParameter("totaltestcount", noilsTestCalc.totaltestcount);
                        request.AddParameter("average_wt_sliverwt", noilsTestCalc.average_wt_sliverwt);
                        request.AddParameter("max_sliverwt", noilsTestCalc.max_sliverwt);
                        request.AddParameter("min_sliverwt", noilsTestCalc.min_sliverwt);
                        request.AddParameter("range_sliverwt", noilsTestCalc.range_sliverwt);
                        request.AddParameter("testaverage_sliverwt", noilsTestCalc.testaverage_sliverwt);
                        request.AddParameter("testsd_sliverwt", noilsTestCalc.testsd_sliverwt);
                        request.AddParameter("testcv_sliverwt", noilsTestCalc.testcv_sliverwt);
                        request.AddParameter("average_wt_noilswt", noilsTestCalc.average_wt_noilswt);
                        request.AddParameter("max_noilswt", noilsTestCalc.max_noilswt);
                        request.AddParameter("min_noilswt", noilsTestCalc.min_noilswt);
                        request.AddParameter("range_noilswt", noilsTestCalc.range_noilswt);
                        request.AddParameter("testaverage_noilswt", noilsTestCalc.testaverage_noilswt);
                        request.AddParameter("testsd_noilswt", noilsTestCalc.testsd_noilswt);
                        request.AddParameter("testcv_noilswt", noilsTestCalc.testcv_noilswt);
                        request.AddParameter("average_wt_noils", noilsTestCalc.average_wt_noils);
                        request.AddParameter("max_noils", noilsTestCalc.max_noils);
                        request.AddParameter("min_noils", noilsTestCalc.min_noils);
                        request.AddParameter("range_noils", noilsTestCalc.range_noils);
                        request.AddParameter("testsd_noils", noilsTestCalc.testsd_noils);
                        request.AddParameter("testcv_noils", noilsTestCalc.testcv_noils);
                        request.AddParameter("status", noilsTestCalc.status);
                        request.AddParameter("createdate", getDateString(noilsTestCalc.createdate));


                        RestResponse response = client.Execute(request);
                        if (response.IsSuccessful)
                        {
                            if (response.StatusCode == System.Net.HttpStatusCode.Created ||
                                response.StatusCode == System.Net.HttpStatusCode.Accepted)
                            {
                                noilsTestCalc.dataSyncStatus = true;
                                int row = 0;
                                row = conn.Update(noilsTestCalc);
                                if (row > 0)
                                {
                                    syncDataCount = syncDataCount + 1;
                                }
                            }
                        }
                        //updateProgress("Uploading.....", 100, Color.BlueViolet);
                    }

                    if (syncDataCount == noilsTestCalculatedModelList.Count)
                    {
                        //success
                        //updateProgress("Backup completed!!!", 100, Color.Green);
                        status = true;
                    }
                    else if (syncDataCount != noilsTestCalculatedModelList.Count)
                    {
                        if (syncDataCount > 0)
                        {
                            if (poorInternet)
                            {
                                //Poor internet but few record(s) pushed to server
                                //updateProgress("Noils Calculations: Backup partially completed but failed due to poor internet connection!!!", 100, Color.Yellow);
                                showAlert("Attention", "Noils Calculations: Backup partially completed but failed due to poor internet connection!!!", "OK");
                            }
                            else
                            {
                                //Internet is good but few records not pushed to server
                                if (!checkConnection())
                                {
                                    //updateProgress("Noils Calculations: Backup partially completed but failed due to poor internet connection!!!", 100, Color.Yellow);
                                    showAlert("Attention", "Noils Calculations: Backup partially completed but failed due to poor internet connection!!!", "OK");
                                }
                                else
                                {
                                    //updateProgress("Noils Calculations: Backup completed partially but failed", 100, Color.Red);
                                    showAlert("Attention", "Noils Calculations: Backup completed partially but failed", "OK");
                                }

                            }
                        }
                        else
                        {
                            if (poorInternet)
                            {
                                //Poor internet, so none of the record(s) pushed to server
                                //updateProgress("Noils Calculations: Backup failed due to poor internet connection!!!", 100, Color.Yellow);
                                showAlert("Attention", "Noils Calculations: Backup failed due to poor internet connection!!!", "OK");
                            }
                            else
                            {
                                //Internet is good but none of the record(s) pushed to server
                                if (!checkConnection())
                                {
                                    //updateProgress("Noils Calculations: Backup failed due to poor internet connection!!!", 100, Color.Yellow);
                                    showAlert("Attention", "Noils Calculations: Backup failed due to poor internet connection!!!", "OK");
                                }
                                else
                                {
                                    //updateProgress("Noils Calculations: Backup failed. Please contact manufacturer!!!", 100, Color.Red);
                                    showAlert("Attention", "Noils Calculations: Backup failed. Please contact manufacturer!!!", "OK");
                                }
                            }
                        }
                    }
                }
                if (status)
                {
                    //toggleLoading(false);
                    backupStatus = false;
                    showAlert("Success!!!", "Back-up Completed", "OK");
                }
                else
                {
                    backupStatus = false;
                    //toggleLoading(false);
                }
            }
            catch (Exception ex)
            {
                backupStatus = false;
                showAlert("Backup Failed!!!", "Error : " + ex.Message.ToString(), "OK");
                //updateProgress("Backup failed - " + ex.Message.ToString(), 100, Color.Red);
                //toggleLoading(false);

            }
        }
    }
}
