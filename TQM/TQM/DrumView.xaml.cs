using System;
using System.Collections.Generic;
using TQM.ModelView;
using Xamarin.Forms;

namespace TQM
{	
	public partial class DrumView : ContentPage
	{
		private string selectedMachineCategory = null;
		private Guid selectedMachineID = Guid.Empty;
		private string selectedMachineName = null;
		private int selectedSectionNo = 0;
		private int selectedDrumStartNo = 0;
		private int selectedDrumEndNo = 0;
        private ViewCell lastCell=null;

        public DrumView ()
		{
			InitializeComponent ();
		}

        public DrumView(string macCat, Guid macID, string macName, int sectionNo, int startDrumNo, int endDrumNo)
        {
            InitializeComponent();
			selectedMachineCategory = macCat;
			selectedMachineID = macID;
			selectedMachineName = macName;
			selectedSectionNo = sectionNo;
			selectedDrumStartNo = startDrumNo;
			selectedDrumEndNo = endDrumNo;
			generateDrumView();
        }

		public void generateDrumView()
		{
			int totalDrums = (selectedDrumEndNo - selectedDrumStartNo)+1;
			List<DrumMV> dv_list = new List<DrumMV>();
			for(int i = 0; i < totalDrums; i++)
			{
				DrumMV dv = new DrumMV();
				if ((i + 1) <= totalDrums) { dv.D1 = (i + 1).ToString(); dv.D1_Visible = true; } else { dv.D1 = ""; dv.D1_Visible = false; }
                if ((i + 2) <= totalDrums) { dv.D2 = (i + 2).ToString(); dv.D2_Visible = true; } else { dv.D2 = ""; dv.D2_Visible = false; }
                if ((i + 3) <= totalDrums) { dv.D3 = (i + 3).ToString(); dv.D3_Visible = true; } else { dv.D3 = ""; dv.D3_Visible = false; }
                if ((i + 4) <= totalDrums) { dv.D4 = (i + 4).ToString(); dv.D4_Visible = true; } else { dv.D4 = ""; dv.D4_Visible = false; }
                if ((i + 5) <= totalDrums) { dv.D5 = (i + 5).ToString(); dv.D5_Visible = true; } else { dv.D5 = ""; dv.D5_Visible = false; }
                if ((i + 6) <= totalDrums) { dv.D6 = (i + 6).ToString(); dv.D6_Visible = true; } else { dv.D6 = ""; dv.D6_Visible = false; }
                if ((i + 7) <= totalDrums) { dv.D7 = (i + 7).ToString(); dv.D7_Visible = true; } else { dv.D7 = ""; dv.D7_Visible = false; }
                if ((i + 8) <= totalDrums) { dv.D8 = (i + 8).ToString(); dv.D8_Visible = true; } else { dv.D8 = ""; dv.D8_Visible = false; }
                if ((i + 9) <= totalDrums) { dv.D9 = (i + 9).ToString(); dv.D9_Visible = true; } else { dv.D9 = ""; dv.D9_Visible = false; }
                if ((i + 10) <= totalDrums) { dv.D10 = (i + 10).ToString(); dv.D10_Visible = true; } else { dv.D10 = ""; dv.D10_Visible = false; }
				dv_list.Add(dv);
                i += 9;
            }
			listview_drums.ItemsSource = dv_list;
			listview_drums.IsVisible = true;
        }

        void Btn_DrumSelection_Clicked(System.Object sender, System.EventArgs e)
        {
            var btn = (Button)sender;
            int selectedDrumNumber = 0;
            int.TryParse(btn.Text, out selectedDrumNumber);
            DisplayAlert("Alert!!!", "Selected drum for test is " + selectedDrumNumber.ToString(), "Ok");
            Navigation.PushAsync(new StrengthAnalyzer());
        }

        void ViewCell_Tapped(System.Object sender, System.EventArgs e)
        {
            if (lastCell != null)
                lastCell.View.BackgroundColor = Color.White;
            var viewCell = (ViewCell)sender;
            if (viewCell.View != null)
            {
                viewCell.View.BackgroundColor = Color.White;
                lastCell = viewCell;
            }
        }
    }
}

