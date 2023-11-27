using System;
using System.Collections.Generic;
using SQLite;
using TQM.Model;
using Xamarin.Forms;

namespace TQM
{	
	public partial class TestHome : ContentPage
	{
        private Guid selectedCategoryID = Guid.Empty;
        private string selectedCategory = null;
        private Guid selectedMachineID = Guid.Empty;
        private string selectedMachineName = null;
        private int selectedOverallDrumNos = 0;
        private int selectedOverallSections = 0;
        private int sec1_lowerLimit = 0;
        private int sec1_upperLimit = 0;
        private int sec2_lowerLimit = 0;
        private int sec2_upperLimit = 0;
        private int sec3_lowerLimit = 0;
        private int sec3_upperLimit = 0;
        private int sec4_lowerLimit = 0;
        private int sec4_upperLimit = 0;
        public TestHome ()
		{
			InitializeComponent ();
            using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            {
                //conn.DropTable<StrengthTestModel>();
                //conn.DropTable<StrengthTestSummaryModel>();
                //conn.DropTable<TestConfigModel>();

                conn.CreateTable<StrengthTestModel>();
                conn.CreateTable<StrengthTestSummaryModel>();
                conn.CreateTable<TestConfigModel>();

                conn.CreateTable<CategoryModel>();
                conn.CreateTable<MachineModel>();

                List<CategoryModel> cm = conn.Table<CategoryModel>().ToList();
                picker_machinecategory.ItemsSource = cm;
            }
        }


        public TestHome(string macCat, Guid macID, string macName)
        {
            try
            {
                InitializeComponent();
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    //conn.DropTable<StrengthTestModel>();
                    //conn.DropTable<StrengthTestSummaryModel>();
                    //conn.DropTable<TestConfigModel>();

                    conn.CreateTable<StrengthTestModel>();
                    conn.CreateTable<StrengthTestSummaryModel>();
                    conn.CreateTable<TestConfigModel>();

                    conn.CreateTable<CategoryModel>();
                    conn.CreateTable<MachineModel>();

                    List<CategoryModel> cm = conn.Table<CategoryModel>().ToList();
                    picker_machinecategory.ItemsSource = cm;
                }

                IList<string> machineCategorylist = picker_machinecategory.Items;
                int machineCatindex = 0;
                foreach (string mCat in machineCategorylist)
                {
                    if (mCat != macCat)
                    {
                        machineCatindex++;
                    }
                    else
                    {
                        break;
                    }
                }
                picker_machinecategory.SelectedIndex = machineCatindex;

                

                IList<string> machinelist = picker_machinename.Items;
                int machineindex = 0;
                foreach (string m in machinelist)
                {
                    if (m != macName)
                    {
                        machineindex++;
                    }
                    else
                    {
                        break;
                    }
                }
                picker_machinename.SelectedIndex = machineindex;
            }
            catch (Exception ex)
            {
                DisplayAlert("Notice-Home", ex.Message.ToString(), "Ok");
            }
        }

        private void picker_machinename_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                btn_section1.IsVisible = false;
                lbl_section1.IsVisible = false;
                lbl_section1.Text = "";
                btn_section2.IsVisible = false;
                lbl_section2.IsVisible = false;
                lbl_section2.Text = "";
                btn_section3.IsVisible = false;
                lbl_section3.IsVisible = false;
                lbl_section3.Text = "";
                btn_section4.IsVisible = false;
                lbl_section4.IsVisible = false;
                lbl_section4.Text = "";

