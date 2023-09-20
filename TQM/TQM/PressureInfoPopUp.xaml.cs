using System;
using SQLite;
using TQM.Model;
using Xamarin.CommunityToolkit.UI.Views;

namespace TQM
{
    public partial class PressureInfoPopUp : Popup
    {
        private Guid macID = Guid.Empty;
        private string macName = null;
        public PressureInfoPopUp(Guid machineID, string machineName)
		{
			InitializeComponent ();
            macID = machineID;
            macName = machineName;
            using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            {
                ConfigModel macDetails = conn.Table<ConfigModel>().Where(ConfigModel =>
                                        (ConfigModel.machineID == macID && ConfigModel.machineName == macName)).FirstOrDefault();
                if (macDetails == null)
                {
                    Dismiss("Unable to reterive selected machine details. Please contact admin!!!");
                }
                else
                {
                    entry_speed.Text = macDetails.speed.ToString();
                    entry_p1.Text = macDetails.p1.ToString();
                    entry_p2.Text = macDetails.p2.ToString();
                    entry_n1.Text = macDetails.n1.ToString();
                }
            }
        }

        private void btn_confirm_Clicked(System.Object sender, System.EventArgs e)
        {
            try
            {
                if (entry_speed.Text.Trim().Contains(".") || entry_speed.Text.Trim().Contains("-"))
                {
                    lbl_error.Text = "Speed should not be a decimal or negative value!!!";
                    return;
                }
                if (entry_speed.Text.Trim() == "" || int.Parse(entry_speed.Text.Trim()) == 0)
                {
                    lbl_error.Text = "Speed should not be blank or zero!!!";
                    return;
                }
                if (entry_p1.Text.Trim().Contains("-"))
                {
                    lbl_error.Text = "P1 should not be a negative value!!!";
                    return;
                }
                if (entry_p1.Text.Trim() == "" || decimal.Parse(entry_p1.Text.Trim()) == 0.0m)
                {
                    lbl_error.Text = "P1 should not be blank or zero!!!";
                    return;
                }
                if (entry_p2.Text.Trim().Contains("-"))
                {
                    lbl_error.Text = "P2 should not be a negative value!!!";
                    return;
                }
                if (entry_p2.Text.Trim() == "" || decimal.Parse(entry_p2.Text.Trim()) == 0.0m)
                {
                    lbl_error.Text = "P2 should not be blank or zero!!!";
                    return;
                }
                if (entry_n1.Text.Trim().Contains("-"))
                {
                    lbl_error.Text = "N1 should not be a negative value!!!";
                    return;
                }
                if (entry_n1.Text.Trim() == "" || decimal.Parse(entry_n1.Text.Trim()) == 0.0m)
                {
                    lbl_error.Text = "N1 should not be blank or zero!!!";
                    return;
                }

                int speed = 0;
                int.TryParse(entry_speed.Text.Trim(), out speed);
                decimal p1 = 0.0m;
                decimal.TryParse(entry_p1.Text.Trim(), out p1);
                decimal p2 = 0.0m;
                decimal.TryParse(entry_p2.Text.Trim(), out p2);
                decimal n1 = 0.0m;
                decimal.TryParse(entry_n1.Text.Trim(), out n1);
                //using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                //{
                //    ConfigModel macDetails = conn.Table<ConfigModel>().Where(ConfigModel =>
                //                            (ConfigModel.machineID == macID && ConfigModel.machineName == macName)).FirstOrDefault();
                //    if (macDetails == null)
                //    {
                //        Dismiss("Unable to reterive selected machine details. Please contact admin!!!");
                //    }
                //    else
                //    {
                //        macDetails.speed = speed;
                //        macDetails.p1 = p1;
                //        macDetails.p2 = p2;
                //        macDetails.n1 = n1;
                //        int row = conn.Update(macDetails);

                //        if (row>0)
                //        {
                //            Dismiss("Success");
                //        }
                //        else
                //        {
                //            Dismiss("Failed to update machine details. Please try again");
                //        }

                //    }
                //}

                Dismiss("Success~"+speed.ToString() +"|"+ p1.ToString() + "|"+ p2.ToString() + "|"+ n1.ToString());
            }
            catch (Exception)
            {
                Dismiss("Error occurred while reteriving machine information!!! Try again...");
            }
        }

    }
}

