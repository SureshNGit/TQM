using System;
using System.Collections.Generic;
using SQLite;
using TQM.Model;
using TQM.ModelView;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Forms;

namespace TQM
{	
	public partial class DrumView : ContentPage
	{
        private Guid selectedCategoryID = Guid.Empty;
        private string selectedMachineCategory = null;
		private Guid selectedMachineID = Guid.Empty;
		private string selectedMachineName = null;
        private int selectedOverallDrumNos = 0;
        private int selectedOverallSections = 0;
		private int selectedSectionNo = 0;
		private int selectedDrumStartNo = 0;
		private int selectedDrumEndNo = 0;
        private ViewCell lastCell=null;
        private Dictionary<int, decimal> drumDict = null;
        private Dictionary<int, Boolean> drumDictRandomTest = null;

        public DrumView ()
		{
			InitializeComponent ();
		}

        public DrumView(Guid catID, string macCat, Guid macID, string macName,int overallDrums, int overallSections, int sectionNo, int startDrumNo, int endDrumNo)
        {
            InitializeComponent();
            drumDict = new Dictionary<int, decimal>();
            drumDictRandomTest = new Dictionary<int, Boolean>();
            selectedCategoryID = catID;
            selectedMachineCategory = macCat;
			selectedMachineID = macID;
			selectedMachineName = macName;
            selectedOverallDrumNos = overallDrums;
            selectedOverallSections = overallSections;
			selectedSectionNo = sectionNo;
			selectedDrumStartNo = startDrumNo;
			selectedDrumEndNo = endDrumNo;
			generateDrumView();
        }

