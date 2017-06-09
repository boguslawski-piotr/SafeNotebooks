using pbXForms;

namespace SafeNotebooks
{
	public partial class MainWnd : MasterDetailPageEx
    {
        public static MainWnd Current { get; set; }
		public static MainWnd C => Current;

		public MainWnd()
        {
            Current = this;

            InitializeComponent();
            InitializeViews();
        }
    }

	public static class Wnd
	{
		public static MainWnd C => MainWnd.C;
	}
}
