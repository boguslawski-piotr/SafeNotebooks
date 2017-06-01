using System;
using System.Collections;
using System.Collections.Generic;
using pbXForms;
using pbXNet;
using Xamarin.Forms;

namespace SafeNotebooks
{
    public abstract partial class BaseView : ContentViewEx
    {
        public BaseView()
        {
            InitializeComponent();
        }


		//

		public void SelectUnselectItem_Clicked(object sender, System.EventArgs e)
		{
			if ((sender as ImageEx)?.CommandParameter is Item item)
				item.IsSelected = !item.IsSelected;
		}

		public void EditItem_Clicked(object sender, System.EventArgs e)
		{
			throw new NotImplementedException();
		}

		public void MoveItem_Clicked(object sender, System.EventArgs e)
		{
			throw new NotImplementedException();
		}

		public async void DeleteItem_Clicked(object sender, System.EventArgs e)
		{
			if ((sender as MenuItem)?.CommandParameter is Item item)
				await item.DeleteAsync();
		}
		

		//

		Layout<View> _SearchBar;
        Entry _SearchQuery;
        PIButton _CancelSearchBtn;

        protected void InitializeSearchBarFor(ListView ListCtl)
        {
            _SearchBar = ListCtl.HeaderElement as Layout<View>;
            _SearchQuery = _SearchBar.Children[0] as Entry;
            _CancelSearchBtn = _SearchBar.Children[1] as PIButton;

            if (_SearchBar == null || _SearchQuery == null || _CancelSearchBtn == null)
                throw new Exception("BaseView: SearchBar is NOT constructed correctly.");
            
            _SearchBar.BackgroundColor = ListCtl.BackgroundColor;
            _SearchQuery.BackgroundColor = ListCtl.BackgroundColor;
        }

        public void SearchQuery_Focused(object sender, Xamarin.Forms.FocusEventArgs e)
        {
            _CancelSearchBtn.IsVisible = true;
            _SearchQuery.Placeholder = "";

            Thickness t = (Thickness)Resources["SearchBarPadding"];
            t.Right = 0;
            _SearchBar.Padding = t;
        }

        public void SearchQuery_Unfocused(object sender, Xamarin.Forms.FocusEventArgs e)
        {
            _CancelSearchBtn.IsVisible = false;
            _SearchQuery.Placeholder = T.Localized("SearchQueryPlaceholder");
            _SearchBar.Padding = (Thickness)Resources["SearchBarPadding"];
        }

        public abstract void SearchQueryChanged(string text);

        public void SearchQuery_TextChanged(object sender, Xamarin.Forms.TextChangedEventArgs e)
        {
            SearchQueryChanged((sender as Entry).Text);
        }

        public void CancelSearchBtn_Clicked(object sender, System.EventArgs e)
        {
            _SearchQuery.Text = "";
            _SearchQuery.Unfocus();
        }


		//

		protected async void SortBtn_Clicked(string title, ItemWithItems item, ListView ListCtl, bool detailView = false)
		{
			if (item == null)
				return;

			SortParametersDlg dlg = new SortParametersDlg(T.Localized("HowToSort") + " " + title + "?", item.SortParams);
			if (await MainWnd.Current.ModalManager.DisplayModalAsync(dlg, MainWnd.Current.IsSplitView && !detailView ? ModalViewsManager.ModalPosition.BottomLeft : ModalViewsManager.ModalPosition.BottomCenter))
			{
				item.SortParams = dlg.SortParams;
				item.SortItems();
				Device.BeginInvokeOnMainThread(() => BaseView.ListViewScrollToFirst(ListCtl));
			}
		}
		

        //

		public static void ListViewScrollTo(ListView ListCtl, Item item)
        {
            ListCtl.ScrollTo(item, ScrollToPosition.Center, false);
        }

        public static void ListViewScrollToFirst(ListView ListCtl)
        {
            IEnumerator e = ListCtl.ItemsSource.GetEnumerator();
            e.Reset();
            object o = null;
            if (e.MoveNext())
                o = e.Current;
            if (o != null)
                ListCtl.ScrollTo(o, ScrollToPosition.Start, false);
        }

        public static void ListViewSetItemsSource(ListView ListCtl, IEnumerable l)
        {
            if (l == null)
            {
                ListCtl.ItemsSource = l;
                return;
            }

            ListCtl.BeginRefresh();

            IEnumerator e = l.GetEnumerator();
            e.Reset();
            object o = null;
            if (e.MoveNext())
                o = e.Current;

            ListCtl.ItemsSource = l;

            if (o != null)
                ListCtl.ScrollTo(o, ScrollToPosition.Start, false);

            ListCtl.EndRefresh();
        }
    }
}