		public void generateDrumView()
		{
            try
            {
                int totalDrums = (selectedDrumEndNo - selectedDrumStartNo) + 1;
                int firstDrumNo = selectedDrumStartNo - 1;

                List<DrumMV> dv_list = new List<DrumMV>();
                for (int i = 0; i < totalDrums; i++)
                {
                    DrumMV dv = new DrumMV();
                    if ((firstDrumNo + 1) <= selectedDrumEndNo) { dv.D1 = (firstDrumNo + 1).ToString(); if (getTestDetailsForDrum(firstDrumNo + 1)) { if (drumDictRandomTest[firstDrumNo + 1]) { dv.D1_BG_Color = "orange"; } else { dv.D1_BG_Color = "red"; }; dv.D1_Strength = drumDict[firstDrumNo + 1].ToString(); } else { if (drumDictRandomTest[firstDrumNo + 1]) { dv.D1_BG_Color = "orange"; } else { dv.D1_BG_Color = "green"; }; dv.D1_Strength = drumDict[firstDrumNo + 1].ToString(); }; dv.D1_Visible = true; } else { dv.D1 = ""; dv.D1_Visible = false; }
                    if ((firstDrumNo + 2) <= selectedDrumEndNo) { dv.D2 = (firstDrumNo + 2).ToString(); if (getTestDetailsForDrum(firstDrumNo + 2)) { if (drumDictRandomTest[firstDrumNo + 2]) { dv.D2_BG_Color = "orange"; } else { dv.D2_BG_Color = "red"; }; dv.D2_Strength = drumDict[firstDrumNo + 2].ToString(); } else { if (drumDictRandomTest[firstDrumNo + 2]) { dv.D2_BG_Color = "orange"; } else { dv.D2_BG_Color = "green"; }; dv.D2_Strength = drumDict[firstDrumNo + 2].ToString(); }; dv.D2_Visible = true; } else { dv.D2 = ""; dv.D2_Visible = false; }
                    if ((firstDrumNo + 3) <= selectedDrumEndNo) { dv.D3 = (firstDrumNo + 3).ToString(); if (getTestDetailsForDrum(firstDrumNo + 3)) { if (drumDictRandomTest[firstDrumNo + 3]) { dv.D3_BG_Color = "orange"; } else { dv.D3_BG_Color = "red"; }; dv.D3_Strength = drumDict[firstDrumNo + 3].ToString(); } else { if (drumDictRandomTest[firstDrumNo + 3]) { dv.D3_BG_Color = "orange"; } else { dv.D3_BG_Color = "green"; }; dv.D3_Strength = drumDict[firstDrumNo + 3].ToString(); }; dv.D3_Visible = true; } else { dv.D3 = ""; dv.D3_Visible = false; }
                    if ((firstDrumNo + 4) <= selectedDrumEndNo) { dv.D4 = (firstDrumNo + 4).ToString(); if (getTestDetailsForDrum(firstDrumNo + 4)) { if (drumDictRandomTest[firstDrumNo + 4]) { dv.D4_BG_Color = "orange"; } else { dv.D4_BG_Color = "red"; }; dv.D4_Strength = drumDict[firstDrumNo + 4].ToString(); } else { if (drumDictRandomTest[firstDrumNo + 4]) { dv.D4_BG_Color = "orange"; } else { dv.D4_BG_Color = "green"; }; dv.D4_Strength = drumDict[firstDrumNo + 4].ToString(); }; dv.D4_Visible = true; } else { dv.D4 = ""; dv.D4_Visible = false; }
                    if ((firstDrumNo + 5) <= selectedDrumEndNo) { dv.D5 = (firstDrumNo + 5).ToString(); if (getTestDetailsForDrum(firstDrumNo + 5)) { if (drumDictRandomTest[firstDrumNo + 5]) { dv.D5_BG_Color = "orange"; } else { dv.D5_BG_Color = "red"; }; dv.D5_Strength = drumDict[firstDrumNo + 5].ToString(); } else { if (drumDictRandomTest[firstDrumNo + 5]) { dv.D5_BG_Color = "orange"; } else { dv.D5_BG_Color = "green"; }; dv.D5_Strength = drumDict[firstDrumNo + 5].ToString(); }; dv.D5_Visible = true; } else { dv.D5 = ""; dv.D5_Visible = false; }
                    if ((firstDrumNo + 6) <= selectedDrumEndNo) { dv.D6 = (firstDrumNo + 6).ToString(); if (getTestDetailsForDrum(firstDrumNo + 6)) { if (drumDictRandomTest[firstDrumNo + 6]) { dv.D6_BG_Color = "orange"; } else { dv.D6_BG_Color = "red"; }; dv.D6_Strength = drumDict[firstDrumNo + 6].ToString(); } else { if (drumDictRandomTest[firstDrumNo + 6]) { dv.D6_BG_Color = "orange"; } else { dv.D6_BG_Color = "green"; }; dv.D6_Strength = drumDict[firstDrumNo + 6].ToString(); }; dv.D6_Visible = true; } else { dv.D6 = ""; dv.D6_Visible = false; }
                    if ((firstDrumNo + 7) <= selectedDrumEndNo) { dv.D7 = (firstDrumNo + 7).ToString(); if (getTestDetailsForDrum(firstDrumNo + 7)) { if (drumDictRandomTest[firstDrumNo + 7]) { dv.D7_BG_Color = "orange"; } else { dv.D7_BG_Color = "red"; }; dv.D7_Strength = drumDict[firstDrumNo + 7].ToString(); } else { if (drumDictRandomTest[firstDrumNo + 7]) { dv.D7_BG_Color = "orange"; } else { dv.D7_BG_Color = "green"; }; dv.D7_Strength = drumDict[firstDrumNo + 7].ToString(); }; dv.D7_Visible = true; } else { dv.D7 = ""; dv.D7_Visible = false; }
                    if ((firstDrumNo + 8) <= selectedDrumEndNo) { dv.D8 = (firstDrumNo + 8).ToString(); if (getTestDetailsForDrum(firstDrumNo + 8)) { if (drumDictRandomTest[firstDrumNo + 8]) { dv.D8_BG_Color = "orange"; } else { dv.D8_BG_Color = "red"; }; dv.D8_Strength = drumDict[firstDrumNo + 8].ToString(); } else { if (drumDictRandomTest[firstDrumNo + 8]) { dv.D8_BG_Color = "orange"; } else { dv.D8_BG_Color = "green"; }; dv.D8_Strength = drumDict[firstDrumNo + 8].ToString(); }; dv.D8_Visible = true; } else { dv.D8 = ""; dv.D8_Visible = false; }
                    if ((firstDrumNo + 9) <= selectedDrumEndNo) { dv.D9 = (firstDrumNo + 9).ToString(); if (getTestDetailsForDrum(firstDrumNo + 9)) { if (drumDictRandomTest[firstDrumNo + 9]) { dv.D9_BG_Color = "orange"; } else { dv.D9_BG_Color = "red"; }; dv.D9_Strength = drumDict[firstDrumNo + 9].ToString(); } else { if (drumDictRandomTest[firstDrumNo + 9]) { dv.D9_BG_Color = "orange"; } else { dv.D9_BG_Color = "green"; }; dv.D9_Strength = drumDict[firstDrumNo + 9].ToString(); }; dv.D9_Visible = true; } else { dv.D9 = ""; dv.D9_Visible = false; }
                    if ((firstDrumNo + 10) <= selectedDrumEndNo) { dv.D10 = (firstDrumNo + 10).ToString(); if (getTestDetailsForDrum(firstDrumNo + 10)) { if (drumDictRandomTest[firstDrumNo + 10]) { dv.D10_BG_Color = "orange"; } else { dv.D10_BG_Color = "red"; }; dv.D10_Strength = drumDict[firstDrumNo + 10].ToString(); } else { if (drumDictRandomTest[firstDrumNo + 10]) { dv.D10_BG_Color = "orange"; } else { dv.D10_BG_Color = "green"; }; dv.D10_Strength = drumDict[firstDrumNo + 10].ToString(); }; dv.D10_Visible = true; } else { dv.D10 = ""; dv.D10_Visible = false; }
                    dv_list.Add(dv);
                    i += 9;
                    firstDrumNo += 10;
                }
                listview_drums.ItemsSource = dv_list;
                listview_drums.IsVisible = true;
            }
            catch (Exception ex)
            {
                DisplayAlert("Attention", "An error occurred.Error: " + ex.ToString(), "OK");
                return;
            }
        }

