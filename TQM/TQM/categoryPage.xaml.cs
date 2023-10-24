
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
    public partial class categoryPage : ContentPage
    {
        private Guid currentCategoryID = Guid.Empty;
        private ViewCell lastCell;
        public categoryPage()
        {
            InitializeComponent();
            //using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
            //{
            //    conn.DropTable<CategoryModel>();
            //}
        }

        private void categorySearchResultView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            try
            {
                var selectedItem = categorySearchResultView.SelectedItem as CategoryModel;
                currentCategoryID = selectedItem.ID;
                entry_category.Text = selectedItem.category;
                btn_save.Text = "Update";
                categorySearchResultView.ScrollTo(entry_category, ScrollToPosition.Start, true);
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
                string category = entry_category.Text.Trim();
                category = Regex.Replace(category, @"\s+", " ");
                if (category == "")
                {
                    DisplayAlert("Alert", "Please enter category to proceed!!!", "OK");
                    return;
                }
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    conn.CreateTable<CategoryModel>();
                    List<CategoryModel> categoryList = conn.Table<CategoryModel>().Where(
                        CategoryModel => CategoryModel.category.ToLower().Contains(category.ToLower())).ToList();
                    if (categoryList.Count > 0)
                    {
                        if (btn_save.Text.ToLower() == "save")
                        {
                            DisplayAlert("Notice", "Category already exist!!!", "OK");
                            return;
                        }
                        else
                        {
                            foreach (var cat in categoryList)
                            {
                                if (cat.ID != currentCategoryID)
                                {
                                    DisplayAlert("Notice", "Category already exist!!!", "OK");
                                    return;
                                }
                            }
                        }
                    }
                }
                CategoryModel cm = new CategoryModel()
                {
                    category = category,
                    createdate = DateTime.Now
                };

                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    conn.CreateTable<CategoryModel>();
                    int row = 0;
                    string msg = "saved";
                    if (btn_save.Text.ToLower() == "save")
                    {
                        cm.ID = Guid.NewGuid();
                        row = conn.Insert(cm);
                    }
                    else if (btn_save.Text.ToLower() == "update")
                    {
                        msg = "updated";
                        cm.ID = this.currentCategoryID;
                        row = conn.Update(cm);
                    }
                    if (row > 0)
                    {
                        DisplayAlert("Success", "Category " + msg + " successfully!!!", "OK");
                        reset();
                    }
                    else
                    {
                        DisplayAlert("Failure", "Category failed to be " + msg + "!!!", "OK");
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
            entry_category.Text = "";
            entry_categorySearch.Text = "";
            categorySearchResultView.ItemsSource = null;
            this.currentCategoryID = Guid.Empty;
        }

        private void btn_viewall_Clicked(object sender, EventArgs e)
        {
            try
            {
                entry_categorySearch.Text = "";
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    conn.CreateTable<CategoryModel>();
                    List<CategoryModel> categoryList = conn.Table<CategoryModel>().ToList();
                    if (categoryList.Count > 0)
                    {
                        categorySearchResultView.ItemsSource = categoryList;
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

        private void btn_categorySearch_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (entry_categorySearch.Text == null) { DisplayAlert("Notice", "Please enter category to search!!!", "OK"); return; }
                if (entry_categorySearch.Text.Trim() == "") { DisplayAlert("Notice", "Please enter category to search!!!", "OK"); return; }
                using (SQLiteConnection conn = new SQLiteConnection(App.DatabaseLocation))
                {
                    string categorySearched = entry_categorySearch.Text.Trim();
                    conn.CreateTable<CategoryModel>();
                    List<CategoryModel> categorylist = conn.Table<CategoryModel>().Where(CategoryModel => CategoryModel.category.ToLower().Contains(categorySearched.ToLower())).ToList();
                    if (categorylist.Count > 0)
                    {
                        categorySearchResultView.ItemsSource = categorylist;
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

        
    }

}