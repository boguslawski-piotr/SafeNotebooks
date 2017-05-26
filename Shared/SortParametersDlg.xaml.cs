using pbXForms;

namespace SafeNotebooks
{
    public partial class SortParametersDlg : ModalContentView
    {
        public ItemWithItems.SortParameters SortParams;

        public SortParametersDlg()
        {
			SortParams = new ItemWithItems.SortParameters();
			Initialize();
		}

        public SortParametersDlg(string title, ItemWithItems.SortParameters sortParams = null)
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

        void Cancel_Clicked(object sender, System.EventArgs e)
        {
            OnCancel();
        }

        public override void OnOK()
        {
            SortParams.Descending = Descending.IsToggled;
            base.OnOK();
        }

        void ByDate_Clicked(object sender, System.EventArgs e)
        {
            SortParams.Clear();
            SortParams.ByDate = true;
            OnOK();
        }

        void ByName_Clicked(object sender, System.EventArgs e)
        {
            SortParams.Clear();
            SortParams.ByName = true;
            OnOK();
        }

        void ByColor_Clicked(object sender, System.EventArgs e)
        {
            SortParams.Clear();
            SortParams.ByColor = true;
            OnOK();
        }
    }
}
