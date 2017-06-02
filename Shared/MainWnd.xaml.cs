using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using pbXForms;
using pbXNet;
using Xamarin.Forms;

namespace SafeNotebooks
{
    public partial class MainWnd : MasterDetailPageEx
    {
        public static MainWnd Current { get; set; }

        public MainWnd()
        {
            Current = this;

            InitializeComponent();
            InitializeViews();
        }
    }
}