                List<MachineModel> source = (List<MachineModel>)picker_machinename.ItemsSource;
                if (picker_machinename.SelectedIndex < 0)
                {
                    return;
                }
                selectedMachineID = (Guid)source[picker_machinename.SelectedIndex].ID;
                MachineModel selectedMachine = (MachineModel)picker_machinename.SelectedItem;
                selectedMachineName = selectedMachine.machineName;
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    conn.CreateTable<ConfigModel>();
                    ConfigModel yarncountconfigmodel = conn.Table<ConfigModel>().Where(ConfigModel =>
                                                                (ConfigModel.categoryID == selectedCategoryID &&
                                                                ConfigModel.machineID == selectedMachineID &&
                                                                ConfigModel.machineName == selectedMachineName)).FirstOrDefault();
                    if (yarncountconfigmodel != null)
                    {
                        selectedOverallDrumNos = yarncountconfigmodel.totalDrumCount;
                        selectedOverallSections = yarncountconfigmodel.totalSections;
                        if (yarncountconfigmodel.drumNumbers_s1 != null
                            && yarncountconfigmodel.drumNumbers_s1.Trim() != ""
                            && yarncountconfigmodel.drumNumbers_s1.Trim() != "0.0")
                        {
                            btn_section1.IsVisible = true;
                            sec1_lowerLimit = int.Parse(yarncountconfigmodel.drumNumbers_s1.ToString().Split('.')[0]);
                            sec1_upperLimit = int.Parse(yarncountconfigmodel.drumNumbers_s1.ToString().Split('.')[1]);
                            lbl_section1.Text = "Drums (" + sec1_lowerLimit.ToString() + " to " + sec1_upperLimit.ToString() + ")";
                            lbl_section1.IsVisible = true;
                            if (yarncountconfigmodel.drumNumbers_s2 != null
                                && yarncountconfigmodel.drumNumbers_s2.Trim() != ""
                                && yarncountconfigmodel.drumNumbers_s2.Trim() != "0.0")
                            {
                                btn_section2.IsVisible = true;
                                sec2_lowerLimit = int.Parse(yarncountconfigmodel.drumNumbers_s2.ToString().Split('.')[0]);
                                sec2_upperLimit = int.Parse(yarncountconfigmodel.drumNumbers_s2.ToString().Split('.')[1]);
                                lbl_section2.Text = "Drums (" + sec2_lowerLimit.ToString() + " to " + sec2_upperLimit.ToString() + ")";
                                lbl_section2.IsVisible = true;
                                if (yarncountconfigmodel.drumNumbers_s3 != null
                                    && yarncountconfigmodel.drumNumbers_s3.Trim() != ""
                                    && yarncountconfigmodel.drumNumbers_s3.Trim() != "0.0")
                                {
                                    btn_section3.IsVisible = true;
                                    sec3_lowerLimit = int.Parse(yarncountconfigmodel.drumNumbers_s3.ToString().Split('.')[0]);
                                    sec3_upperLimit = int.Parse(yarncountconfigmodel.drumNumbers_s3.ToString().Split('.')[1]);
                                    lbl_section3.Text = "Drums (" + sec3_lowerLimit.ToString() + " to " + sec3_upperLimit.ToString() + ")";
                                    lbl_section3.IsVisible = true;
                                    if (yarncountconfigmodel.drumNumbers_s4 != null
                                        && yarncountconfigmodel.drumNumbers_s4.Trim() != ""
                                        && yarncountconfigmodel.drumNumbers_s1.Trim() != "0.0")
                                    {
                                        btn_section4.IsVisible = true;
                                        sec4_lowerLimit = int.Parse(yarncountconfigmodel.drumNumbers_s4.ToString().Split('.')[0]);
                                        sec4_upperLimit = int.Parse(yarncountconfigmodel.drumNumbers_s4.ToString().Split('.')[1]);
                                        lbl_section4.Text = "Drums (" + sec4_lowerLimit.ToString() + " to " + sec4_upperLimit.ToString() + ")";
                                        lbl_section4.IsVisible = true;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        btn_section1.IsVisible = false;
                        lbl_section1.IsVisible = false;
                        lbl_section1.Text = "";
                        btn_section2.IsVisible = false;
                        lbl_section2.IsVisible = false;
                        lbl_section2.Text = "";
                        btn_section3.IsVisible = false;
                        lbl_section3.IsVisible = false;
                        lbl_section3.Text = "";
                        btn_section4.IsVisible = false;
                        lbl_section4.IsVisible = false;
                        lbl_section4.Text = "";
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Notice-Home", ex.Message.ToString(), "Ok");
            }
        }

        private void picker_machinecategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                btn_section1.IsVisible = false;
                lbl_section1.IsVisible = false;
                lbl_section1.Text = "";
                btn_section2.IsVisible = false;
                lbl_section2.IsVisible = false;
                lbl_section2.Text = "";
                btn_section3.IsVisible = false;
                lbl_section3.IsVisible = false;
                lbl_section3.Text = "";
                btn_section4.IsVisible = false;
                lbl_section4.IsVisible = false;
                lbl_section4.Text = "";
                if (picker_machinecategory.SelectedItem == null)
                {
                    selectedCategoryID = Guid.Empty;
                    selectedCategory = null;
                    selectedMachineID = Guid.Empty;
                    selectedMachineName = null;
                    return;
                }
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {

                    List<CategoryModel> source = (List<CategoryModel>)picker_machinecategory.ItemsSource;
                    if (picker_machinecategory.SelectedIndex < 0)
                    {
                        selectedCategoryID = Guid.Empty;
                        selectedCategory = null;
                        selectedMachineID = Guid.Empty;
                        selectedMachineName = null;
                        return;
                    }
                    selectedCategoryID = (Guid)source[picker_machinecategory.SelectedIndex].ID;
                    CategoryModel selectedMachine = (CategoryModel)picker_machinecategory.SelectedItem;
                    selectedCategory = selectedMachine.category;

                    List<MachineModel> machines = conn.Table<MachineModel>().Where(
                        MachineModel => MachineModel.categoryID == selectedCategoryID).ToList();
                    picker_machinename.ItemsSource = machines;
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Notice-Home", ex.Message.ToString(), "Ok");
            }
        }

        void btn_section1_Clicked(System.Object sender, System.EventArgs e)
        {
            Navigation.PushAsync(new DrumView(selectedCategoryID,
                                                selectedCategory,
                                                selectedMachineID,
                                                selectedMachineName,
                                                selectedOverallDrumNos,
                                                selectedOverallSections,
                                                1,
                                                sec1_lowerLimit,
                                                sec1_upperLimit));
        }

        void btn_section2_Clicked(System.Object sender, System.EventArgs e)
        {
            Navigation.PushAsync(new DrumView(selectedCategoryID,
                                                selectedCategory,
                                                selectedMachineID,
                                                selectedMachineName,
                                                selectedOverallDrumNos,
                                                selectedOverallSections,
                                                2,
                                                sec2_lowerLimit,
                                                sec2_upperLimit));
        }

        void btn_section3_Clicked(System.Object sender, System.EventArgs e)
        {
            Navigation.PushAsync(new DrumView(selectedCategoryID,
                                                selectedCategory,
                                                selectedMachineID,
                                                selectedMachineName,
                                                selectedOverallDrumNos,
                                                selectedOverallSections,
                                                3,
                                                sec3_lowerLimit,
                                                sec3_upperLimit));
        }

        void btn_section4_Clicked(System.Object sender, System.EventArgs e)
        {
            Navigation.PushAsync(new DrumView(selectedCategoryID,
                                                selectedCategory,
                                                selectedMachineID,
                                                selectedMachineName,
                                                selectedOverallDrumNos,
                                                selectedOverallSections,
                                                4,
                                                sec4_lowerLimit,
                                                sec4_upperLimit));
        }
    }
}

