using System;
using Xamarin.CommunityToolkit.UI.Views;

namespace TQM
{	
	public partial class UserFieldsPopup : Popup
    {
        private string selectedCategory = null;
        private Guid selectedMachineID = Guid.Empty;
        private string selectedMachineName = null;
		public UserFieldsPopup ()
		{
			InitializeComponent ();
		}

        private void reset()
        {
            lbl_UF1.IsVisible = false;
            lbl_UF2.IsVisible = false;
            lbl_UF3.IsVisible = false;
            lbl_UF4.IsVisible = false;

            entry_UF1.IsVisible = false;
            entry_UF2.IsVisible = false;
            entry_UF3.IsVisible = false;
            entry_UF4.IsVisible = false;

            lbl_UF1.Text = "";
            lbl_UF2.Text = "";
            lbl_UF3.Text = "";
            lbl_UF4.Text = "";

            entry_UF1.Text = "";
            entry_UF2.Text = "";
            entry_UF3.Text = "";
            entry_UF4.Text = "";
        }

        public UserFieldsPopup(string macCat, Guid macID, string macName, string UF1, string UF2, string UF3, string UF4,
                                                                        string UF_Val1, string UF_Val2, string UF_Val3, string UF_Val4)
        {
            InitializeComponent();
            reset();
            selectedCategory = macCat;
            selectedMachineID = macID;
            selectedMachineName = macName;
            if(UF1 != null && UF1 != "")
            {
                lbl_UF1.IsVisible = true;
                entry_UF1.IsVisible = true;
                lbl_UF1.Text = UF1;
                entry_UF1.Text = UF_Val1;
            }
            if (UF2 != null && UF2 != "")
            {
                lbl_UF2.IsVisible = true;
                entry_UF2.IsVisible = true;
                lbl_UF2.Text = UF2;
                entry_UF2.Text = UF_Val2;
            }
            if (UF3 != null && UF3 != "")
            {
                lbl_UF3.IsVisible = true;
                entry_UF3.IsVisible = true;
                lbl_UF3.Text = UF3;
                entry_UF3.Text = UF_Val3;
            }
            if (UF4 != null && UF4 != "")
            {
                lbl_UF4.IsVisible = true;
                entry_UF4.IsVisible = true;
                lbl_UF4.Text = UF4;
                entry_UF4.Text = UF_Val4;
            }
        }


        private void btn_confirm_Clicked(System.Object sender, System.EventArgs e)
        {
            try
            {
                if (entry_UF1.Text.Contains("|"))
                {
                    lbl_error.Text = "Please remove '|' symbol from "+ lbl_UF1.Text + " field value";
                    return;
                }
                if (entry_UF2.Text.Contains("|"))
                {
                    lbl_error.Text = "Please remove '|' symbol from " + lbl_UF2.Text + " field value";
                    return;
                }
                if (entry_UF3.Text.Contains("|"))
                {
                    lbl_error.Text = "Please remove '|' symbol from " + lbl_UF3.Text + " field value";
                    return;
                }
                if (entry_UF4.Text.Contains("|"))
                {
                    lbl_error.Text = "Please remove '|' symbol from " + lbl_UF4.Text + " field value";
                    return;
                }
                Dismiss("Success~" + entry_UF1.Text + "|" + entry_UF2.Text + "|" + entry_UF3.Text + "|" + entry_UF4.Text);
            }
            catch (Exception)
            {
                Dismiss("Error occurred!!! Try again...");
            }
        }

        
    }


}