        private bool getTestDetailsForDrum(int drumNo)
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    bool ret = false;
                    StrengthTestSummaryModel sts = conn.Table<StrengthTestSummaryModel>().Where(StrengthTestSummaryModel =>
                                                        (StrengthTestSummaryModel.categoryID == selectedCategoryID
                                                        && StrengthTestSummaryModel.machineID == selectedMachineID
                                                        && StrengthTestSummaryModel.sectionNumber == selectedSectionNo
                                                        && StrengthTestSummaryModel.drumSelectionMethod == "Scheduled"
                                                        && StrengthTestSummaryModel.drumNumber==drumNo)).FirstOrDefault();
                    StrengthTestModel stm = conn.Table<StrengthTestModel>().Where(StrengthTestModel =>
                                                        (StrengthTestModel.categoryID == selectedCategoryID
                                                        && StrengthTestModel.machineID == selectedMachineID
                                                        && StrengthTestModel.sectionNumber == selectedSectionNo
                                                        && StrengthTestModel.drumNumber == drumNo))
                                                        .OrderByDescending(StrengthTestModel=>StrengthTestModel.createdate).FirstOrDefault();
                    if (sts == null)
                    {
                        drumDict.Add(drumNo, 0.0m);
                        ret= true;
                    }
                    else
                    {
                        drumDict.Add(drumNo, sts.yarnStrength);
                        ret= false;
                    }

                    if (stm != null && stm.totalTestCount != stm.sampleNo)
                    {
                        drumDictRandomTest.Add(drumNo, true);
                    }
                    else
                    {
                        drumDictRandomTest.Add(drumNo, false);
                    }
                    return ret;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

