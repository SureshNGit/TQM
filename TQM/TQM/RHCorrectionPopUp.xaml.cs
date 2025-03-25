using System;
using SQLite;
using TQM.Model;
using Xamarin.CommunityToolkit.UI.Views;

namespace TQM
{
    public partial class RHCorrectionPopUp : Popup
    {
        public RHCorrectionPopUp()
		{
			InitializeComponent ();
            entry_RH_Correction_Percent.Text = "0";
        }

        private void btn_confirm_Clicked(System.Object sender, System.EventArgs e)
        {
            try
            {
                if (entry_RH_Correction_Percent.Text.Trim().Contains(".") || entry_RH_Correction_Percent.Text.Trim().Contains("-"))
                {
                    lbl_error.Text = "RH correction percentage should not be a decimal or negative value!!!";
                    return;
                }
                if (entry_RH_Correction_Percent.Text.Trim() == "" || int.Parse(entry_RH_Correction_Percent.Text.Trim()) == 0)
                {
                    lbl_error.Text = "RH correction percentage should not be blank or zero!!!";
                    return;
                }
                               
                int RH_Correction_Percentage = 0;
                int.TryParse(entry_RH_Correction_Percent.Text.Trim(), out RH_Correction_Percentage);

                if (getRHCorrectionFactor(RH_Correction_Percentage) == 0.000m)
                {
                    lbl_error.Text = "Invalid RH correction percentage!!!. It should be from 34 till 99";
                    return;
                }

                Dismiss("Success~"+ RH_Correction_Percentage.ToString());
            }
            catch (Exception ex)
            {
                Dismiss("Error occurred!!!" + ex.Message.ToString());
            }
        }

        private decimal getRHCorrectionFactor(int rh_correction_percent)
        {
            if (rh_correction_percent >= 34 && rh_correction_percent < 39)
            {
                return 0.971m;
            }
            else if (rh_correction_percent >= 39 && rh_correction_percent < 44)
            {
                return 0.973m;
            }
            else if (rh_correction_percent >= 44 && rh_correction_percent < 49)
            {
                return 0.977m;
            }
            else if (rh_correction_percent >= 49 && rh_correction_percent < 54)
            {
                return 0.984m;
            }
            else if (rh_correction_percent >= 54 && rh_correction_percent < 59)
            {
                return 0.989m;
            }
            else if (rh_correction_percent >= 59 && rh_correction_percent < 64)
            {
                return 0.995m;
            }
            else if (rh_correction_percent >= 64 && rh_correction_percent < 69)
            {
                return 1.000m;
            }
            else if (rh_correction_percent >= 69 && rh_correction_percent < 74)
            {
                return 1.005m;
            }
            else if (rh_correction_percent >= 74 && rh_correction_percent < 79)
            {
                return 1.011m;
            }
            else if (rh_correction_percent >= 79 && rh_correction_percent < 84)
            {
                return 1.017m;
            }
            else if (rh_correction_percent >= 84 && rh_correction_percent < 89)
            {
                return 1.030m;
            }
            else if (rh_correction_percent >= 89 && rh_correction_percent < 95)
            {
                return 1.045m;
            }
            else if (rh_correction_percent >= 95 && rh_correction_percent < 100)
            {
                return 1.082m;
            }
            else
            {
                return 0.000m;
            }
        }

    }
}

