using System.Threading.Tasks;
using pbXForms;
using Xamarin.Forms;

namespace SafeNotebooks
{
    //[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsWnd : pbXForms.ContentPageEx
    {
        public SettingsWnd()
        {
            InitializeComponent();
            LayoutAppBar();
        }

        void LayoutAppBar()
        {
            CaptionBar.Margin = new Thickness(BackBtn.IsVisible ? 0 : Metrics.ScreenEdgeMargin, 0, 0, 0);
        }

        void OKBtn_Clicked(object sender, System.EventArgs e)
        {
#if DEBUG
            Test();
#endif
        }

        void CancelBtn_Clicked(object sender, System.EventArgs e)
        {
            Navigation.PopModalAsync();
        }

        void BackBtn_Clicked(object sender, System.EventArgs e)
        {
            Navigation.PopModalAsync();
        }

#if DEBUG
        TestView d1;
        async Task Test()
        {
            if (d1 == null)
                //d1 = new NotebooksView();
                d1 = new TestView();
            await ModalManager.DisplayModalAsync(d1, ModalViewsManager.ModalPosition.BottomCenter);
        }
#endif

    }
}
