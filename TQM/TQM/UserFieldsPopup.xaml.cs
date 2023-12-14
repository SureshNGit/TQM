using System;
using Xamarin.CommunityToolkit.UI.Views;

namespace TQM
{	
	public partial class UserFieldsPopup : Popup
    {	
		public UserFieldsPopup ()
		{
			InitializeComponent ();
		}


        private void btn_confirm_Clicked(System.Object sender, System.EventArgs e)
        {
            try
            {


                
                Dismiss("Success~" + entry_UF1.ToString() + "|~*~|" + entry_UF2.ToString() + "|~*~|" + entry_UF3.ToString() + "|~*~|" + entry_UF4.ToString());
            }
            catch (Exception)
            {
                Dismiss("Error occurred!!! Try again...");
            }
        }

        
    }


}