       async void Btn_DrumSelection_Clicked(System.Object sender, System.EventArgs e)
        {
            try
            {
                var btn = (Button)sender;
                int selectedDrumNumber = 0;
                int.TryParse(btn.Text.Split(new string[] { "\nST" }, StringSplitOptions.None)[0], out selectedDrumNumber);

                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {

                    ConfigModel cm = conn.Table<ConfigModel>().Where(ConfigModel =>
                                    (ConfigModel.machineID == selectedMachineID)).FirstOrDefault();

                    if (cm == null)
                    {
                        await DisplayAlert("Attention", "Unable to read machine settings!!! Please try again", "OK");
                        return;
                    }

                    if (DateTime.Now.Date > cm.scheduledEndDate)
                    {
                        await DisplayAlert("Attention", "The scheduled date is expired for the selected drum [" +
                            selectedDrumNumber.ToString() + "]. Please reach admin to change the machine settings", "OK");
                        return;
                    }

                    if (btn.BackgroundColor.ToHex() == "#FF008000")
                    {


                        bool userDecision = await DisplayAlert("Attention",
                                                       "Test already completed for Drum Number ["
                                                       + selectedDrumNumber.ToString() + "]"
                                                       + ". Still do you want to conduct test in Random method?",
                                                       "Yes",
                                                       "No");
                        if (userDecision)
                        {
                            //using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                            //{
                                UserModel loggedInUser = conn.Table<UserModel>().Where(UserModel => UserModel.isloggedIn == true).FirstOrDefault();
                                if (loggedInUser == null)
                                {
                                    await DisplayAlert("Attention", "Unable to get logged user information!!!", "OK");
                                    return;
                                }
                                else
                                {
                                    if (!loggedInUser.isAdmin)
                                    {
                                        var result = await Navigation.ShowPopupAsync(new AdminCredPopUp());
                                        if (result != null)
                                        {
                                            if (result.ToString() == "Success")
                                            {
                                                _ = Navigation.PushAsync(new StrengthAnalyzer(selectedCategoryID,
                                                                  selectedMachineCategory,
                                                                  selectedMachineID,
                                                                  selectedMachineName,
                                                                  selectedOverallDrumNos,
                                                                  selectedOverallSections,
                                                                  selectedSectionNo,
                                                                  selectedDrumNumber,
                                                                  selectedDrumStartNo,
                                                                  selectedDrumEndNo,
                                                                  true));
                                            }
                                            else
                                            {
                                                await DisplayAlert("Attention", result.ToString(), "OK");
                                                return;
                                            }
                                        }
                                        else
                                        {
                                            await DisplayAlert("Attention", "Invalid Admin Credentials. Please try again!!!", "OK");
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        _ = Navigation.PushAsync(new StrengthAnalyzer(selectedCategoryID,
                                                                  selectedMachineCategory,
                                                                  selectedMachineID,
                                                                  selectedMachineName,
                                                                  selectedOverallDrumNos,
                                                                  selectedOverallSections,
                                                                  selectedSectionNo,
                                                                  selectedDrumNumber,
                                                                  selectedDrumStartNo,
                                                                  selectedDrumEndNo,
                                                                  true));
                                    }
                                }
                            //}
                        }
                    }
                    else
                    {

                        _ = Navigation.PushAsync(new StrengthAnalyzer(selectedCategoryID,
                                                                    selectedMachineCategory,
                                                                    selectedMachineID,
                                                                    selectedMachineName,
                                                                    selectedOverallDrumNos,
                                                                    selectedOverallSections,
                                                                    selectedSectionNo,
                                                                    selectedDrumNumber,
                                                                    selectedDrumStartNo,
                                                                    selectedDrumEndNo,
                                                                    false));
                    }
                }
            }
            catch(Exception ex)
            {
                await DisplayAlert("Attention", "An error occurred.Error: "+ex.ToString(), "OK");
                return;
            }
        }

        void ViewCell_Tapped(System.Object sender, System.EventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                DisplayAlert("Attention", "An error occurred.Error: " + ex.ToString(), "OK");
                return;
            }
        }

        void btn_backToHome_Clicked(System.Object sender, System.EventArgs e)
        {
            Navigation.PushAsync(new TestHome(selectedMachineCategory,selectedMachineID,selectedMachineName));
        }
    }
}

