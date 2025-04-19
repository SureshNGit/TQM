
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TQM
{
    using SQLite;
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using TQM.Model;

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class machinePage : ContentPage
    {
        private Guid selectedCategoryID = Guid.Empty;
        private string selectedCategory = null;
        private Guid currentMachineID = Guid.Empty;
        private ViewCell lastCell;
        public machinePage()
        {
            InitializeComponent();
            using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            {
                //conn.DropTable<CategoryModel>();
                //conn.DropTable<MachineModel>();

                conn.CreateTable<CategoryModel>();
                conn.CreateTable<MachineModel>();

                List<CategoryModel> cm = conn.Table<CategoryModel>().ToList();
                picker_machinecategory.ItemsSource = cm;
            }
        }

        private void machineSearchResultView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            try
            {
                var selectedItem = machineSearchResultView.SelectedItem as MachineModel;
                currentMachineID = selectedItem.ID;
                IList<string> machinecategorylist = picker_machinecategory.Items;
                int machinecategoryindex = 0;
                foreach (string machinecategory in machinecategorylist)
                {
                    if (machinecategory != selectedItem.machineCategory)
                    {
                        machinecategoryindex++;
                    }
                    else
                    {
                        break;
                    }
                }
                picker_machinecategory.SelectedIndex = machinecategoryindex;
                entry_machinename.Text = selectedItem.machineName;
                entry_macSerialNo.Text = selectedItem.macSerialNo;
                btn_save.Text = "Update";
                machineSearchResultView.ScrollTo(entry_machinename, ScrollToPosition.Start, true);
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", "Error Occurred!!! " + ex.Message.ToString(), "OK");
            }
        }

        private void btn_save_Clicked(object sender, EventArgs e)
        {
            try
            {
                string machinename = entry_machinename.Text.Trim();
                machinename = Regex.Replace(machinename, @"\s+", " ");
                if (machinename == "")
                {
                    DisplayAlert("Alert", "Please enter machine name to proceed!!!", "OK");
                    return;
                }
                string macSerialNo = entry_macSerialNo.Text.Trim();
                macSerialNo = Regex.Replace(macSerialNo, @"\s+", " ");
                if (macSerialNo == "")
                {
                    DisplayAlert("Alert", "Please enter machine serial number to proceed!!!", "OK");
                    return;
                }
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    conn.CreateTable<MachineModel>();
                    //List<MachineModel> machinelist = conn.Table<MachineModel>().Where(
                    //    MachineModel => MachineModel.machineName.ToLower().Contains(machinename.ToLower())).ToList();

                    List<MachineModel> machinelist = conn.Table<MachineModel>().Where(
                                                        m => m.machineName.ToLower().Contains(machinename.ToLower()) ||
                                                             m.macSerialNo.ToLower().Contains(machinename.ToLower())
                                                    ).ToList();

                    if (machinelist.Count > 0)
                    {
                        if (btn_save.Text.ToLower() == "save")
                        {
                            DisplayAlert("Notice", "Machine already exist!!!", "OK");
                            return;
                        }
                        else
                        {
                            foreach (var machine in machinelist)
                            {
                                if (machine.ID != currentMachineID)
                                {
                                    DisplayAlert("Notice", "Machine already exist!!!", "OK");
                                    return;
                                }
                            }
                        }
                    }
                }
                MachineModel machinemodel = new MachineModel()
                {
                    categoryID = selectedCategoryID,
                    machineCategory = selectedCategory,
                    machineName = machinename,
                    macSerialNo = macSerialNo,
                    createdate = DateTime.Now
                };

                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    conn.CreateTable<MachineModel>();
                    int row = 0;
                    string msg = "saved";
                    if (btn_save.Text.ToLower() == "save")
                    {
                        machinemodel.ID = Guid.NewGuid();
                        row = conn.Insert(machinemodel);
                    }
                    else if (btn_save.Text.ToLower() == "update")
                    {
                        msg = "updated";
                        machinemodel.ID = this.currentMachineID;
                        row = conn.Update(machinemodel);
                    }
                    if (row > 0)
                    {
                        DisplayAlert("Success", "Machine " + msg + " successfully!!!", "OK");
                        reset();
                    }
                    else
                    {
                        DisplayAlert("Failure", "Machine failed to be " + msg + "!!!", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", "Error Occurred!!! " + ex.Message.ToString(), "OK");
            }
        }

        private void reset()
        {
            lbl_searchresultheader.IsVisible = false;
            btn_save.Text = "Save";
            picker_machinecategory.SelectedItem = "";
            entry_machinename.Text = "";
            entry_macSerialNo.Text = "";
            entry_machinesearch.Text = "";
            machineSearchResultView.ItemsSource = null;
            this.currentMachineID = Guid.Empty;
            this.selectedCategoryID = Guid.Empty;
            this.selectedCategory = null;
        }

        private void btn_viewall_Clicked(object sender, EventArgs e)
        {
            try
            {
                entry_machinesearch.Text = "";
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    conn.CreateTable<MachineModel>();
                    List<MachineModel> machinelist = conn.Table<MachineModel>().ToList();
                    if (machinelist.Count > 0)
                    {
                        machineSearchResultView.ItemsSource = machinelist;
                        lbl_searchresultheader.IsVisible = true;
                    }
                    else
                    {
                        DisplayAlert("Notice", "No data avaialble to display!!!", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", "Erro Occurred!!! " + ex.Message.ToString(), "OK");
            }
        }

        private void btn_searchmachine_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (entry_machinesearch.Text == null) { DisplayAlert("Notice", "Please enter machine name to search!!!", "OK"); return; }
                if (entry_machinesearch.Text.Trim() == "") { DisplayAlert("Notice", "Please enter machine name to search!!!", "OK"); return; }
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    string machinesearched = entry_machinesearch.Text.Trim();
                    conn.CreateTable<MachineModel>();
                    //List<MachineModel> machinelist = conn.Table<MachineModel>().Where(MachineModel => MachineModel.machineName.ToLower().Contains(machinesearched.ToLower())).ToList();
                    List<MachineModel> machinelist = conn.Table<MachineModel>().Where(
                                                    MachineModel => MachineModel.machineName.ToLower().Contains(machinesearched.ToLower()) ||
                                                                    MachineModel.macSerialNo.ToLower().Contains(machinesearched.ToLower())
                                                ).ToList();

                    if (machinelist.Count > 0)
                    {
                        machineSearchResultView.ItemsSource = machinelist;
                        lbl_searchresultheader.IsVisible = true;
                    }
                    else
                    {
                        DisplayAlert("Notice", "No data avaialble to display!!!", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", "Erro Occurred!!! " + ex.Message.ToString(), "OK");
            }
        }

        private void btn_clear_Clicked(object sender, EventArgs e)
        {
            reset();
        }

        private void ViewCell_Tapped(object sender, EventArgs e)
        {
            if (lastCell != null)
                lastCell.View.BackgroundColor = Color.Transparent;
            var viewCell = (ViewCell)sender;
            if (viewCell.View != null)
            {
                viewCell.View.BackgroundColor = Color.FromHex("#FCF3CF");
                lastCell = viewCell;
            }
        }

        void picker_machinecategory_SelectedIndexChanged(System.Object sender, System.EventArgs e)
        {
            try
            {
                List<CategoryModel> source = (List<CategoryModel>)picker_machinecategory.ItemsSource;
                if (picker_machinecategory.SelectedIndex < 0)
                {
                    return;
                }
                selectedCategoryID = (Guid)source[picker_machinecategory.SelectedIndex].ID;
                CategoryModel selectedMachine = (CategoryModel)picker_machinecategory.SelectedItem;
                selectedCategory = selectedMachine.category;
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", "Erro Occurred!!! " + ex.Message.ToString(), "OK");
            }
        }
    }

}