using System;
using System.Collections.Generic;
using SQLite;
using TQM.Model;
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
            int firstDrumNo = selectedDrumStartNo - 1;

            List<DrumMV> dv_list = new List<DrumMV>();
			for(int i = 0; i < totalDrums; i++)
			{
				DrumMV dv = new DrumMV();
				if ((firstDrumNo + 1) <= selectedDrumEndNo) { dv.D1 = (firstDrumNo + 1).ToString(); dv.D1_Visible = true; } else { dv.D1 = ""; dv.D1_Visible = false; }
                if ((firstDrumNo + 2) <= selectedDrumEndNo) { dv.D2 = (firstDrumNo + 2).ToString(); dv.D2_Visible = true; } else { dv.D2 = ""; dv.D2_Visible = false; }
                if ((firstDrumNo + 3) <= selectedDrumEndNo) { dv.D3 = (firstDrumNo + 3).ToString(); dv.D3_Visible = true; } else { dv.D3 = ""; dv.D3_Visible = false; }
                if ((firstDrumNo + 4) <= selectedDrumEndNo) { dv.D4 = (firstDrumNo + 4).ToString(); dv.D4_Visible = true; } else { dv.D4 = ""; dv.D4_Visible = false; }
                if ((firstDrumNo + 5) <= selectedDrumEndNo) { dv.D5 = (firstDrumNo + 5).ToString(); dv.D5_Visible = true; } else { dv.D5 = ""; dv.D5_Visible = false; }
                if ((firstDrumNo + 6) <= selectedDrumEndNo) { dv.D6 = (firstDrumNo + 6).ToString(); dv.D6_Visible = true; } else { dv.D6 = ""; dv.D6_Visible = false; }
                if ((firstDrumNo + 7) <= selectedDrumEndNo) { dv.D7 = (firstDrumNo + 7).ToString(); dv.D7_Visible = true; } else { dv.D7 = ""; dv.D7_Visible = false; }
                if ((firstDrumNo + 8) <= selectedDrumEndNo) { dv.D8 = (firstDrumNo + 8).ToString(); dv.D8_Visible = true; } else { dv.D8 = ""; dv.D8_Visible = false; }
                if ((firstDrumNo + 9) <= selectedDrumEndNo) { dv.D9 = (firstDrumNo + 9).ToString(); dv.D9_Visible = true; } else { dv.D9 = ""; dv.D9_Visible = false; }
                if ((firstDrumNo + 10) <= selectedDrumEndNo) { dv.D10 = (firstDrumNo + 10).ToString(); dv.D10_Visible = true; } else { dv.D10 = ""; dv.D10_Visible = false; }
				dv_list.Add(dv);
                i += 9;
                firstDrumNo += 10;
            }
			listview_drums.ItemsSource = dv_list;
			listview_drums.IsVisible = true;
        }

        private bool getTestDetailsForDrum(int drumNo)
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    StrengthTestSummaryModel sts = conn.Table<StrengthTestSummaryModel>().Where(StrengthTestSummaryModel =>
                                                        (StrengthTestSummaryModel.machineCategory == selectedMachineCategory
                                                        && StrengthTestSummaryModel.machineID == selectedMachineID
                                                        && StrengthTestSummaryModel.drumSelectionMethod == "Scheduled"
                                                        && StrengthTestSummaryModel.drumNumber==drumNo)).FirstOrDefault();
                    if (sts == null) { return false; } else { return true; }
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        void Btn_DrumSelection_Clicked(System.Object sender, System.EventArgs e)
        {
            var btn = (Button)sender;
            int selectedDrumNumber = 0;
            int.TryParse(btn.Text, out selectedDrumNumber);
            //DisplayAlert("Alert!!!", "Selected drum for test is " + selectedDrumNumber.ToString(), "Ok");
            Navigation.PushAsync(new StrengthAnalyzer(selectedMachineCategory,
                                                        selectedMachineID,
                                                        selectedMachineName,
                                                        selectedSectionNo,
                                                        selectedDrumNumber,
                                                        selectedDrumStartNo,
                                                        selectedDrumEndNo));
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

