using System.Threading.Tasks;
using pbXForms;

namespace SafeNotebooks
{
    public partial class SortParametersDlg : ModalContentView
    {
        public ItemWithItems.SortParameters SortParams;

        public SortParametersDlg()
        {
            Initialize();
        }

        public SortParametersDlg(string title, ItemWithItems.SortParameters sortParams)
        {
            SortParams = sortParams;
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

            SortParams.Clear();
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
            SortParams.ByDate = true;
            OnOK();
        }

        void ByName_Clicked(object sender, System.EventArgs e)
        {
            SortParams.ByName = true;
            OnOK();
        }

        void ByColor_Clicked(object sender, System.EventArgs e)
        {
            //Test();
            //return;

            SortParams.ByColor = true;
            OnOK();
        }

#if DEBUG
        TestView d1;
        async Task Test()
        {
            if (d1 == null)
                //d1 = new NotebooksView();
                d1 = new TestView();
            await MainWnd.Current.ModalManager.DisplayModalAsync(d1, ModalViewsManager.ModalPosition.BottomCenter);
        }
#endif
    }
}
