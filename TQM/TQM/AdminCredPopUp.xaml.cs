using System;
//using Rg.Plugins.Popup.Pages;
using SQLite;
using TQM.Model;
using Xamarin.CommunityToolkit.UI.Views;

namespace TQM
{
    public partial class AdminCredPopUp : Popup
    {
        private StrengthAnalyzer sa = new StrengthAnalyzer();
        public AdminCredPopUp ()
		{
			InitializeComponent ();
		}

        private void btn_checkCreds_Clicked(System.Object sender, System.EventArgs e)
        {
            try
            {
                string username = entry_username.Text.Trim();
                string password = entry_password.Text.Trim();
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    UserModel loggedInUser = conn.Table<UserModel>().Where(UserModel =>
                                            (UserModel.userId == username
                                            && UserModel.password == password)).FirstOrDefault();
                    if (loggedInUser == null)
                    {
                        Dismiss("Invalid Credentials. Try again....!!!");
                    }
                    else
                    {
                        if (loggedInUser.isAdmin)
                        {
                            //Environment.SetEnvironmentVariable("DrumSelectionMethodChange", "YES");

                            //sa.toggleDrumSelectionMethod(true);
                            Dismiss("Success");
                        }
                        else
                        {
                            //Environment.SetEnvironmentVariable("DrumSelectionMethodChange", "NO");
                            //sa.toggleDrumSelectionMethod(false);
                            Dismiss("Entered user credential is not an Admin");
                        }

                    }
                }
            }
            catch (Exception)
            {
                Dismiss("Error occurred while reteriving user information!!! Try again...");
            }
        }

       
    }
}

