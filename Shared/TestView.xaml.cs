using System;
using System.Collections.Generic;
using pbXForms;
using Xamarin.Forms;

namespace SafeNotebooks
{
    public class TestModel : BindableObject
    {
        public static readonly BindableProperty ModelTestPropertyB = BindableProperty.Create("ModelTestProperty", typeof(string), typeof(TestModel), null);

        public static TestModel Current;

        public string ModelTestProperty {
            get => (string)GetValue(ModelTestPropertyB);
            set => SetValue(ModelTestPropertyB, value);
        }

        public TestModel()
        {
            ModelTestProperty = "empty constructor";
            Current = this;
        }

        public TestModel(string s)
        {
            ModelTestProperty = s;
            Current = this;
        }
    }

    public partial class TestView : ModalContentView
    {
        public static TestModel Model { get; } = new TestModel("constructor from TestView");

        public static string TestProperty => "static test property";

        public TestView()
        {
            InitializeComponent();
        }

        void Hide_Clicked(object sender, System.EventArgs e)
        {
            //Wnd.C.IsSplitView = true;
            //Wnd.C.ShowDetailView<PageView>("vPage", MastersDetailsPage.ViewsSwitchingAnimation.LeftToRight);
            //Wnd.C.ShowDetailViewAsync<PageView>(MastersDetailsPage.ViewsSwitchingAnimation.Back);
        }

        void MS_Clicked(object sender, System.EventArgs e)
        {
            //Wnd.C.ShowMasterViewAsync<NotebookView>(MastersDetailsPage.ViewsSwitchingAnimation.Back);
            //Wnd.C.MasterViewIsVisible = true;
        }

        void MH_Clicked(object sender, System.EventArgs e)
        {
            Model.ModelTestProperty = "changed...";
        }
    }
}
