using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace SafeNotebooks
{
    public partial class ChooseSortDlg : ContentView
    {
        public ItemWithItems.SortParameters SortParams;

        public ChooseSortDlg()
        {
			SortParams = new ItemWithItems.SortParameters();
			Initialize();
		}

        public ChooseSortDlg(string title, ItemWithItems.SortParameters sortParams = null)
        {
            SortParams = sortParams ?? new ItemWithItems.SortParameters();

            Initialize();

            Title.Text = title;
        }

        void Initialize()
        {
            InitializeComponent();

            string selImg = "ic_done.png";
            if (SortParams.ByName) ByNameBtn.Image = selImg;
			if (SortParams.ByDate) ByDateBtn.Image = selImg;
			if (SortParams.ByColor) ByColorBtn.Image = selImg;
			if (SortParams.Descending) Descending.IsToggled = true;
        }

        public event EventHandler OK;
        public event EventHandler Cancel;

        void Cancel_Clicked(object sender, System.EventArgs e)
        {
            Cancel?.Invoke(this, null);
        }

        void _OK()
        {
            SortParams.Descending = Descending.IsToggled;
            OK?.Invoke(this, null);
        }

        void ByDate_Clicked(object sender, System.EventArgs e)
        {
            SortParams.Clear();
            SortParams.ByDate = true;
            _OK();
        }

        void ByName_Clicked(object sender, System.EventArgs e)
        {
            SortParams.Clear();
            SortParams.ByName = true;
            _OK();
        }

        void ByColor_Clicked(object sender, System.EventArgs e)
        {
            SortParams.Clear();
            SortParams.ByColor = true;
            _OK();
        }
    }
}
