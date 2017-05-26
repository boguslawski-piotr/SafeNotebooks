using System;
using System.Collections.Generic;
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

        //

        public bool NotebooksViewIsVisible
        {
            get {
                return MasterViewIsVisible;
            }
            set {
                MasterViewIsVisible = value;
            }
        }
    }
}
